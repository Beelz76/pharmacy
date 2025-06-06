using Pharmacy.Database.Entities;
using Pharmacy.Database.Repositories.Interfaces;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto;
using Pharmacy.Shared.Result;

namespace Pharmacy.Services;

public class DeliveryService : IDeliveryService
{
    private readonly IDeliveryRepository _repository;

    public DeliveryService(IDeliveryRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<DeliveryDto>> GetByOrderIdAsync(int orderId)
    {
        var entity = await _repository.GetByOrderIdAsync(orderId);
        if (entity == null)
        {
            return Result.Failure<DeliveryDto>(Error.NotFound("Доставка не найдена"));
        }

        var dto = new DeliveryDto(
            entity.Id,
            entity.OrderId,
            entity.UserAddressId,
            entity.Comment,
            entity.DeliveryDate
        );

        return Result.Success(dto);
    }

    public async Task<Result<CreatedDto>> CreateAsync(CreateDeliveryRequest request)
    {
        var delivery = new Delivery
        {
            OrderId = request.OrderId,
            UserAddressId = request.UserAddressId,
            Comment = request.Comment,
            DeliveryDate = request.DeliveryDate
        };

        await _repository.AddAsync(delivery);
        return Result.Success(new CreatedDto(delivery.Id));
    }

    public async Task<Result> UpdateAsync(UpdateDeliveryRequest request)
    {
        var delivery = await _repository.GetByOrderIdAsync(request.OrderId);
        if (delivery == null)
        {
            return Result.Failure(Error.NotFound("Доставка не найдена"));
        }

        delivery.UserAddressId = request.UserAddressId;
        delivery.Comment = request.Comment;
        delivery.DeliveryDate = request.DeliveryDate;

        await _repository.UpdateAsync(delivery);
        return Result.Success();
    }
}