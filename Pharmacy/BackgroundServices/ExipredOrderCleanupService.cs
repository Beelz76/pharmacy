﻿using Pharmacy.ExternalServices;
using Pharmacy.Services.Interfaces;

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
    private int _iteration = 1;

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
                var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
                var pharmacyProductService = scope.ServiceProvider.GetRequiredService<IPharmacyProductService>();

                var now = _dateTimeProvider.UtcNow;
                var expiredOrders = await db.Orders
                    .Include(o => o.Payment)
                    .Include(o => o.User)
                    .Include(o => o.OrderItems)
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
                        order.CancellationComment = "Истек срок оплаты";

                        foreach (var item in order.OrderItems)
                        {
                            var pharmacyProduct = await db.PharmacyProducts.FirstOrDefaultAsync(
                                pp => pp.PharmacyId == order.PharmacyId && pp.ProductId == item.ProductId,
                                cancellationToken: stoppingToken);
                            if (pharmacyProduct != null)
                            {
                                pharmacyProduct.StockQuantity += item.Quantity;
                            }
                        }
                        
                        if (order.Payment != null &&
                            ((PaymentStatusEnum)order.Payment.StatusId == PaymentStatusEnum.Pending ||
                             (PaymentStatusEnum)order.Payment.StatusId == PaymentStatusEnum.NotPaid))
                        {
                            order.Payment.StatusId = (int)PaymentStatusEnum.Cancelled;
                            order.Payment.UpdatedAt = now;
                        }
                        
                        if (order.User != null)
                        {
                            var body = $@"<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 10px;'>
                                <h2 style='color: #2c3e50;'>Здравствуйте, {order.User.LastName} {order.User.FirstName}!</h2>
                                <p style='font-size: 16px; color: #333;'>Ваш заказ <strong>{order.Number}</strong> был отменен.</p>
                                <p>Причина: Истек срок оплаты</p>
                            </div>";
                            await emailSender.SendEmailAsync(order.User.Email, $"Заказ {order.Number} отменен", body);
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
