using Pharmacy.Shared.Dto;
using Pharmacy.Shared.Dto.Delivery;
using Pharmacy.Shared.Result;

namespace Pharmacy.Services.Interfaces;

public interface IDeliveryService
{
    Task<Result<DeliveryDto>> GetByOrderIdAsync(int orderId);
    Task<Result<CreatedDto>> CreateAsync(CreateDeliveryRequest request);
    Task<Result> UpdateAsync(UpdateDeliveryRequest request);
    Task<Result<PaginatedList<DeliveryDetailsDto>>> GetPaginatedAsync(DeliveryFilters filters, int pageNumber, int pageSize);
}