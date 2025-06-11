using FastEndpoints;
using FluentValidation;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Enums;

namespace Pharmacy.Endpoints.Payments;

public class WebhookEndpoint : Endpoint<WebhookRequest>
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<WebhookEndpoint> _logger;

    public WebhookEndpoint(IPaymentService paymentService, ILogger<WebhookEndpoint> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    public override void Configure()
    {
        Post("payments/yookassa/webhook");
        AllowAnonymous();
        Tags("Payments");
        Summary(s => s.Summary = "Обработка вебхуков от ЮKassa");
    }

    public override async Task HandleAsync(WebhookRequest req, CancellationToken ct)
    {
        _logger.LogInformation("Получен вебхук YooKassa: {event}", req.Event);

        if (req.Object?.Metadata == null || !req.Object.Metadata.TryGetValue("order_id", out var orderIdStr) || !int.TryParse(orderIdStr, out var orderId))
        {
            _logger.LogWarning("Некорректный webhook payload");
            await SendAsync("Некорректный payload", 400, ct);
            return;
        }

        switch (req.Event)
        {
            case "payment.succeeded":
                await _paymentService.UpdateStatusAsync(orderId, PaymentStatusEnum.Completed);
                break;
            case "payment.canceled":
                await _paymentService.UpdateStatusAsync(orderId, PaymentStatusEnum.Cancelled);
                break;
            default:
                _logger.LogInformation("Необработанное событие: {event}", req.Event);
                break;
        }

        await SendOkAsync(ct);
    }
}

public record YooKassaPaymentObject(Dictionary<string, string> Metadata);
public record WebhookRequest(string Event, YooKassaPaymentObject Object);

public class WebhookRequestValidator : Validator<WebhookRequest>
{
    public WebhookRequestValidator()
    {
        RuleFor(x => x.Event).NotEmpty();
        RuleFor(x => x.Object).NotNull();
    }
}


