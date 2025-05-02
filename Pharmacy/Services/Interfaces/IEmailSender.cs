using Pharmacy.Shared.Result;

namespace Pharmacy.Services.Interfaces;

public interface IEmailSender
{
    Task<Result> SendEmailAsync(string to, string subject, string body);
}