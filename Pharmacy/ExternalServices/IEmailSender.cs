using Pharmacy.Shared.Result;

namespace Pharmacy.ExternalServices;

public interface IEmailSender
{
    Task<Result> SendEmailAsync(string to, string subject, string body);
}