using Pharmacy.Shared.Dto;
using Pharmacy.Shared.Enums;
using Pharmacy.Shared.Result;

namespace Pharmacy.Services.Interfaces;

public interface IEmailVerificationService
{
    Task<Result> SendCodeAsync(string email, VerificationPurposeEnum purpose);
    Task<Result<ConfirmCodeDto>> ConfirmCodeAsync(string email, string code, VerificationPurposeEnum purpose, int? userId = null);
    Task<Result<bool>> CheckEmailVerifiedAsync(string email);
    Task<Result> RecoverPasswordAsync(string email, string newPassword);
    Task<Result> GenerateVerificationCodeAsync(int userId, string email, VerificationPurposeEnum purpose);
}