using Pharmacy.Shared.Dto;
using Pharmacy.Shared.Enums;
using Pharmacy.Shared.Result;

namespace Pharmacy.Services.Interfaces;

public interface IOrderService
{
    Task<Result<CreatedWithNumberDto>> CreateAsync(int userId, string pharmacyAddress, PaymentMethodEnum paymentMethod);
    Task<Result<OrderDetailsDto>> GetByIdAsync(int orderId, int currentUserId, UserRoleEnum role);
    Task<Result<string>> PayAsync(int orderId, int userId);
    Task<Result<PaginatedList<OrderDto>>> GetPaginatedAsync(OrderFilters filters, int pageNumber, int pageSize, string? sortBy, string? sortOrder, int? userId = null);
    Task<Result> UpdateStatusAsync(int orderId, OrderStatusEnum newStatus);
    Task<IEnumerable<OrderStatusDto>> GetAllStatusesAsync();
    Task<Result> RefundAsync(int orderId);
    Task<Result> CancelAsync(int userId, int orderId);
}