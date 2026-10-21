// Services/IEmailService.cs
using System.Threading.Tasks;

namespace WorkConnect.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}