using Microsoft.EntityFrameworkCore;
using Pharmacy.Database.Entities;
using Pharmacy.Database.Repositories.Interfaces;
using Pharmacy.DateTimeProvider;
using Pharmacy.Extensions;
using Pharmacy.ExternalServices;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto;
using Pharmacy.Shared.Enums;
using Pharmacy.Shared.Result;

namespace Pharmacy.Services;

public class OrderService : IOrderService
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentService _paymentService;
    private readonly IUserRepository _userRepository;
    private readonly IEmailSender _emailSender;
    private readonly IDateTimeProvider _dateTimeProvider;

    public OrderService(ICartRepository cartRepository, IProductRepository productRepository, IOrderRepository orderRepository, IDateTimeProvider dateTimeProvider, IPaymentService paymentService, IUserRepository userRepository, IEmailSender emailSender)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _dateTimeProvider = dateTimeProvider;
        _paymentService = paymentService;
        _userRepository = userRepository;
        _emailSender = emailSender;
    }

    public async Task<Result<CreatedDto>> CreateAsync(int userId, string deliveryAddress, PaymentMethodEnum paymentMethod)
    {
        var cartItems = await _cartRepository.GetRawUserCartAsync(userId);
        if (!cartItems.Any())
            return Result.Failure<CreatedDto>(Error.Failure("Корзина пуста"));

        var now = _dateTimeProvider.UtcNow;
        var orderItems = new List<OrderItem>();
        decimal totalPrice = 0;

        foreach (var cartItem in cartItems)
        {
            var product = await _productRepository.GetByIdWithRelationsAsync(cartItem.ProductId);
            if (product is null || !product.IsAvailable || product.StockQuantity < cartItem.Quantity)
            {
                return Result.Failure<CreatedDto>(Error.Failure($"Товар {cartItem.ProductId} недоступен или недостаточно на складе"));
            }

            orderItems.Add(new OrderItem
            {
                ProductId = product.Id,
                Quantity = cartItem.Quantity,
                Price = product.Price
            });

            totalPrice += cartItem.Quantity * product.Price;
        }

        var order = new Order
        {
            UserId = userId,
            Number = $"ORD-{Guid.NewGuid().ToString()[..8].ToUpper()}",
            TotalPrice = totalPrice,
            StatusId = paymentMethod == PaymentMethodEnum.Online ? (int)OrderStatusEnum.WaitingForPayment : (int)OrderStatusEnum.Pending,
            CreatedAt = now,
            UpdatedAt = now,
            OrderItems = orderItems
        };

        await _orderRepository.AddAsync(order);
        await _cartRepository.RemoveRangeAsync(cartItems);
        await _paymentService.CreateInitialPaymentAsync(order.Id, totalPrice, paymentMethod);

        var user = await _userRepository.GetByIdAsync(userId);
        if (user is not null)
        {
            var subject = $"Заказ {order.Number} успешно оформлен";
            var body = $@"
                <div style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 10px;"">
                    <h2 style=""color: #2c3e50;"">Здравствуйте, {user.LastName} {user.FirstName}!</h2>
                    <p style=""font-size: 16px; color: #333;"">
                        Ваш заказ <strong>{order.Number}</strong> на сумму <strong>{totalPrice:C}</strong> успешно оформлен.
                    </p>
                    <p style=""font-size: 16px; color: #555;"">
                        Текущий статус заказа: 
                        <strong>{(paymentMethod == PaymentMethodEnum.Online ? "Ожидает оплаты" : "Ожидает обработки")}</strong>.
                    </p>
                    <p style=""font-size: 14px; color: #888;"">
                        Спасибо, что выбрали нашу аптеку!
                    </p>
                </div>";
            await _emailSender.SendEmailAsync(user.Email, subject, body);
        }
        
        return Result.Success(new CreatedDto(order.Id));
    }

    public async Task<Result> PayAsync(int orderId, int userId)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId, includePayment: true);
        if (order is null || order.UserId != userId)
        {
            return Result.Failure(Error.NotFound("Заказ не найден"));
        }

        var payment = order.Payment;
        if (payment is null || (PaymentStatusEnum)payment.StatusId != PaymentStatusEnum.Pending)
        {
            return Result.Failure(Error.Failure("Платёж не ожидает оплаты"));
        }

        if ((PaymentMethodEnum)payment.PaymentMethodId != PaymentMethodEnum.Online)
        {
            return Result.Failure(Error.Failure("Оплата не требуется"));
        }

        payment.StatusId = (int)PaymentStatusEnum.Completed;
        payment.TransactionDate = _dateTimeProvider.UtcNow;
        payment.UpdatedAt = _dateTimeProvider.UtcNow;

        order.StatusId = (int)OrderStatusEnum.Pending;
        order.UpdatedAt = _dateTimeProvider.UtcNow;

        await _orderRepository.UpdateAsync(order);
        return Result.Success();
    }
    
    public async Task<Result<OrderDetailsDto>> GetByIdAsync(int orderId, int currentUserId, UserRoleEnum role)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId, true, true, true, true, true);
        if (order is null)
        {
            return Result.Failure<OrderDetailsDto>(Error.NotFound("Заказ не найден"));
        }

        if (role == UserRoleEnum.User && order.UserId != currentUserId)
        {
            return Result.Failure<OrderDetailsDto>(Error.Failure("Нет доступа к заказу"));
        }

        return Result.Success(new OrderDetailsDto(
            order.Id,
            order.Number,
            order.CreatedAt,
            order.TotalPrice,
            order.Status.Description,
            order.PickupCode,
            order.UserId,
            $"{order.User.LastName} {order.User.FirstName} {order.User.Patronymic}",
            order.User.Email,
            order.OrderItems.Select(oi => new OrderItemDto(
                oi.ProductId,
                oi.Product.Name,
                oi.Quantity,
                oi.Price,
                oi.Product.Images.OrderBy(i => i.Id).Select(i => i.Url).FirstOrDefault()
            )).ToList(),
            new PaymentDto(
                order.Payment.Id,
                ((PaymentMethodEnum)order.Payment.PaymentMethodId).GetDescription(),
                order.Payment.Amount,
                ((PaymentStatusEnum)order.Payment.StatusId).GetDescription(),
                order.Payment.TransactionDate
            )
        ));
    }
    
    public async Task<Result<PaginatedList<OrderDto>>> GetPaginatedAsync(OrderFilters filters, int pageNumber, int pageSize, int? userId = null)
    {
        var query = _orderRepository.QueryWithStatus();

        if (userId.HasValue)
        {
            query = query.Where(o => o.UserId == userId.Value);
        }
        
        if (filters.UserId.HasValue && userId == null)
        {
            query = query.Where(o => o.UserId == filters.UserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filters.UserEmail) && userId == null)
        {
            query = query.Where(o => o.User.Email.Contains(filters.UserEmail));
        }

        if (!string.IsNullOrWhiteSpace(filters.UserFullName) && userId == null)
        {
            var name = filters.UserFullName.ToLower();
            query = query.Where(o => (o.User.LastName + " " + o.User.FirstName + (o.User.Patronymic != null ? " " + o.User.Patronymic : "")).ToLower().Contains(name));
        }
        
        if (!string.IsNullOrWhiteSpace(filters.Number))
        {
            query = query.Where(o => o.Number.Contains(filters.Number));
        }

        if (filters.Status.HasValue)
        {
            query = query.Where(o => o.StatusId == (int)filters.Status);
        }

        if (filters.FromDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= filters.FromDate);
        }

        if (filters.ToDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt <= filters.ToDate);
        }

        var totalCount = await query.CountAsync();

        query = filters.SortBy switch
        {
            "number" => filters.SortOrder == "asc" ? query.OrderBy(o => o.Number) : query.OrderByDescending(o => o.Number),
            "price" => filters.SortOrder == "asc" ? query.OrderBy(o => o.TotalPrice) : query.OrderByDescending(o => o.TotalPrice),
            "date" or _ => filters.SortOrder == "asc" ? query.OrderBy(o => o.CreatedAt) : query.OrderByDescending(o => o.CreatedAt)
        };

        var pageItems = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new OrderDto(
                o.Id,
                o.Number,
                o.CreatedAt,
                o.TotalPrice,
                o.Status.Name,
                o.PickupCode,
                o.UserId,
                $"{o.User.LastName} {o.User.FirstName} {o.User.Patronymic}",
                o.User.Email))
            .ToListAsync();

        return Result.Success(new PaginatedList<OrderDto>(pageItems, totalCount, pageNumber, pageSize));
    }
    
    public async Task<Result> UpdateStatusAsync(int orderId, OrderStatusEnum newStatus)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId);
        if (order is null)
        {
            return Result.Failure(Error.NotFound("Заказ не найден"));
        }

        var user = await _userRepository.GetByIdAsync(order.UserId);
        if (user is null)
        {
            return Result.Failure(Error.NotFound("Пользователь не найден"));
        }
        
        if (order.StatusId == (int)newStatus)
        {
            return Result.Failure(Error.Failure("Статус уже установлен"));
        }

        order.StatusId = (int)newStatus;
        order.UpdatedAt = _dateTimeProvider.UtcNow;

        if (newStatus == OrderStatusEnum.ReadyForReceive)
        {
            order.PickupCode = new Random().Next(1000, 9999).ToString();
            var subject = $"Ваш заказ {order.Number} готов к получению";
            var body = $@"
                <div style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 10px;"">
                    <h2 style=""color: #2c3e50;"">Здравствуйте, {user.LastName} {user.FirstName}!</h2>
                    <p style=""font-size: 16px; color: #333;"">
                        Ваш заказ <strong>{order.Number}</strong> теперь готов к получению в аптеке.
                    </p>
                    <p style=""font-size: 18px; color: #000; margin-top: 20px;"">
                        <strong>Код для получения:</strong>
                    </p>
                    <div style=""font-size: 28px; font-weight: bold; background-color: #f2f2f2; padding: 15px; text-align: center; border-radius: 6px; margin-bottom: 20px;"">
                        {order.PickupCode}
                    </div>
                    <p style=""font-size: 14px; color: #888;"">
                        Покажите этот код сотруднику при получении заказа.
                    </p>
                </div>";
            await _emailSender.SendEmailAsync(user.Email, subject, body);
        }
        
        await _orderRepository.UpdateAsync(order);
        
        return Result.Success();
    }

    public async Task<Result> CancelAsync(int userId, int orderId)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId, includePayment: true);
        if (order is null || order.UserId != userId)
        {
            return Result.Failure(Error.NotFound("Заказ не найден или нет доступа"));
        }

        if (order.StatusId != (int)OrderStatusEnum.Pending && order.StatusId != (int)OrderStatusEnum.WaitingForPayment)
        {
            return Result.Failure(Error.Failure("Отменить можно только заказы в статусе 'Ожидает обработки' или 'Ожидает оплаты'"));
        }
        
        if (order.Payment.StatusId == (int)PaymentStatusEnum.Completed)
        {
            return Result.Failure(Error.Failure("Нельзя отменить уже оплаченный заказ"));
        }

        if (order.Payment.StatusId == (int)PaymentStatusEnum.Pending || order.Payment.StatusId == (int)PaymentStatusEnum.NotPaid)
        {
            order.Payment.StatusId = (int)PaymentStatusEnum.Cancelled;
            order.Payment.UpdatedAt = _dateTimeProvider.UtcNow;
        }

        order.StatusId = (int)OrderStatusEnum.Cancelled;
        order.UpdatedAt = _dateTimeProvider.UtcNow;
        await _orderRepository.UpdateAsync(order);
        return Result.Success();
    }

    public async Task<Result> RefundAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId, includePayment: true);
        if (order is null)
        {
            return Result.Failure(Error.NotFound("Заказ не найден"));
        }

        var payment = order.Payment;
        if (payment is null || (PaymentStatusEnum)payment.StatusId != PaymentStatusEnum.Completed)
        {
            return Result.Failure(Error.Failure("Возврат возможен только для оплаченных заказов"));
        }

        payment.StatusId = (int)PaymentStatusEnum.Refunded;
        payment.UpdatedAt = _dateTimeProvider.UtcNow;
        payment.TransactionDate = _dateTimeProvider.UtcNow;

        order.StatusId = (int)OrderStatusEnum.Refunded;
        order.UpdatedAt = _dateTimeProvider.UtcNow;

        await _orderRepository.UpdateAsync(order);
        return Result.Success();
    }
    
    public async Task<IEnumerable<OrderStatusDto>> GetAllStatusesAsync()
    {
        var statuses = await _orderRepository.GetAllOrderStatuses();
        return statuses.Select(s => new OrderStatusDto(s.Id, s.Name, s.Description));
    }
}