using System.Threading.Tasks;

namespace FunnyActivities.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }
}