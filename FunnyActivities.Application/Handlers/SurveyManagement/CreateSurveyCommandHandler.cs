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
    /// Handler for creating a new survey.
    /// </summary>
    public class CreateSurveyCommandHandler : IRequestHandler<CreateSurveyCommand, SurveyDto>
    {
        private readonly FunnyActivities.Domain.Interfaces.ISurveyRepository _surveyRepository;
        private readonly FunnyActivities.Application.Interfaces.IActivityRepository _activityRepository;
        private readonly FunnyActivities.Domain.Interfaces.IUserRepository _userRepository;
        private readonly ILogger<CreateSurveyCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateSurveyCommandHandler"/> class.
        /// </summary>
        /// <param name="surveyRepository">The survey repository.</param>
        /// <param name="activityRepository">The activity repository.</param>
        /// <param name="userRepository">The user repository.</param>
        /// <param name="logger">The logger.</param>
        public CreateSurveyCommandHandler(
            FunnyActivities.Domain.Interfaces.ISurveyRepository surveyRepository,
            FunnyActivities.Application.Interfaces.IActivityRepository activityRepository,
            FunnyActivities.Domain.Interfaces.IUserRepository userRepository,
            ILogger<CreateSurveyCommandHandler> logger)
        {
            _surveyRepository = surveyRepository;
            _activityRepository = activityRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        /// <summary>
        /// Handles the create survey command.
        /// </summary>
        /// <param name="request">The create survey command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created survey DTO.</returns>
        public async Task<SurveyDto> Handle(CreateSurveyCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating new survey: {Title}", request.Title);
            _logger.LogInformation("Survey StartDate: {StartDate} (Kind: {Kind}), EndDate: {EndDate} (Kind: {EndKind})",
                request.StartDate, request.StartDate.Kind,
                request.EndDate, request.EndDate?.Kind);

            // Validate that the user exists
            var user = await _userRepository.GetByIdAsync(request.CreatedByUserId);
            if (user == null)
            {
                _logger.LogWarning("User not found: {UserId}", request.CreatedByUserId);
                throw new ArgumentException("User not found");
            }

            // Validate that all activities exist
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

            // Check if survey title already exists
            var existingSurveys = await _surveyRepository.GetByTitleAsync(request.Title);
            if (existingSurveys.Any())
            {
                _logger.LogWarning("Survey title already exists: {Title}", request.Title);
                throw new ArgumentException("Survey title already exists");
            }

            // Ensure dates are in UTC to prevent PostgreSQL conversion errors
            var startDateUtc = DateTime.SpecifyKind(request.StartDate, DateTimeKind.Utc);
            var endDateUtc = request.EndDate.HasValue ? DateTime.SpecifyKind(request.EndDate.Value, DateTimeKind.Utc) : (DateTime?)null;

            _logger.LogInformation("Converted dates - StartDate UTC: {StartDateUtc}, EndDate UTC: {EndDateUtc}",
                startDateUtc, endDateUtc);

            // Create the survey
            var survey = new Survey(
                request.Title,
                request.Description,
                request.CreatedByUserId,
                startDateUtc,
                endDateUtc,
                request.MaxParticipants
            );

            // Create survey activities before saving to avoid concurrency issues
            var order = 1;
            foreach (var activityId in request.ActivityIds)
            {
                var surveyActivity = new SurveyActivity(survey.Id, activityId, order++);
                survey.SurveyActivities.Add(surveyActivity);
                _logger.LogInformation("Added SurveyActivity: SurveyId={SurveyId}, ActivityId={ActivityId}, Order={Order}",
                    surveyActivity.SurveyId, surveyActivity.ActivityId, surveyActivity.Order);
            }

            _logger.LogInformation("Total SurveyActivities added: {Count}", survey.SurveyActivities.Count);
            _logger.LogInformation("Survey created with ID: {SurveyId}, Title: {Title}", survey.Id, survey.Title);
            _logger.LogInformation("Calling AddAsync for survey with activities");

            await _surveyRepository.AddAsync(survey);

            _logger.LogInformation("AddAsync completed successfully");

            _logger.LogInformation("Survey created successfully with ID: {SurveyId}", survey.Id);

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
                HasReachedMaxParticipants = survey.HasReachedMaxParticipants(),
                ShareToken = survey.ShareToken
            };
        }
    }
}