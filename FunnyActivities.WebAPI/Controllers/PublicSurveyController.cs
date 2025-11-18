using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using FunnyActivities.Application.Queries.SurveyManagement;
using FunnyActivities.Application.Commands.SurveyManagement;
using FunnyActivities.Application.DTOs.SurveyManagement;
using FunnyActivities.Application.Services;
using FunnyActivities.WebAPI.Controllers.Base;
using Microsoft.Extensions.Localization;

namespace FunnyActivities.WebAPI.Controllers
{
    /// <summary>
    /// Public Survey Controller for public access to surveys and voting.
    /// Provides endpoints that don't require authentication for survey participation.
    /// </summary>
    /// <remarks>
    /// Authorization Requirements:
    /// - No authentication required for public endpoints
    /// - Some endpoints may require survey-specific tokens for voting
    /// </remarks>
    [ApiController]
    [Route("api/surveys")]
    public class PublicSurveyController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly IVotingService _votingService;
        private readonly ILogger<PublicSurveyController> _logger;
        private readonly IStringLocalizer<PublicSurveyController> _localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublicSurveyController"/> class.
        /// </summary>
        /// <param name="mediator">The mediator for handling commands and queries.</param>
        /// <param name="votingService">The voting service.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="localizer">The string localizer.</param>
        public PublicSurveyController(
            IMediator mediator,
            IVotingService votingService,
            ILogger<PublicSurveyController> logger,
            IStringLocalizer<PublicSurveyController> localizer)
            : base(logger)
        {
            _mediator = mediator;
            _votingService = votingService;
            _logger = logger;
            _localizer = localizer;
        }

