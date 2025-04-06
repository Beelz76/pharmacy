namespace Pharmacy.Services.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string email, string subject, string message);
}