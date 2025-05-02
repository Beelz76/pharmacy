using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Result;

namespace Pharmacy.Services;

public class EmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;
    public EmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<Result> SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(_configuration["SmtpSettings:Sender"]));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = body };
            message.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(
                _configuration["SmtpSettings:Host"],
                int.Parse(_configuration["SmtpSettings:Port"]!),
                SecureSocketOptions.StartTls);

            await smtp.AuthenticateAsync(
                _configuration["SmtpSettings:Username"],
                _configuration["SmtpSettings:Password"]);

            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Problem("Ошибка при отправке письма: " + ex.Message));
        }
    }
}