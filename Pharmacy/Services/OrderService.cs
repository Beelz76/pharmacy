using Microsoft.EntityFrameworkCore;
using Pharmacy.Database;
using Pharmacy.Database.Entities;
using Pharmacy.Database.Repositories.Interfaces;
using Pharmacy.DateTimeProvider;
using Pharmacy.Endpoints.Orders;
using Pharmacy.Extensions;
using Pharmacy.ExternalServices;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto;
using Pharmacy.Shared.Dto.Delivery;
using Pharmacy.Shared.Dto.Order;
using Pharmacy.Shared.Dto.Payment;
using Pharmacy.Shared.Enums;
using Pharmacy.Shared.Result;

namespace Pharmacy.Services;

public class OrderService : IOrderService
{
    private readonly ICartRepository _cartRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentService _paymentService;
    private readonly IUserRepository _userRepository;
    private readonly IEmailSender _emailSender;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IStorageProvider _storage;
    private readonly YooKassaHttpClient _yooKassaClient;
    private readonly IPharmacyService _pharmacyService;
    private readonly IPharmacyProductService _pharmacyProductService;
    private readonly IDeliveryService _deliveryService;
    private readonly IUserAddressRepository _userAddressRepository;
    private readonly TransactionRunner _transactionRunner;

    private static readonly Dictionary<OrderStatusEnum, OrderStatusEnum[]> _allowedTransitions = new()
    {
        { OrderStatusEnum.WaitingForPayment, new[] { OrderStatusEnum.Pending, OrderStatusEnum.Cancelled } },
        { OrderStatusEnum.Pending, new[] { OrderStatusEnum.Processing, OrderStatusEnum.Cancelled } },
        { OrderStatusEnum.Processing, new[] { OrderStatusEnum.ReadyForReceive, OrderStatusEnum.OutForDelivery } },
        { OrderStatusEnum.ReadyForReceive, new[] { OrderStatusEnum.Received, OrderStatusEnum.Cancelled } },
        { OrderStatusEnum.OutForDelivery, new[] { OrderStatusEnum.Delivered, OrderStatusEnum.Cancelled } },
        { OrderStatusEnum.Delivered, new[] { OrderStatusEnum.Refunded } },
        { OrderStatusEnum.Received, new[] { OrderStatusEnum.Refunded } },
    };
    
    public OrderService(ICartRepository cartRepository,
        IOrderRepository orderRepository, IDateTimeProvider dateTimeProvider, IPaymentService paymentService,
        IUserRepository userRepository, IEmailSender emailSender, IStorageProvider storage,
        YooKassaHttpClient yooKassaClient, IPharmacyService pharmacyService,
        IPharmacyProductService pharmacyProductService, IDeliveryService deliveryService,
        IUserAddressRepository userAddressRepository, TransactionRunner transactionRunner)
    {
        _cartRepository = cartRepository;
        _orderRepository = orderRepository;
        _dateTimeProvider = dateTimeProvider;
        _paymentService = paymentService;
        _userRepository = userRepository;
        _emailSender = emailSender;
        _storage = storage;
        _yooKassaClient = yooKassaClient;
        _pharmacyService = pharmacyService;
        _pharmacyProductService = pharmacyProductService;
        _deliveryService = deliveryService;
        _userAddressRepository = userAddressRepository;
        _transactionRunner = transactionRunner;
    }

