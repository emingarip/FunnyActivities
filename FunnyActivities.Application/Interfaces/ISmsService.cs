using System.Threading.Tasks;

namespace FunnyActivities.Application.Interfaces
{
    public interface ISmsService
    {
        Task SendSmsAsync(string to, string message);
    }
}