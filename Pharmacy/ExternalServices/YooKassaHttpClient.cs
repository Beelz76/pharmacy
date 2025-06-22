using System.Text;
using System.Text.Json;
using Pharmacy.Shared.Dto;
using Pharmacy.Shared.Dto.YooKassa;
using Pharmacy.Shared.Result;

namespace Pharmacy.ExternalServices;

public class YooKassaHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<YooKassaHttpClient> _logger;

    public YooKassaHttpClient(HttpClient httpClient, ILogger<YooKassaHttpClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Result<YooKassaPaymentResult>> CreatePaymentAsync(YooKassaCreatePaymentRequest request, string idempotenceKey)
    {
        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        });

        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "payments")
        {
            Content = content
        };
        httpRequest.Headers.Add("Idempotence-Key", idempotenceKey);

        try
        {
            var response = await _httpClient.SendAsync(httpRequest);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Ошибка при создании платежа: {Body}", responseBody);
                return Result.Failure<YooKassaPaymentResult>(Error.Failure("Ошибка при создании платежа"));
            }

            var result = JsonSerializer.Deserialize<YooKassaPaymentResult>(responseBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            })!;

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании платежа");
            return Result.Failure<YooKassaPaymentResult>(Error.Failure("Ошибка при обращении к ЮKassa"));
        }
    }
    
    public async Task<Result<YooKassaPaymentInfo>> GetPaymentInfoAsync(string paymentId)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"payments/{paymentId}");
        try
        {
            var response = await _httpClient.SendAsync(httpRequest);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Ошибка при получении платежа: {Body}", responseBody);
                return Result.Failure<YooKassaPaymentInfo>(Error.Failure("Ошибка при обращении к ЮKassa"));
            }

            var result = JsonSerializer.Deserialize<YooKassaPaymentInfo>(responseBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            })!;

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обращении к ЮKassa");
            return Result.Failure<YooKassaPaymentInfo>(Error.Failure("Ошибка при обращении к ЮKassa"));
        }
    }
}
