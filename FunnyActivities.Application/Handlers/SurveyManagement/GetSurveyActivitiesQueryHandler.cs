using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Domain.Interfaces;
using FunnyActivities.Application.Queries.SurveyManagement;
using FunnyActivities.Application.DTOs.SurveyManagement;
using FunnyActivities.Application.Interfaces;

namespace FunnyActivities.Application.Handlers.SurveyManagement
{
    /// <summary>
    /// Handler for getting survey activities for public voting access.
    /// </summary>
    public class GetSurveyActivitiesQueryHandler : IRequestHandler<GetSurveyActivitiesQuery, IEnumerable<SurveyActivityDto>>
    {
        private readonly ISurveyRepository _surveyRepository;
        private readonly ILogger<GetSurveyActivitiesQueryHandler> _logger;
        private readonly IMinioService _minioService;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetSurveyActivitiesQueryHandler"/> class.
        /// </summary>
        /// <param name="surveyRepository">The survey repository.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="minioService">The Minio service for generating signed URLs.</param>
        public GetSurveyActivitiesQueryHandler(
            ISurveyRepository surveyRepository,
            ILogger<GetSurveyActivitiesQueryHandler> logger,
            IMinioService minioService)
        {
            _surveyRepository = surveyRepository;
            _logger = logger;
            _minioService = minioService;
        }

        /// <summary>
        /// Determines if a video URL is a MinIO object key that needs signed URL conversion.
        /// </summary>
        /// <param name="videoUrl">The video URL to check.</param>
        /// <returns>True if the URL is a MinIO object key, false otherwise.</returns>
        private bool IsMinioObjectKey(string videoUrl)
        {
            // MinIO object keys for videos start with "videos/" pattern
            // They are not valid HTTP/HTTPS URLs
            return !string.IsNullOrEmpty(videoUrl) &&
                   videoUrl.StartsWith("videos/") &&
                   !Uri.TryCreate(videoUrl, UriKind.Absolute, out _);
        }

        /// <summary>
        /// Handles the get survey activities query.
        /// </summary>
        /// <param name="request">The get survey activities query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The collection of survey activity DTOs.</returns>
        public async Task<IEnumerable<SurveyActivityDto>> Handle(GetSurveyActivitiesQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting survey activities for survey ID: {SurveyId}", request.SurveyId);

            var survey = await _surveyRepository.GetByIdAsync(request.SurveyId);
            if (survey == null)
            {
                _logger.LogWarning("Survey not found: {SurveyId}", request.SurveyId);
                throw new KeyNotFoundException("Survey not found");
            }

            // Check if survey is active and currently available
            if (!survey.IsActive || !survey.IsCurrentlyActive())
            {
                _logger.LogWarning("Survey is not active or not currently available: {SurveyId}", request.SurveyId);
                throw new InvalidOperationException("Survey is not available");
            }

            var surveyWithActivities = await _surveyRepository.GetWithActivitiesAsync(request.SurveyId);
            if (!surveyWithActivities.Any())
            {
                _logger.LogInformation("No activities found for survey ID: {SurveyId}", request.SurveyId);
                return new List<SurveyActivityDto>();
            }

            var surveyData = surveyWithActivities.First();
            var surveyActivities = surveyData.SurveyActivities.OrderBy(sa => sa.Order).ToList();

            // Map to DTOs and process video URLs
            var activities = new List<SurveyActivityDto>();
            foreach (var sa in surveyActivities)
            {
                string videoUrl = null;

                // Check if the activity has a video URL and if it's a MinIO object key
                if (sa.Activity?.VideoUrl != null && IsMinioObjectKey(sa.Activity.VideoUrl.Value))
                {
                    try
                    {
                        // Check if MinIO service is available before attempting to generate signed URL
                        if (_minioService != null)
                        {
                            // Generate signed URL for MinIO object key
                            videoUrl = await _minioService.GenerateVideoPreSignedUrlAsync(sa.Activity.VideoUrl.Value);
                            _logger.LogInformation("Generated signed URL for video object key: {ObjectKey}", sa.Activity.VideoUrl.Value);
                        }
                        else
                        {
                            _logger.LogWarning("MinIO service is not available for object key: {ObjectKey}", sa.Activity.VideoUrl.Value);
                            // Fallback to object key if MinIO service is not available
                            videoUrl = sa.Activity.VideoUrl.Value;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to generate signed URL for video object key: {ObjectKey}", sa.Activity.VideoUrl.Value);
                        // Fallback to object key if signed URL generation fails
                        videoUrl = sa.Activity.VideoUrl.Value;
                    }
                }
                else
                {
                    // Use the original URL if it's not a MinIO object key
                    videoUrl = sa.Activity?.VideoUrl?.Value;
                }

                activities.Add(new SurveyActivityDto
                {
                    Id = sa.Id,
                    SurveyId = sa.SurveyId,
                    ActivityId = sa.ActivityId,
                    ActivityName = sa.Activity?.Name ?? "Unknown Activity",
                    ActivityDescription = sa.Activity?.Description ?? "",
                    DurationMinutes = sa.Activity?.Duration != null ? (int?)sa.Activity.Duration.Value.TotalMinutes : null,
                    AverageVote = sa.GetAverageVote(),
                    VoteCount = sa.GetVoteCount(),
                    Order = sa.Order,
                    VideoUrl = videoUrl
                });
            }

            _logger.LogInformation("Survey activities retrieved successfully for survey ID: {SurveyId}", request.SurveyId);

            return activities;
        }
    }
}