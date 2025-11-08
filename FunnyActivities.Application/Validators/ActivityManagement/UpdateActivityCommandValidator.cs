using System;
using FluentValidation;
using FunnyActivities.Application.Commands.ActivityManagement;

namespace FunnyActivities.Application.Validators.ActivityManagement
{
    /// <summary>
    /// Validator for UpdateActivityCommand.
    /// </summary>
    public class UpdateActivityCommandValidator : AbstractValidator<UpdateActivityCommand>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateActivityCommandValidator"/> class.
        /// </summary>
        public UpdateActivityCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Activity ID is required. Please provide a valid activity identifier.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Activity name is required. Please enter a name for the activity.")
                .Length(1, 200).WithMessage("Activity name must be between 1 and 200 characters. For example: 'Morning Yoga Session' or 'Basic Cooking Class'.");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Activity description cannot exceed 1000 characters. Please keep your description concise and informative.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.VideoUrl)
                .Must(url => string.IsNullOrEmpty(url) || IsValidVideoUrl(url))
                .WithMessage("Invalid video URL format. Please provide a valid video URL (e.g., https://youtube.com/watch?v=...) or leave empty to remove the current video.")
                .When(x => !string.IsNullOrEmpty(x.VideoUrl));

            RuleFor(x => x.IntroVideoUrl)
                .Must(url => string.IsNullOrEmpty(url) || IsValidVideoUrl(url))
                .WithMessage("Invalid intro video URL format. Please provide a valid video URL or leave empty to remove the existing intro.")
                .When(x => !string.IsNullOrEmpty(x.IntroVideoUrl));

            RuleFor(x => x.DurationHours)
                .InclusiveBetween(0, 23).WithMessage("Hours must be between 0 and 23. For activities longer than 24 hours, consider breaking them into multiple sessions.")
                .When(x => x.DurationHours.HasValue);

            RuleFor(x => x.DurationMinutes)
                .InclusiveBetween(0, 59).WithMessage("Minutes must be between 0 and 59. Use this field to specify additional minutes beyond the hours.")
                .When(x => x.DurationMinutes.HasValue);

            RuleFor(x => x.DurationSeconds)
                .InclusiveBetween(0, 59).WithMessage("Seconds must be between 0 and 59. This is typically used for very short activities or precise timing.")
                .When(x => x.DurationSeconds.HasValue);

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required. Please ensure you are properly authenticated.");
        }

        /// <summary>
        /// Validates if a video URL is in a valid format.
        /// This method matches the validation logic in VideoUrl.cs to ensure consistency.
        /// </summary>
        /// <param name="url">The video URL to validate.</param>
        /// <returns>True if the URL is valid, false otherwise.</returns>
        private static bool IsValidVideoUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            // Allow MinIO object keys (they don't have URL format)
            // or valid HTTP/HTTPS URLs with more flexible validation
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uriResult))
            {
                // If it's not a valid URL, treat it as a MinIO object key
                // Allow any non-empty string for object keys (MinIO handles validation)
                // But ensure it's not just whitespace or special characters
                if (url.Trim().Length == 0 || url.Any(c => char.IsControl(c) && c != '\t' && c != '\n' && c != '\r'))
                {
                    return false;
                }
                return true; // Valid MinIO object key
            }
            else
            {
                // For valid URIs, allow more schemes beyond just HTTP/HTTPS
                // This includes data URIs, blob URIs, and other valid URI schemes
                var allowedSchemes = new[] {
                    Uri.UriSchemeHttp,
                    Uri.UriSchemeHttps,
                    "data",    // Data URIs for embedded content
                    "blob",    // Blob URIs for local content
                    "file",    // File URIs for local files
                    "rtmp",    // RTMP streams
                    "rtsp",    // RTSP streams
                    "mms"      // MMS streams
                };

                return allowedSchemes.Contains(uriResult.Scheme.ToLower());
            }
        }
    }
}
