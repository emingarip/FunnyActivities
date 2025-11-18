using System;
using FluentValidation;
using FunnyActivities.Application.Commands.ActivityManagement;
using FunnyActivities.Application.Validators;

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
                .NotEmpty().WithMessage(ValidationMessageProvider.Get("ActivityIdRequired"));

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(ValidationMessageProvider.Get("ActivityNameRequired"))
                .Length(1, 200).WithMessage(ValidationMessageProvider.Get("ActivityNameLength1To200"));

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage(ValidationMessageProvider.Get("ActivityDescriptionMax1000"))
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.VideoUrl)
                .Must(url => string.IsNullOrEmpty(url) || IsValidVideoUrl(url))
                .WithMessage(ValidationMessageProvider.Get("ActivityVideoUrlInvalid"))
                .When(x => !string.IsNullOrEmpty(x.VideoUrl));

            RuleFor(x => x.IntroVideoUrl)
                .Must(url => string.IsNullOrEmpty(url) || IsValidVideoUrl(url))
                .WithMessage(ValidationMessageProvider.Get("ActivityIntroVideoUrlInvalid"))
                .When(x => !string.IsNullOrEmpty(x.IntroVideoUrl));

            RuleFor(x => x.DurationHours)
                .InclusiveBetween(0, 23).WithMessage(ValidationMessageProvider.Get("DurationHoursRange"))
                .When(x => x.DurationHours.HasValue);

            RuleFor(x => x.DurationMinutes)
                .InclusiveBetween(0, 59).WithMessage(ValidationMessageProvider.Get("DurationMinutesRange"))
                .When(x => x.DurationMinutes.HasValue);

            RuleFor(x => x.DurationSeconds)
                .InclusiveBetween(0, 59).WithMessage(ValidationMessageProvider.Get("DurationSecondsRange"))
                .When(x => x.DurationSeconds.HasValue);

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage(ValidationMessageProvider.Get("UserIdRequired"));
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
