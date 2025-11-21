using System;

namespace FunnyActivities.Application.AI
{
    /// <summary>
    /// Desteklenen büyük dil modeli sağlayıcıları.
    /// </summary>
    public enum LlmProvider
    {
        Ollama = 0,
        OpenAI = 1
    }

    /// <summary>
    /// Bir LLM çağrısı için seçilen sağlayıcı ve model bilgileri.
    /// </summary>
    public sealed record LlmSelection(
        LlmProvider Provider,
        string? Model = null,
        float? Temperature = null,
        int? MaxTokens = null,
        string? SystemPrompt = null);

    /// <summary>
    /// Sistem genelindeki varsayılan seçim değerlerini temsil eder.
    /// </summary>
    public sealed record LlmDefaults(
        LlmProvider DefaultProvider,
        string? DefaultModel);

    /// <summary>
    /// Sağlayıcıların expose ettiği model bilgisini temsil eder.
    /// </summary>
    public sealed record LlmModelInfo(
        string Name,
        string DisplayName,
        LlmProvider Provider,
        bool IsDefault = false,
        bool IsAvailable = true);
}
