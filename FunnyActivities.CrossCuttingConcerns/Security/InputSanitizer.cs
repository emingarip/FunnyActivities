using Ganss.Xss;

namespace FunnyActivities.CrossCuttingConcerns.Security;

public static class InputSanitizer
{
    private static readonly HtmlSanitizer _sanitizer = new HtmlSanitizer();

    public static string Sanitize(string input)
    {
        return _sanitizer.Sanitize(input);
    }
}