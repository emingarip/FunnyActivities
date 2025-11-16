using System.Collections.Generic;
using System.Threading.Tasks;

namespace FunnyActivities.Application.Interfaces
{
    public interface IAIService
    {
        Task<string> GenerateContentAsync(string prompt, string model = "llama2");
        Task<string> GeneratePersonaContentAsync(string personaDescription, string activityDescription, string model = "llama2");
        Task<string> GenerateStoryAsync(string personaDescription, string activityDescription, string model = "llama2");
        Task<string> GenerateNarrativeAsync(string personaDescription, string activityDescription, string model = "llama2");
        Task<string> GenerateTipsAsync(string personaDescription, string activityDescription, string model = "llama2");
        Task<bool> ValidateConnectionAsync();
        Task<IEnumerable<string>> ListAvailableModelsAsync();
    }
}