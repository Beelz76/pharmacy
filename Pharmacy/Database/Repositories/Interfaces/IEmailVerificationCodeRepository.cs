﻿using Pharmacy.Database.Entities;
using Pharmacy.Shared.Enums;

namespace Pharmacy.Database.Repositories.Interfaces;

public interface IEmailVerificationCodeRepository
{
    Task<EmailVerificationCode?> GetAsync(string email, string code, VerificationPurposeEnum purpose, DateTime now);
    Task<List<EmailVerificationCode>> GetAllActiveAsync(int userId, VerificationPurposeEnum purpose, DateTime now);
    Task<EmailVerificationCode?> GetLatestUsedAsync(int userId, VerificationPurposeEnum purpose, DateTime now);
    Task<bool> ExistsAsync(string email, string code, VerificationPurposeEnum purpose, DateTime now);
    Task AddAsync(EmailVerificationCode code);
    Task UpdateAsync(EmailVerificationCode code);
}