        /// <summary>
        /// Retrieves a public survey by its unique identifier.
        /// </summary>
        /// <remarks>
        /// Public endpoint that doesn't require authentication.
        /// Returns survey information if the survey is currently active and public.
        /// </remarks>
        /// <param name="surveyId">The unique identifier of the survey.</param>
        /// <returns>The public survey information.</returns>
        [HttpGet("{surveyId}/public")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(SurveyDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetPublicSurvey(Guid surveyId)
        {
            _logger.LogInformation("Retrieving public survey with ID: {SurveyId}", surveyId);

            var query = new GetPublicSurveyQuery { SurveyId = surveyId };
            var survey = await _mediator.Send(query);

            if (survey == null)
            {
                _logger.LogWarning("Public survey with ID {SurveyId} not found or not accessible", surveyId);
                return this.ApiError(_localizer["PublicSurveyNotFoundOrAccessible"], "NotFound", 404);
            }

            return this.ApiSuccess(survey, _localizer["PublicSurveyRetrieved"]);
        }

        /// <summary>
        /// Retrieves a public survey by its share token.
        /// </summary>
        /// <remarks>
        /// Public endpoint that doesn't require authentication.
        /// Returns survey information if the survey is currently active and the share token is valid.
        /// </remarks>
        /// <param name="shareToken">The share token of the survey.</param>
        /// <returns>The public survey information.</returns>
        [HttpGet("share/{shareToken}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(SurveyDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetPublicSurveyByShareToken(string shareToken)
        {
            _logger.LogInformation("Retrieving public survey with share token: {ShareToken}", shareToken);

            try
            {
                var query = new GetSurveyByShareTokenQuery { ShareToken = shareToken };
                var survey = await _mediator.Send(query);

                return this.ApiSuccess(survey, _localizer["PublicSurveyRetrieved"]);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Public survey with share token {ShareToken} not found: {Message}", shareToken, ex.Message);
                return this.ApiError(_localizer["SurveyNotFound"], "NotFound", 404);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Public survey with share token {ShareToken} not available: {Message}", shareToken, ex.Message);
                return this.ApiError(_localizer["SurveyNotAvailable"], "NotAvailable", 404);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving survey with share token {ShareToken}", shareToken);
                return this.ApiError(_localizer["PublicSurveyRetrieveUnexpected"], "InternalError", 500);
            }
        }

        /// <summary>
        /// Registers a new survey participant.
        /// </summary>
        /// <remarks>
        /// Public endpoint for registering participants before voting.
        /// Creates a participant record with name, surname, and children count.
        /// Accepts share token and resolves survey ID internally.
        /// </remarks>
        /// <param name="shareToken">The survey share token.</param>
        /// <param name="request">The participant registration request.</param>
        /// <returns>The created participant information.</returns>
        [HttpPost("share/{shareToken}/register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(SurveyParticipantDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RegisterParticipant(string shareToken, [FromBody] RegisterParticipantRequest request)
        {
            _logger.LogInformation("Register participant endpoint called for share token: {ShareToken}", shareToken);

            // Resolve survey by share token
            SurveyDto survey;
            try
            {
                var surveyQuery = new GetSurveyByShareTokenQuery { ShareToken = shareToken };
                survey = await _mediator.Send(surveyQuery);
                _logger.LogInformation("Resolved survey ID {SurveyId} from share token {ShareToken}", survey.Id, shareToken);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Survey not found for share token {ShareToken}: {Message}", shareToken, ex.Message);
                return this.ApiError(_localizer["SurveyNotFound"], "NotFound", 404);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Survey not available for share token {ShareToken}: {Message}", shareToken, ex.Message);
                return this.ApiError(_localizer["SurveyNotAvailable"], "NotAvailable", 404);
            }

            var command = new RegisterParticipantCommand
            {
                SurveyId = survey.Id,
                FirstName = request.FirstName,
                LastName = request.LastName,
                ChildrenCount = request.ChildrenCount
            };

            try
            {
                var result = await _mediator.Send(command);
                _logger.LogInformation("Participant registered successfully with ID: {ParticipantId}", result.Id);
                return this.ApiSuccess(result, _localizer["ParticipantRegistered"]);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Participant registration failed: {Message}", ex.Message);
                return this.ApiError(string.Format(_localizer["ParticipantRegistrationValidationError"], ex.Message), "ValidationError", 400);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Participant registration failed: {Message}", ex.Message);
                return this.ApiError(_localizer["ParticipantRegistrationNotFound"], "NotFound", 404);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while registering participant for share token {ShareToken}", shareToken);
                return this.ApiError(_localizer["ParticipantRegistrationUnexpected"], "InternalError", 500);
            }
        }

        /// <summary>
        /// Submits a vote for a survey activity.
        /// </summary>
        /// <remarks>
        /// Public endpoint for voting on survey activities.
        /// Requires a valid participant ID from registration.
        /// </remarks>
        /// <param name="request">The vote request containing activity ID, vote value, and participant ID.</param>
        /// <returns>The vote confirmation.</returns>
        [HttpPost("vote")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(VoteDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Vote([FromBody] VoteRequest request)
        {
            _logger.LogInformation("Vote endpoint called. Received VoteRequest: SurveyActivityId={SurveyActivityId}, VoteValue={VoteValue}, SurveyParticipantId={SurveyParticipantId}",
                request.SurveyActivityId, request.VoteValue, request.SurveyParticipantId);

            var command = new VoteCommand
            {
                SurveyActivityId = request.SurveyActivityId,
                VoteValue = request.VoteValue,
                SurveyParticipantId = request.SurveyParticipantId
            };

            _logger.LogInformation("Created VoteCommand: SurveyActivityId={SurveyActivityId}, VoteValue={VoteValue}, SurveyParticipantId={SurveyParticipantId}",
                command.SurveyActivityId, command.VoteValue, command.SurveyParticipantId);

            try
            {
                var result = await _mediator.Send(command);
                _logger.LogInformation("Vote submitted successfully for activity ID: {ActivityId}", request.SurveyActivityId);
                return this.ApiSuccess(result, _localizer["VoteSubmitted"]);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Vote submission failed: {Message}", ex.Message);
                return this.ApiError(string.Format(_localizer["VoteValidationError"], ex.Message), "ValidationError", 400);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Vote submission failed: {Message}", ex.Message);
                return this.ApiError(_localizer["VoteNotFound"], "NotFound", 404);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while submitting vote for activity {ActivityId}", request.SurveyActivityId);
                return this.ApiError(_localizer["VoteUnexpected"], "InternalError", 500);
            }
        }

        /// <summary>
        /// Retrieves activities for a public survey.
        /// </summary>
        /// <remarks>
        /// Public endpoint that returns the list of activities included in a survey.
        /// This is useful for displaying survey options to users before voting.
        /// </remarks>
        /// <param name="surveyId">The unique identifier of the survey.</param>
        /// <returns>The list of survey activities.</returns>
        [HttpGet("{surveyId}/activities")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<SurveyActivityDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetSurveyActivities(Guid surveyId)
        {
            _logger.LogInformation("Retrieving activities for public survey ID: {SurveyId}", surveyId);

            var query = new GetSurveyActivitiesQuery(surveyId);
            var activities = await _mediator.Send(query);

            if (activities == null || !activities.Any())
            {
                _logger.LogWarning("No activities found for survey ID {SurveyId}", surveyId);
                return this.ApiError(_localizer["SurveyActivitiesNotFound"], "NotFound", 404);
            }

            return this.ApiSuccess(activities, _localizer["SurveyActivitiesRetrieved"]);
        }

        /// <summary>
        /// Gets all votes by a survey participant.
        /// </summary>
        /// <remarks>
        /// Public endpoint to get votes for a participant to determine which activities they've already voted on.
        /// </remarks>
        /// <param name="participantId">The unique identifier of the survey participant.</param>
        /// <returns>The list of votes by the participant.</returns>
        [HttpGet("participant/{participantId}/votes")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<VoteDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetParticipantVotes(Guid participantId)
        {
            _logger.LogInformation("Getting votes for participant ID: {ParticipantId}", participantId);

            try
            {
                var votes = await _votingService.GetParticipantVotesAsync(participantId);
                return this.ApiSuccess(votes, _localizer["ParticipantVotesRetrieved"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving votes for participant {ParticipantId}", participantId);
                return this.ApiError(_localizer["ParticipantVotesUnexpected"], "InternalError", 500);
            }
        }

        /// <summary>
        /// Checks if a survey is currently active and accepting votes.
        /// </summary>
        /// <remarks>
        /// Public endpoint to check survey availability before voting.
        /// Returns survey status information including whether it's active and accepting votes.
        /// </remarks>
        /// <param name="surveyId">The unique identifier of the survey.</param>
        /// <returns>The survey status information.</returns>
        [HttpGet("{surveyId}/status")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> GetSurveyStatus(Guid surveyId)
        {
            _logger.LogInformation("Checking status for survey ID: {SurveyId}", surveyId);

            var query = new GetPublicSurveyQuery { SurveyId = surveyId };
            var survey = await _mediator.Send(query);

            if (survey == null)
            {
                _logger.LogWarning("Survey status check failed - survey not found: {SurveyId}", surveyId);
                return this.ApiError(_localizer["SurveyNotFound"], "NotFound", 404);
            }

            var status = new
            {
                SurveyId = survey.Id,
                Title = survey.Title,
                IsActive = survey.IsActive,
                IsCurrentlyActive = survey.StartDate <= DateTime.UtcNow && (!survey.EndDate.HasValue || survey.EndDate.Value >= DateTime.UtcNow),
                StartDate = survey.StartDate,
                EndDate = survey.EndDate,
                TotalActivities = survey.Activities?.Count ?? 0,
                CanVote = survey.IsActive && survey.StartDate <= DateTime.UtcNow && (!survey.EndDate.HasValue || survey.EndDate.Value >= DateTime.UtcNow)
            };

            return this.ApiSuccess(status, _localizer["SurveyStatusRetrieved"]);
        }
    }
}
