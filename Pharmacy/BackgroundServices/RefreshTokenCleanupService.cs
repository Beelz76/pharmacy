using Microsoft.EntityFrameworkCore;
using Pharmacy.Database;
using Pharmacy.DateTimeProvider;

namespace Pharmacy.BackgroundServices;

public class RefreshTokenCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<RefreshTokenCleanupService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IDateTimeProvider _dateTimeProvider;
    private int _iteration = 1;

    public RefreshTokenCleanupService(IServiceScopeFactory serviceScopeFactory,
        ILogger<RefreshTokenCleanupService> logger,
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
            _logger.LogInformation("Очистка refresh токенов - итерация №{Iteration}", _iteration++);
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<PharmacyDbContext>();

                var now = _dateTimeProvider.UtcNow;
                var expired = await db.RefreshTokens
                    .Where(t => t.ExpiresAt <= now || t.IsUsed)
                    .ToListAsync(stoppingToken);

                if (expired.Any())
                {
                    db.RefreshTokens.RemoveRange(expired);
                    await db.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Удалено {Count} токенов", expired.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка очистки refresh токенов");
            }

            if (!int.TryParse(_configuration["RefreshTokenCleanupInMinutes"], out var delayMinutes))
            {
                _logger.LogWarning("Некорректное значение RefreshTokenCleanupInMinutes. Используется значение по умолчанию: 60 мин.");
                delayMinutes = 60;
            }

            await Task.Delay(TimeSpan.FromMinutes(delayMinutes), stoppingToken);
        }
    }
}