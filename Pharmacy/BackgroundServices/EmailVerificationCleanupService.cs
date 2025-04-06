using Pharmacy.Data;

namespace Pharmacy.BackgroundServices;

public class EmailVerificationCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<EmailVerificationCleanupService> _logger;
    private readonly IConfiguration _configuration;
    public EmailVerificationCleanupService(ILogger<EmailVerificationCleanupService> logger, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<PharmacyDbContext>();

                var now = DateTime.UtcNow;
                var expiredCodes = db.EmailVerificationCodes.Where(x => x.ExpiresAt < now || x.IsUsed).ToList();

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
            
            await Task.Delay(TimeSpan.FromMinutes(int.Parse(_configuration["EmailVerificationCleanupInMinutes"]!)), stoppingToken);
        }
    }
}