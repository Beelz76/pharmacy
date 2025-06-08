using Pharmacy.Database.Entities;
using Pharmacy.Shared.Dto;
using Pharmacy.Shared.Dto.Auth;
using Pharmacy.Shared.Enums;
using Pharmacy.Shared.Result;

namespace Pharmacy.Services.Interfaces;

public interface IEmailVerificationService
{
    Task<Result> SendCodeAsync(int userId, string email, bool emailVerified, VerificationPurposeEnum purpose);
    Task<Result<ConfirmCodeDto>> ConfirmCodeAsync(string email, string code, VerificationPurposeEnum purpose, int? userId = null);
    Task<Result<bool>> CheckEmailVerifiedAsync(string email);
    Task<Result> RecoverPasswordAsync(string email, string newPassword);
}