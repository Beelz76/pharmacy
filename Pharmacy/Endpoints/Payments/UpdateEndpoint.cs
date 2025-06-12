using FastEndpoints;
using FluentValidation;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Enums;

namespace Pharmacy.Endpoints.Payments;

public class UpdateEndpoint : Endpoint<UpdatePaymentRequest>
{
    private readonly ILogger<UpdateEndpoint> _logger;
    private readonly IPaymentService _paymentService;
    public UpdateEndpoint(ILogger<UpdateEndpoint> logger, IPaymentService paymentService)
    {
        _logger = logger;
        _paymentService = paymentService;
    }

    public override void Configure()
    {
        Put("payments/status");
        Roles("Admin", "Employee");
        AllowAnonymous();
        Tags("Payments");
        Summary(s => { s.Summary = "Обновить статус платежа по заказу"; }); 
    }

    public override async Task HandleAsync(UpdatePaymentRequest request, CancellationToken ct)
    {
        var result = await _paymentService.UpdateStatusAsync(request.OrderId, request.NewStatus);
        if (result.IsSuccess)
        {
            await SendOkAsync(ct);
        }
        else
        {
            await SendAsync(result.Error, (int)result.Error.StatusCode, ct);
        }
    }
}

public record UpdatePaymentRequest(int OrderId, PaymentStatusEnum NewStatus);

public class UpdatePaymentRequestValidator : Validator<UpdatePaymentRequest>
{
    public UpdatePaymentRequestValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty(); 
        
        RuleFor(x => x.NewStatus)
            .NotEmpty(); 
    }
}