    public async Task<Result<CreatedWithNumberDto>> CreateAsync(int userId, CreateOrderRequest request)
    {
        var cartItems = await _cartRepository.GetRawUserCartAsync(userId);
        if (!cartItems.Any())
        {
            return Result.Failure<CreatedWithNumberDto>(Error.Failure("Корзина пуста"));
        }

        if (request.IsDelivery && !request.UserAddressId.HasValue)
        {
            return Result.Failure<CreatedWithNumberDto>(Error.Failure("Адрес не указан"));
        }

        var now = _dateTimeProvider.UtcNow;
        var productTuples = cartItems.Select(c => (c.ProductId, c.Quantity)).ToList();

        // Получить или создать аптеку
        int pharmacyId;
        if (request.PharmacyId.HasValue)
        {
            pharmacyId = request.PharmacyId.Value;
        }
        else if (request.NewPharmacy is not null)
        {
            var pharmacyResult = await _pharmacyService.GetOrCreatePharmacyIdAsync(request.NewPharmacy);
            if (pharmacyResult.IsFailure) return Result.Failure<CreatedWithNumberDto>(pharmacyResult.Error);
            pharmacyId = pharmacyResult.Value;
        }
        else if (request.IsDelivery)
        {
            var userAddress = await _userAddressRepository.GetByIdAsync(userId, request.UserAddressId!.Value);
            if (userAddress == null)
                return Result.Failure<CreatedWithNumberDto>(Error.Failure("Адрес пользователя не найден"));

            var nearestResult =
                await _pharmacyService.GetNearestPharmacyIdAsync(userAddress.Address.Latitude,
                    userAddress.Address.Longitude);
            if (nearestResult.Value == null)
                return Result.Failure<CreatedWithNumberDto>(Error.Failure("Нет аптек поблизости"));

            pharmacyId = nearestResult.Value.Value;
        }
        else
        {
            return Result.Failure<CreatedWithNumberDto>(Error.Failure("Аптека не указана"));
        }
        
        var orderItemDetails = new List<(string Name, int Quantity, decimal Price)>();
        decimal totalPrice = 0;

        var transactionResult = await _transactionRunner.ExecuteAsync(async () =>
        {
            var pharmacyProductsResult =
                await _pharmacyProductService.ValidateOrAddProductsAsync(pharmacyId, productTuples);
            if (pharmacyProductsResult.IsFailure)
                return Result.Failure<CreatedWithNumberDto>(pharmacyProductsResult.Error);

            var pharmacyProducts = pharmacyProductsResult.Value;

            var orderItems = new List<OrderItem>();

            foreach (var cartItem in cartItems)
            {
                var pharmacyProduct = pharmacyProducts.First(x => x.ProductId == cartItem.ProductId);
                orderItems.Add(new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    Price = pharmacyProduct.Price
                });

                orderItemDetails.Add((pharmacyProduct.ProductName, cartItem.Quantity, pharmacyProduct.Price));
                totalPrice += cartItem.Quantity * pharmacyProduct.Price;
            }
            
            var order = new Order
            {
                UserId = userId,
                Number = $"ORD-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                TotalPrice = totalPrice,
                StatusId = request.PaymentMethod == PaymentMethodEnum.Online ? (int)OrderStatusEnum.WaitingForPayment : (int)OrderStatusEnum.Pending,
                CreatedAt = now,
                UpdatedAt = now,
                OrderItems = orderItems,
                IsDelivery = request.IsDelivery,
                PharmacyId = pharmacyId,
                ExpiresAt = request.PaymentMethod == PaymentMethodEnum.Online ? now.AddMinutes(30) : null
            };

            await _orderRepository.AddAsync(order);

            foreach (var item in orderItems)
            {
                var currentStock = pharmacyProducts.First(p => p.ProductId == item.ProductId).StockQuantity;
                await _pharmacyProductService.UpdateStockQuantityAsync(pharmacyId, item.ProductId, currentStock - item.Quantity);
            }

            await _cartRepository.RemoveRangeAsync(cartItems);

            await _paymentService.CreateInitialPaymentAsync(order.Id, totalPrice, request.PaymentMethod);

            if (request.IsDelivery && request.UserAddressId.HasValue)
            {
                await _deliveryService.CreateAsync(new CreateDeliveryRequest(
                    order.Id,
                    request.UserAddressId.Value,
                    request.DeliveryComment,
                    null));
            }

            return Result.Success(new CreatedWithNumberDto(order.Id, order.Number));
        });

        if (transactionResult.IsFailure)
            return transactionResult;

