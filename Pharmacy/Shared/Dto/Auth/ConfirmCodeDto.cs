namespace Pharmacy.Shared.Dto.Auth;

public record ConfirmCodeDto(bool Success, string? Token, string? RefreshToken);