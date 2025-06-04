// using Pharmacy.Database;
// using Pharmacy.DateTimeProvider;
//
// namespace Pharmacy.BackgroundServices;
//
// public class RefreshTokenCleanupService
// {
//     private readonly IServiceScopeFactory _serviceScopeFactory;
//     private readonly ILogger<RefreshTokenCleanupService> _logger;
//     private readonly IDateTimeProvider _dateTimeProvider;
//     private readonly IConfiguration _configuration;
//
//     private int _iteration = 0;
//     
//     public RefreshTokenCleanupService(
//         ILogger<RefreshTokenCleanupService> logger,
//         IServiceScopeFactory serviceScopeFactory,
//         IConfiguration configuration,
//         IDateTimeProvider dateTimeProvider)
//     {
//         _logger = logger;
//         _serviceScopeFactory = serviceScopeFactory;
//         _configuration = configuration;
//         _dateTimeProvider = dateTimeProvider;
//     }
//
//     protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//     {
//         while (!stoppingToken.IsCancellationRequested)
//         {
//             _logger.LogInformation("Очистка refresh-токенов - итерация №{Iteration}", _iteration++);
//             try
//             {
//                 using var scope = _serviceScopeFactory.CreateScope();
//                 var db = scope.ServiceProvider.GetRequiredService<PharmacyDbContext>();
//
//                 var now = _dateTimeProvider.UtcNow;
//                 var expiredTokens = await db.RefreshTokens.Where(x => x.ExpiresAt < now && x.IsRevoked).ToListAsync(cancellationToken: stoppingToken);
//
//                 if (expiredTokens.Any())
//                 {
//                     db.RefreshTokens.RemoveRange(expiredTokens);
//                     await db.SaveChangesAsync(stoppingToken);
//                     _logger.LogInformation("Удалено {Count} просроченных/отозванных refresh-токенов.", expiredTokens.Count);
//                 }
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Ошибка при очистке refresh-токенов.");
//             }
//             
//             if (!int.TryParse(_configuration["RefreshTokenCleanupInMinutes"], out var delayMinutes))
//             {
//                 _logger.LogWarning("Некорректное значение RefreshTokenCleanupInMinutes. Используется значение по умолчанию: 60 мин.");
//                 delayMinutes = 60;
//             }
//             
//             await Task.Delay(TimeSpan.FromMinutes(delayMinutes), stoppingToken);
//         }
//     }
// }