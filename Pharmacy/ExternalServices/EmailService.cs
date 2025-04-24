namespace Pharmacy.ExternalServices;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public async Task SendEmailAsync(string email, string subject, string message)
    {
        //var smtpClient = new SmtpClient("")
    }
}