        var created = transactionResult.Value;
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is not null)
        {
            string pharmacyName = (await _pharmacyService.GetByIdAsync(pharmacyId)).Value?.Name ?? "неизвестно";
            var addressString = request.IsDelivery && request.UserAddressId.HasValue
                ? AddressExtensions.FormatAddress(await _userAddressRepository.GetByIdAsync(userId, request.UserAddressId.Value))
                : $"Аптека: {pharmacyName}";

            var itemsTable = string.Join("", orderItemDetails.Select(item =>
                $"<tr><td>{item.Name}</td><td>{item.Quantity}</td><td>{item.Price:C}</td><td>{item.Quantity * item.Price:C}</td></tr>"));

            var body = $@"<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 10px;'>
                    <h2 style='color: #2c3e50;'>Здравствуйте, {user.LastName} {user.FirstName}!</h2>
                    <p style='font-size: 16px; color: #333;'>Ваш заказ <strong>{created.Number}</strong> оформлен.</p>
                    <p style='font-size: 16px; color: #333;'>Состав заказа:</p>
                    <table border='1' cellpadding='4' style='width:100%;border-collapse:collapse;'>
                        <tr><th>Товар</th><th>Кол-во</th><th>Цена</th><th>Сумма</th></tr>
                        {itemsTable}
                    </table>
                    <p style='font-size: 16px; color: #333;'><strong>Итого:</strong> {totalPrice:C}<br/>
                    <strong>Адрес:</strong> {addressString}<br/>
                    <strong>Оплата:</strong> {(request.PaymentMethod == PaymentMethodEnum.Online ? "Онлайн" : "При получении")}</p>
                </div>";

            await _emailSender.SendEmailAsync(user.Email, $"Заказ {created.Number}", body);
        }

        return Result.Success(created);
    }

    
    public async Task<Result<string>> PayAsync(int orderId, int userId)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId, includePayment: true);
        if (order is null || order.UserId != userId)
        {
            return Result.Failure<string>(Error.NotFound("Заказ не найден"));
        }

        var payment = order.Payment;
        if (payment is null || (PaymentStatusEnum)payment.StatusId != PaymentStatusEnum.Pending)
        {
            return Result.Failure<string>(Error.Failure("Платёж не ожидает оплаты"));
        }

        if ((PaymentMethodEnum)payment.PaymentMethodId != PaymentMethodEnum.Online)
        {
            return Result.Failure<string>(Error.Failure("Оплата не требуется"));
        }

        var paymentRequest = new YooKassaCreatePaymentRequest
        {
            Amount = new YooKassaAmount
            {
                Value = payment.Amount,
                Currency = "RUB"
            },
            Confirmation = new YooKassaConfirmation
            {
                Type = "redirect",
                ReturnUrl = $"http://localhost:5173/account/orders/{orderId}"
            },
            Capture = true,
            Description = $"Оплата заказа №{order.Number}",
            Metadata = new Dictionary<string, string>
            {
                { "order_id", order.Id.ToString() }
            }
        };

        var idempotenceKey = Guid.NewGuid().ToString();
        
        var createResult = await _yooKassaClient.CreatePaymentAsync(paymentRequest, idempotenceKey);
        if (!createResult.IsSuccess)
        {
            return Result.Failure<string>(createResult.Error);
        }

        //Вместо коллбека
        payment.StatusId = (int)PaymentStatusEnum.Completed;
        payment.TransactionDate = _dateTimeProvider.UtcNow;
        payment.UpdatedAt = _dateTimeProvider.UtcNow;
        payment.ExternalPaymentId = createResult.Value.Id;

        order.StatusId = (int)OrderStatusEnum.Pending;
        order.UpdatedAt = _dateTimeProvider.UtcNow;

        await _orderRepository.UpdateAsync(order);
        return Result.Success(createResult.Value.Confirmation.ConfirmationUrl);
    }
    
    public async Task<Result<OrderDetailsDto>> GetByIdAsync(int orderId, int currentUserId, UserRoleEnum role, int? pharmacyId = null)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId,
            includeItems: true,
            includeStatus: true,
            includeProductImages: true,
            includePayment: true,
            includeUser: true,
            includePharmacy: true,
            includeDelivery: true);
        
        if (order is null)
        {
            return Result.Failure<OrderDetailsDto>(Error.NotFound("Заказ не найден"));
        }

        if (role == UserRoleEnum.User && order.UserId != currentUserId)
        {
            return Result.Failure<OrderDetailsDto>(Error.Failure("Нет доступа к заказу"));
        }
        
        if (role == UserRoleEnum.Employee && pharmacyId.HasValue && order.PharmacyId != pharmacyId.Value)
        {
            return Result.Failure<OrderDetailsDto>(Error.Forbidden("Нет доступа к заказу"));
        }

        return Result.Success(new OrderDetailsDto(
            order.Id,
            order.Number,
            order.CreatedAt,
            order.TotalPrice,
            order.Status.Description,
            order.PickupCode,
            order.Pharmacy.Name,
            AddressExtensions.FormatAddress(order.Pharmacy.Address)!,
            order.PharmacyId,
            order.IsDelivery ? AddressExtensions.FormatAddress(order.Delivery?.UserAddress) : null,
            order.IsDelivery,
            order.UserId,
            $"{order.User.LastName} {order.User.FirstName} {order.User.Patronymic}".Trim(),
            order.User.Email,
            order.OrderItems.Select(oi => new OrderItemDto(
                oi.ProductId,
                oi.Product.Name,
                oi.Quantity,
                oi.Price,
                oi.Product.Images.OrderBy(i => i.Id).Select(i => _storage.GetPublicUrl(i.Url)).FirstOrDefault()
            )).ToList(),
            new PaymentDto(
                order.Payment.Id,
                ((PaymentMethodEnum)order.Payment.PaymentMethodId).GetDescription(),
                order.Payment.Amount,
                ((PaymentStatusEnum)order.Payment.StatusId).GetDescription(),
                order.Payment.TransactionDate,
                order.Payment.ExternalPaymentId
            )
        ));
    }
    
    public async Task<Result<PaginatedList<OrderDto>>> GetPaginatedAsync(OrderFilters filters, int pageNumber, int pageSize, string? sortBy, string? sortOrder, int? userId = null)
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
            query = query.Where(o => ((o.User.LastName ?? "") + " " + (o.User.FirstName ?? "") + " " + (o.User.Patronymic ?? ""))
                .Trim().ToLower().Contains(name));
        }
        
        if (!string.IsNullOrWhiteSpace(filters.PharmacyCity) && userId == null)
        {
            query = query.Where(o => o.Pharmacy.Address.City.ToLower().Contains(filters.PharmacyCity.ToLower()));
        }
        
        if (filters.PharmacyId.HasValue && userId == null)
        {
            query = query.Where(o => o.PharmacyId == filters.PharmacyId.Value);
        }
        
        if (!string.IsNullOrWhiteSpace(filters.PharmacyName) && userId == null)
        {
            query = query.Where(o => o.Pharmacy.Name.ToLower().Contains(filters.PharmacyName.ToLower()));
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
        
        if (filters.FromPrice.HasValue)
        {
            query = query.Where(o => o.TotalPrice >= filters.FromPrice.Value);
        }

        if (filters.ToPrice.HasValue)
        {
            query = query.Where(o => o.TotalPrice <= filters.ToPrice.Value);
        }

        var totalCount = await query.CountAsync();

        query = sortBy switch
        {
            "number" => sortOrder == "asc" ? query.OrderBy(o => o.Number) : query.OrderByDescending(o => o.Number),
            "price" => sortOrder == "asc" ? query.OrderBy(o => o.TotalPrice) : query.OrderByDescending(o => o.TotalPrice),
            "date" or _ => sortOrder == "asc" ? query.OrderBy(o => o.CreatedAt) : query.OrderByDescending(o => o.CreatedAt)
        };

        var pageItems = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new OrderDto(
                o.Id,
                o.Number,
                o.CreatedAt,
                o.TotalPrice,
                o.Status.Description,
                o.PickupCode,
                o.PharmacyId,
                o.Pharmacy.Name,
                AddressExtensions.FormatAddress(o.Pharmacy.Address)!,
                o.UserId,
                $"{o.User.LastName} {o.User.FirstName} {o.User.Patronymic}".Trim(),
                o.User.Email))
            .ToListAsync();

        return Result.Success(new PaginatedList<OrderDto>(pageItems, totalCount, pageNumber, pageSize));
    }
    
    public async Task<Result> UpdateStatusAsync(int orderId, OrderStatusEnum newStatus, int? pharmacyId = null)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId, includePayment: true, includePharmacy: true);
        if (order is null)
        {
            return Result.Failure(Error.NotFound("Заказ не найден"));
        }

        if (pharmacyId.HasValue && order.PharmacyId != pharmacyId.Value)
        {
            return Result.Failure(Error.Forbidden("Нет доступа к заказу"));
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

        if (!_allowedTransitions.TryGetValue((OrderStatusEnum)order.StatusId, out var allowed) || !allowed.Contains(newStatus))
        {
            return Result.Failure(Error.Failure("Недопустимый переход статуса"));
        }
        
        order.StatusId = (int)newStatus;
        order.UpdatedAt = _dateTimeProvider.UtcNow;

        if (newStatus == OrderStatusEnum.ReadyForReceive && !order.IsDelivery)
        {
            order.PickupCode = new Random().Next(1000, 9999).ToString();
            var subject = $"Ваш заказ {order.Number} готов к получению";
            var body = $@"
                <div style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 10px;"">
                    <h2 style=""color: #2c3e50;"">Здравствуйте, {user.LastName} {user.FirstName}!</h2>
                    <p style=""font-size: 16px; color: #333;"">
                        Ваш заказ <strong>{order.Number}</strong> теперь готов к получению в аптеке.
                    </p>
                    <p style=\""font-size: 14px; color: #888;\"">
                        Адрес аптеки: <strong>{AddressExtensions.FormatAddress(order.Pharmacy!.Address)}</strong>
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

        if (newStatus == OrderStatusEnum.Received && (PaymentMethodEnum)order.Payment.PaymentMethodId == PaymentMethodEnum.OnDelivery)
        {
            await _paymentService.UpdateStatusAsync(order.Id, PaymentStatusEnum.Completed);
        }
        
        await _orderRepository.UpdateAsync(order);
        
        return Result.Success();
    }

    public async Task<Result> CancelAsync(int userId, int orderId, string? comment = null)
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

        var transactionResult = await _transactionRunner.ExecuteAsync(async () =>
        {
            if (order.Payment.StatusId == (int)PaymentStatusEnum.Pending || order.Payment.StatusId == (int)PaymentStatusEnum.NotPaid)
            {
                order.Payment.StatusId = (int)PaymentStatusEnum.Cancelled;
                order.Payment.UpdatedAt = _dateTimeProvider.UtcNow;
            }

            foreach (var item in order.OrderItems)
            {
                var pharmacyProduct = await _pharmacyProductService.GetAsync(order.PharmacyId, item.ProductId);
                if (pharmacyProduct != null)
                {
                    await _pharmacyProductService.UpdateStockQuantityAsync(order.PharmacyId, item.ProductId, pharmacyProduct.StockQuantity + item.Quantity);
                }
            }

            order.StatusId = (int)OrderStatusEnum.Cancelled;
            order.UpdatedAt = _dateTimeProvider.UtcNow;
            order.CancellationComment = comment;
            await _orderRepository.UpdateAsync(order);
            return Result.Success();
        });

        if (transactionResult.IsFailure)
            return transactionResult;
        
        var user = await _userRepository.GetByIdAsync(order.UserId);
        if (user != null)
        {
            var subject = $"Заказ {order.Number} отменен";
            var body = $@"<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 10px;'>
                    <h2 style='color: #2c3e50;'>Здравствуйте, {user.LastName} {user.FirstName}!</h2>
                    <p style='font-size: 16px; color: #333;'>Ваш заказ <strong>{order.Number}</strong> был отменен.</p> 
                    {(string.IsNullOrWhiteSpace(comment) ? "" : $"<p>Причина: {comment}</p>")}
                </div>";
            await _emailSender.SendEmailAsync(user.Email, subject, body);
        }
        
        return Result.Success();
    }
    
    public async Task<Result> RefundAsync(int orderId, int? pharmacyId = null)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId, includePayment: true);
        if (order is null)
        {
            return Result.Failure(Error.NotFound("Заказ не найден"));
        }

        if (pharmacyId.HasValue && order.PharmacyId != pharmacyId.Value)
        {
            return Result.Failure(Error.Forbidden("Нет доступа к заказу"));
        }
        
        var payment = order.Payment;
        if (payment is null || (PaymentStatusEnum)payment.StatusId != PaymentStatusEnum.Completed)
        {
            return Result.Failure(Error.Failure("Возврат возможен только для оплаченных заказов"));
        }

        var transactionResult = await _transactionRunner.ExecuteAsync(async () =>
        {
            payment.StatusId = (int)PaymentStatusEnum.Refunded;
            payment.UpdatedAt = _dateTimeProvider.UtcNow;
            payment.TransactionDate = _dateTimeProvider.UtcNow;

            order.StatusId = (int)OrderStatusEnum.Refunded;
            order.UpdatedAt = _dateTimeProvider.UtcNow;

            foreach (var item in order.OrderItems)
            {
                var pharmacyProduct = await _pharmacyProductService.GetAsync(order.PharmacyId, item.ProductId);
                if (pharmacyProduct != null)
                {
                    await _pharmacyProductService.UpdateStockQuantityAsync(order.PharmacyId, item.ProductId, pharmacyProduct.StockQuantity + item.Quantity);
                }
            }

            await _orderRepository.UpdateAsync(order);
            return Result.Success();
        });

        if (transactionResult.IsFailure)
            return transactionResult;
        
        var user = await _userRepository.GetByIdAsync(order.UserId);
        if (user != null)
        {
            var subject = $"Заказ {order.Number} возвращен";
            var body = $@"<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 10px;'>
                    <h2 style='color: #2c3e50;'>Здравствуйте, {user.LastName} {user.FirstName}!</h2>
                    <p style='font-size: 16px; color: #333;'>За заказ <strong>{order.Number}</strong> оформлен возврат средств.</p>
                </div>";
            await _emailSender.SendEmailAsync(user.Email, subject, body);
        }
        
        return Result.Success();
    }
    
    public async Task<IEnumerable<OrderStatusDto>> GetAllStatusesAsync()
    {
        var statuses = await _orderRepository.GetAllOrderStatuses();
        return statuses.Select(s => new OrderStatusDto(s.Id, s.Name, s.Description));
    }

    public async Task<Result> MarkAsDeliveredAsync(int orderId, int? pharmacyId = null)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId, includePayment: true, includePharmacy: true);
        if (order is null)
        {
            return Result.Failure(Error.NotFound("Заказ не найден"));
        }

        if (pharmacyId.HasValue && order.PharmacyId != pharmacyId.Value)
        {
            return Result.Failure(Error.Forbidden("Нет доступа к заказу"));
        }
        
        if (order.StatusId == (int)OrderStatusEnum.Delivered)
        {
            return Result.Failure(Error.Failure("Заказ уже отмечен как доставленный"));
        }

        var transactionResult = await _transactionRunner.ExecuteAsync(async () =>
        {
            order.StatusId = (int)OrderStatusEnum.Delivered;
            order.UpdatedAt = _dateTimeProvider.UtcNow;

            if ((PaymentMethodEnum)order.Payment.PaymentMethodId == PaymentMethodEnum.OnDelivery && (PaymentStatusEnum)order.Payment.StatusId != PaymentStatusEnum.Completed)
            {
                await _paymentService.UpdateStatusAsync(order.Id, PaymentStatusEnum.Completed);
            }

            await _orderRepository.UpdateAsync(order);
            return Result.Success();
        });

        if (transactionResult.IsFailure)
            return transactionResult;
        
        var user = await _userRepository.GetByIdAsync(order.UserId);
        if (user != null)
        {
            var subject = $"Ваш заказ {order.Number} доставлен";
            var body = $@"
            <div style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 10px;"">
                <h2 style=""color: #2c3e50;"">Здравствуйте, {user.LastName} {user.FirstName}!</h2>
                <p style=""font-size: 16px; color: #333;"">
                    Ваш заказ <strong>{order.Number}</strong> успешно доставлен.
                </p>
                <p style=""font-size: 14px; color: #888;"">
                    Спасибо, что выбрали нашу аптеку!
                </p>
            </div>";
            await _emailSender.SendEmailAsync(user.Email, subject, body);
        }

        return Result.Success();
    }

    public async Task<Result> CancelByStaffAsync(int orderId, string? comment = null, int? pharmacyId = null)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId, includePayment: true);
        if (order is null)
        {
            return Result.Failure(Error.NotFound("Заказ не найден"));
        }

        if (pharmacyId.HasValue && order.PharmacyId != pharmacyId.Value)
        {
            return Result.Failure(Error.Forbidden("Нет доступа к заказу"));
        }

        if (order.StatusId != (int)OrderStatusEnum.Pending && order.StatusId != (int)OrderStatusEnum.WaitingForPayment)
        {
            return Result.Failure(Error.Failure("Отменить можно только заказы в статусе 'Ожидает обработки' или 'Ожидает оплаты'"));
        }

        if (order.Payment.StatusId == (int)PaymentStatusEnum.Completed)
        {
            return Result.Failure(Error.Failure("Нельзя отменить уже оплаченный заказ"));
        }

        var transactionResult = await _transactionRunner.ExecuteAsync(async () =>
        {
            if (order.Payment.StatusId == (int)PaymentStatusEnum.Pending || order.Payment.StatusId == (int)PaymentStatusEnum.NotPaid)
            {
                order.Payment.StatusId = (int)PaymentStatusEnum.Cancelled;
                order.Payment.UpdatedAt = _dateTimeProvider.UtcNow;
            }

            foreach (var item in order.OrderItems)
            {
                var pharmacyProduct = await _pharmacyProductService.GetAsync(order.PharmacyId, item.ProductId);
                if (pharmacyProduct != null)
                {
                    await _pharmacyProductService.UpdateStockQuantityAsync(order.PharmacyId, item.ProductId, pharmacyProduct.StockQuantity + item.Quantity);
                }
            }

            order.StatusId = (int)OrderStatusEnum.Cancelled;
            order.UpdatedAt = _dateTimeProvider.UtcNow;
            order.CancellationComment = comment;
            await _orderRepository.UpdateAsync(order);
            return Result.Success();
        });

        if (transactionResult.IsFailure)
            return transactionResult;

        var user = await _userRepository.GetByIdAsync(order.UserId);
        if (user != null)
        {
            var subject = $"Заказ {order.Number} отменен";
            var body = $@"<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 10px;'>
                    <h2 style='color: #2c3e50;'>Здравствуйте, {user.LastName} {user.FirstName}!</h2>
                    <p style='font-size: 16px; color: #333;'>Ваш заказ <strong>{order.Number}</strong> был отменен.</p>
                    {(string.IsNullOrWhiteSpace(comment) ? "" : $"<p>Причина: {comment}</p>")}
                </div>";
            await _emailSender.SendEmailAsync(user.Email, subject, body);
        }

        return Result.Success();
    }
    
}