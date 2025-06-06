namespace Pharmacy.BackgroundServices;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Database;
using Shared.Enums;
using DateTimeProvider;

public class ExpiredOrderCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<ExpiredOrderCleanupService> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IConfiguration _configuration;
    private int _iteration = 0;

    public ExpiredOrderCleanupService(IServiceScopeFactory serviceScopeFactory,
        ILogger<ExpiredOrderCleanupService> logger,
        IConfiguration configuration,
        IDateTimeProvider dateTimeProvider)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _configuration = configuration;
        _dateTimeProvider = dateTimeProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Очистка просроченных заказов - итерация №{Iteration}", _iteration++);
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<PharmacyDbContext>();

                var now = _dateTimeProvider.UtcNow;
                var expiredOrders = await db.Orders
                    .Include(o => o.Payment)
                    .Where(o => o.ExpiresAt != null &&
                                o.ExpiresAt < now &&
                                o.StatusId == (int)OrderStatusEnum.WaitingForPayment)
                    .ToListAsync(cancellationToken: stoppingToken);

                if (expiredOrders.Any())
                {
                    foreach (var order in expiredOrders)
                    {
                        order.StatusId = (int)OrderStatusEnum.Cancelled;
                        order.UpdatedAt = now;

                        if (order.Payment != null &&
                            ((PaymentStatusEnum)order.Payment.StatusId == PaymentStatusEnum.Pending ||
                             (PaymentStatusEnum)order.Payment.StatusId == PaymentStatusEnum.NotPaid))
                        {
                            order.Payment.StatusId = (int)PaymentStatusEnum.Cancelled;
                            order.Payment.UpdatedAt = now;
                        }
                    }

                    await db.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Отменено {Count} просроченных заказов", expiredOrders.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отмене просроченных заказов.");
            }

            if (!int.TryParse(_configuration["ExpiredOrderCleanupInMinutes"], out var delayMinutes))
            {
                _logger.LogWarning("Некорректное значение ExpiredOrderCleanupInMinutes. Используется значение по умолчанию: 60 мин.");
                delayMinutes = 60;
            }

            await Task.Delay(TimeSpan.FromMinutes(delayMinutes), stoppingToken);
        }
    }
}
