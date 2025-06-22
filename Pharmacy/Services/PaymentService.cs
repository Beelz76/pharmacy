using Microsoft.EntityFrameworkCore;
using Pharmacy.Database.Entities;
using Pharmacy.Database.Repositories.Interfaces;
using Pharmacy.DateTimeProvider;
using Pharmacy.Extensions;
using Pharmacy.ExternalServices;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto;
using Pharmacy.Shared.Dto.Payment;
using Pharmacy.Shared.Enums;
using Pharmacy.Shared.Result;

namespace Pharmacy.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly YooKassaHttpClient _yooKassaClient;

    public PaymentService(IPaymentRepository paymentRepository, IDateTimeProvider dateTimeProvider, YooKassaHttpClient yooKassaClient)
    {
        _paymentRepository = paymentRepository;
        _dateTimeProvider = dateTimeProvider;
        _yooKassaClient = yooKassaClient;
    }

    public async Task CreateInitialPaymentAsync(int orderId, decimal amount, PaymentMethodEnum method)
    {
        var payment = new Payment
        {
            OrderId = orderId,
            Amount = amount,
            PaymentMethodId = (int)method,
            StatusId = method == PaymentMethodEnum.Online ? (int)PaymentStatusEnum.Pending : (int)PaymentStatusEnum.NotPaid,
            //Number = $"PAY-{Guid.NewGuid().ToString()[..8].ToUpper()}",
            CreatedAt = _dateTimeProvider.UtcNow,
            UpdatedAt = _dateTimeProvider.UtcNow
        };

        await _paymentRepository.AddAsync(payment);
    }
    
    public async Task<Result> UpdateStatusAsync(int orderId, PaymentStatusEnum newStatus)
    {
        var payment = await _paymentRepository.GetByOrderIdAsync(orderId);
        if (payment is null)
        {
            return Result.Failure(Error.NotFound("Платёж не найден"));
        }

        payment.StatusId = (int)newStatus;
        payment.UpdatedAt = _dateTimeProvider.UtcNow;
        if (newStatus == PaymentStatusEnum.Completed)
        {
            payment.TransactionDate = _dateTimeProvider.UtcNow;
        }

        await _paymentRepository.UpdateAsync(payment);
        return Result.Success();
    }
    
    public async Task<Result> SetExternalPaymentIdAsync(int orderId, string externalPaymentId)
    {
        var payment = await _paymentRepository.GetByOrderIdAsync(orderId);
        if (payment is null)
        {
            return Result.Failure(Error.NotFound("Платёж не найден"));
        }

        payment.ExternalPaymentId = externalPaymentId;
        payment.UpdatedAt = _dateTimeProvider.UtcNow;

        await _paymentRepository.UpdateAsync(payment);
        return Result.Success();
    }
    
    public async Task<Result<PaymentDetailsDto>> GetByIdAsync(int id)
    {
        var payment = await _paymentRepository.GetByIdWithDetailsAsync(id);
        if (payment is null)
        {
            return Result.Failure<PaymentDetailsDto>(Error.NotFound("Платёж не найден"));
        }

        var dto = new PaymentDetailsDto(
            payment.Id,
            payment.OrderId,
            payment.Order.Number,
            payment.Order.PharmacyId,
            payment.Order.Pharmacy.Name,
            AddressExtensions.FormatAddress(payment.Order.Pharmacy.Address)!,
            payment.Amount,
            ((PaymentMethodEnum)payment.PaymentMethodId).GetDescription(),
            ((PaymentStatusEnum)payment.StatusId).GetDescription(),
            payment.TransactionDate,
            payment.ExternalPaymentId
        );

        return Result.Success(dto);
    }

    public async Task<Result<PaginatedList<PaymentDetailsDto>>> GetPaginatedAsync(PaymentFilters filters, int pageNumber, int pageSize)
    {
        var query = _paymentRepository.QueryWithDetails();

        if (!string.IsNullOrWhiteSpace(filters.OrderNumber))
        {
            query = query.Where(p => p.Order.Number.Contains(filters.OrderNumber));
        }

        if (filters.PharmacyId.HasValue)
        {
            query = query.Where(p => p.Order.PharmacyId == filters.PharmacyId.Value);
        }

        if (filters.Status.HasValue)
        {
            query = query.Where(p => p.StatusId == (int)filters.Status.Value);
        }

        if (filters.Method.HasValue)
        {
            query = query.Where(p => p.PaymentMethodId == (int)filters.Method.Value);
        }
        
        if (!string.IsNullOrWhiteSpace(filters.ExternalPaymentId))
        {
            query = query.Where(p => p.ExternalPaymentId != null && p.ExternalPaymentId.Contains(filters.ExternalPaymentId));
        }

        if (filters.FromDate.HasValue)
        {
            query = query.Where(p => p.CreatedAt >= filters.FromDate.Value);
        }

        if (filters.ToDate.HasValue)
        {
            query = query.Where(p => p.CreatedAt <= filters.ToDate.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(p => p.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PaymentDetailsDto(
                p.Id,
                p.OrderId,
                p.Order.Number,
                p.Order.PharmacyId,
                p.Order.Pharmacy.Name,
                AddressExtensions.FormatAddress(p.Order.Pharmacy.Address)!,
                p.Amount,
                ((PaymentMethodEnum)p.PaymentMethodId).GetDescription(),
                ((PaymentStatusEnum)p.StatusId).GetDescription(),
                p.TransactionDate,
                p.ExternalPaymentId
            ))
            .ToListAsync();

        return Result.Success(new PaginatedList<PaymentDetailsDto>(items, totalCount, pageNumber, pageSize));
    }
    
    public async Task<Result<PaymentStatusEnum>> SyncStatusWithYooKassaAsync(int paymentId)
    {
        var payment = await _paymentRepository.GetByIdWithDetailsAsync(paymentId);
        if (payment is null)
        {
            return Result.Failure<PaymentStatusEnum>(Error.NotFound("Платёж не найден"));
        }

        if (string.IsNullOrWhiteSpace(payment.ExternalPaymentId))
        {
            return Result.Failure<PaymentStatusEnum>(Error.Failure("Внешний ID платежа отсутствует"));
        }

        var infoResult = await _yooKassaClient.GetPaymentInfoAsync(payment.ExternalPaymentId);
        if (infoResult.IsFailure)
        {
            return Result.Failure<PaymentStatusEnum>(infoResult.Error);
        }

        var yooStatus = infoResult.Value.Status;
        var newStatus = yooStatus switch
        {
            "succeeded" => PaymentStatusEnum.Completed,
            "canceled" => PaymentStatusEnum.Cancelled,
            "pending" => PaymentStatusEnum.Pending,
            "waiting_for_capture" => PaymentStatusEnum.Pending,
            _ => PaymentStatusEnum.Failed
        };

        if (payment.StatusId != (int)newStatus)
        {
            payment.StatusId = (int)newStatus;
            payment.UpdatedAt = _dateTimeProvider.UtcNow;
            if (newStatus == PaymentStatusEnum.Completed)
            {
                payment.TransactionDate = _dateTimeProvider.UtcNow;
            }

            await _paymentRepository.UpdateAsync(payment);
        }

        return Result.Success(newStatus);
    }
}