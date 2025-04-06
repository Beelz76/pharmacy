using System.Net.Mail;
using Pharmacy.Services.Interfaces;

namespace Pharmacy.Data.ExternalServices;

public class EmailSender : IEmailService
{
    private readonly IConfiguration _configuration;
    public EmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public async Task SendEmailAsync(string email, string subject, string message)
    {
        var smtpClient = new SmtpClient("")
    }
}