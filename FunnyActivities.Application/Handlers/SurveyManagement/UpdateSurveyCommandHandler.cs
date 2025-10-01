using MediatR;
using Microsoft.Extensions.Logging;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Application.Commands.SurveyManagement;
using FunnyActivities.Application.DTOs.SurveyManagement;
using FunnyActivities.Domain.Interfaces;

namespace FunnyActivities.Application.Handlers.SurveyManagement
{
    /// <summary>
    /// Handler for updating an existing survey.
    /// </summary>
    public class UpdateSurveyCommandHandler : IRequestHandler<UpdateSurveyCommand, SurveyDto>
    {
        private readonly FunnyActivities.Domain.Interfaces.ISurveyRepository _surveyRepository;
        private readonly FunnyActivities.Application.Interfaces.IActivityRepository _activityRepository;
        private readonly ILogger<UpdateSurveyCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateSurveyCommandHandler"/> class.
        /// </summary>
        /// <param name="surveyRepository">The survey repository.</param>
        /// <param name="activityRepository">The activity repository.</param>
        /// <param name="logger">The logger.</param>
        public UpdateSurveyCommandHandler(
            FunnyActivities.Domain.Interfaces.ISurveyRepository surveyRepository,
            FunnyActivities.Application.Interfaces.IActivityRepository activityRepository,
            ILogger<UpdateSurveyCommandHandler> logger)
        {
            _surveyRepository = surveyRepository;
            _activityRepository = activityRepository;
            _logger = logger;
        }

        /// <summary>
        /// Handles the update survey command.
        /// </summary>
        /// <param name="request">The update survey command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated survey DTO.</returns>
        public async Task<SurveyDto> Handle(UpdateSurveyCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating survey: {SurveyId}", request.Id);

            // Get the existing survey
            var survey = await _surveyRepository.GetByIdAsync(request.Id);
            if (survey == null)
            {
                _logger.LogWarning("Survey not found: {SurveyId}", request.Id);
                throw new KeyNotFoundException("Survey not found");
            }

            // Check if title is being changed and if it already exists
            if (!string.IsNullOrEmpty(request.Title) && request.Title != survey.Title)
            {
                var existingSurveys = await _surveyRepository.GetByTitleAsync(request.Title);
                if (existingSurveys.Any(s => s.Id != request.Id))
                {
                    _logger.LogWarning("Survey title already exists: {Title}", request.Title);
                    throw new ArgumentException("Survey title already exists");
                }
            }

            // Validate activities if provided
            if (request.ActivityIds != null && request.ActivityIds.Count > 0)
            {
                var activities = await _activityRepository.GetAllAsync();
                var activityIds = activities.Select(a => a.Id).ToList();

                foreach (var activityId in request.ActivityIds)
                {
                    if (!activityIds.Contains(activityId))
                    {
                        _logger.LogWarning("Activity not found: {ActivityId}", activityId);
                        throw new ArgumentException($"Activity not found: {activityId}");
                    }
                }
            }

            // Update survey properties - ensure dates are in UTC
            var title = request.Title ?? survey.Title;
            var description = request.Description ?? survey.Description;
            var startDate = request.StartDate.HasValue
                ? DateTime.SpecifyKind(request.StartDate.Value, DateTimeKind.Utc)
                : survey.StartDate;
            var endDate = request.EndDate.HasValue
                ? DateTime.SpecifyKind(request.EndDate.Value, DateTimeKind.Utc)
                : survey.EndDate;
            var maxParticipants = request.MaxParticipants ?? survey.MaxParticipants;

            if (request.StartDate.HasValue || request.EndDate.HasValue)
            {
                _logger.LogInformation("Updated dates to UTC - StartDate: {StartDate}, EndDate: {EndDate}",
                    startDate, endDate);
            }

            survey.Update(title, description, startDate, endDate, maxParticipants);

            // Update activities if provided
            if (request.ActivityIds != null && request.ActivityIds.Count > 0)
            {
                // Remove existing survey activities
                survey.SurveyActivities.Clear();

                // Add new survey activities
                var order = 1;
                foreach (var activityId in request.ActivityIds)
                {
                    var surveyActivity = new SurveyActivity(survey.Id, activityId, order++);
                    survey.SurveyActivities.Add(surveyActivity);
                }
            }

            await _surveyRepository.UpdateAsync(survey);

            _logger.LogInformation("Survey updated successfully: {SurveyId}", survey.Id);

            return MapToDto(survey);
        }

        private SurveyDto MapToDto(Survey survey)
        {
            return new SurveyDto
            {
                Id = survey.Id,
                Title = survey.Title,
                Description = survey.Description,
                CreatedByUserId = survey.CreatedByUserId,
                IsActive = survey.IsActive,
                StartDate = survey.StartDate,
                EndDate = survey.EndDate,
                MaxParticipants = survey.MaxParticipants,
                ParticipantCount = survey.GetParticipantCount(),
                CreatedAt = survey.CreatedAt,
                UpdatedAt = survey.UpdatedAt,
                IsCurrentlyActive = survey.IsCurrentlyActive(),
                HasReachedMaxParticipants = survey.HasReachedMaxParticipants()
            };
        }
    }
}