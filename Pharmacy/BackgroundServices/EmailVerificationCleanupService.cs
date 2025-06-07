using Microsoft.EntityFrameworkCore;
using Pharmacy.Database;
using Pharmacy.DateTimeProvider;

namespace Pharmacy.BackgroundServices;

public class EmailVerificationCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<EmailVerificationCleanupService> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IConfiguration _configuration;

    private int _iteration = 1;
    
    public EmailVerificationCleanupService(ILogger<EmailVerificationCleanupService> logger, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, IDateTimeProvider dateTimeProvider)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _configuration = configuration;
        _dateTimeProvider = dateTimeProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Очистка кодов верификаций - итерация №{Iteration}", _iteration++);
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<PharmacyDbContext>();

                var now = _dateTimeProvider.UtcNow;
                var expiredCodes = await db.EmailVerificationCodes.Where(x => x.ExpiresAt < now || x.IsUsed).ToListAsync(cancellationToken: stoppingToken);

                if (expiredCodes.Any())
                {
                    db.EmailVerificationCodes.RemoveRange(expiredCodes);
                    await db.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Удалено {Count} просроченных/использованных кодов.", expiredCodes.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при очистке кодов подтверждения.");
            }
            
            if (!int.TryParse(_configuration["EmailVerificationCleanupInMinutes"], out var delayMinutes))
            {
                _logger.LogWarning("Некорректное значение EmailVerificationCleanupInMinutes. Используется значение по умолчанию: 10 мин.");
                delayMinutes = 10;
            }
            
            await Task.Delay(TimeSpan.FromMinutes(delayMinutes), stoppingToken);
        }
    }
}