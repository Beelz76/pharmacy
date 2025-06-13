using Microsoft.EntityFrameworkCore;
using Pharmacy.Database.Entities;
using Pharmacy.Database.Repositories.Interfaces;
using Pharmacy.Extensions;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto;
using Pharmacy.Shared.Dto.Delivery;
using Pharmacy.Shared.Result;

namespace Pharmacy.Services;

public class DeliveryService : IDeliveryService
{
    private readonly IDeliveryRepository _repository;

    public DeliveryService(IDeliveryRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<DeliveryDetailsDto>> GetByOrderIdAsync(int orderId)
    {
        var entity = await _repository.GetByOrderIdAsync(orderId);
        if (entity == null)
        {
            return Result.Failure<DeliveryDetailsDto>(Error.NotFound("Доставка не найдена"));
        }

        var dto = new DeliveryDetailsDto(
            entity.Id,
            entity.OrderId,
            entity.Order.Number,
            AddressExtensions.FormatAddress(entity.UserAddress)!,
            entity.Price,
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
            Price = request.Price,
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
        delivery.Price = request.Price;
        delivery.DeliveryDate = request.DeliveryDate;

        await _repository.UpdateAsync(delivery);
        return Result.Success();
    }
    
    public async Task<Result<PaginatedList<DeliveryDetailsDto>>> GetPaginatedAsync(DeliveryFilters filters, int pageNumber, int pageSize)
    {
        var query = _repository.QueryWithDetails();

        if (!string.IsNullOrWhiteSpace(filters.OrderNumber))
        {
            query = query.Where(d => d.Order.Number.Contains(filters.OrderNumber));
        }

        if (filters.PharmacyId.HasValue)
        {
            query = query.Where(d => d.Order.PharmacyId == filters.PharmacyId.Value);
        }

        if (filters.FromDate.HasValue)
        {
            query = query.Where(d => d.DeliveryDate >= filters.FromDate.Value);
        }

        if (filters.ToDate.HasValue)
        {
            query = query.Where(d => d.DeliveryDate <= filters.ToDate.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(d => d.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new DeliveryDetailsDto(
                d.Id,
                d.OrderId,
                d.Order.Number,
                AddressExtensions.FormatAddress(d.UserAddress.Address)!,
                d.Price,
                d.Comment,
                d.DeliveryDate
            ))
            .ToListAsync();

        return Result.Success(new PaginatedList<DeliveryDetailsDto>(items, totalCount, pageNumber, pageSize));
    }

}