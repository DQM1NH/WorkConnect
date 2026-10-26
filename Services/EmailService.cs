// Services/EmailService.cs
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace WorkConnect.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(_config["EmailSettings:SenderName"], _config["EmailSettings:SenderEmail"]));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = message };
            emailMessage.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_config["EmailSettings:SmtpServer"], int.Parse(_config["EmailSettings:Port"]), MailKit.Security.SecureSocketOptions.StartTls);
            var password = _config["EmailSettings:Password"];
            if (string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException(
                    "Thiếu EmailSettings:Password. Hãy đặt bằng User Secrets (dev) hoặc biến môi trường EmailSettings__Password (production), không ghi vào appsettings.json.");

            await client.AuthenticateAsync(_config["EmailSettings:Username"], password);
            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);
        }
    }
}