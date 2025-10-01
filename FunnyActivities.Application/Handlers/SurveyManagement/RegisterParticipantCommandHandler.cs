using MediatR;
using Microsoft.Extensions.Logging;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Domain.Interfaces;
using FunnyActivities.Application.Commands.SurveyManagement;
using FunnyActivities.Application.DTOs.SurveyManagement;

namespace FunnyActivities.Application.Handlers.SurveyManagement
{
    /// <summary>
    /// Handler for registering survey participants.
    /// </summary>
    public class RegisterParticipantCommandHandler : IRequestHandler<RegisterParticipantCommand, SurveyParticipantDto>
    {
        private readonly ISurveyRepository _surveyRepository;
        private readonly ILogger<RegisterParticipantCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterParticipantCommandHandler"/> class.
        /// </summary>
        /// <param name="surveyRepository">The survey repository.</param>
        /// <param name="logger">The logger.</param>
        public RegisterParticipantCommandHandler(
            ISurveyRepository surveyRepository,
            ILogger<RegisterParticipantCommandHandler> logger)
        {
            _surveyRepository = surveyRepository;
            _logger = logger;
        }

        /// <summary>
        /// Handles the register participant command.
        /// </summary>
        /// <param name="request">The register participant command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created survey participant DTO.</returns>
        public async Task<SurveyParticipantDto> Handle(RegisterParticipantCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("RegisterParticipantCommandHandler.Handle called. SurveyId={SurveyId}, FirstName={FirstName}, LastName={LastName}, ChildrenCount={ChildrenCount}",
                request.SurveyId, request.FirstName, request.LastName, request.ChildrenCount);

            // Validate that the survey exists and is active
            var survey = await _surveyRepository.GetByIdAsync(request.SurveyId);
            if (survey == null)
            {
                _logger.LogWarning("Survey not found: {SurveyId}", request.SurveyId);
                throw new ArgumentException("Survey not found");
            }

            if (!survey.IsActive)
            {
                _logger.LogWarning("Survey is not active: {SurveyId}", request.SurveyId);
                throw new InvalidOperationException("Survey is not active");
            }

            // Create the participant
            var participant = new SurveyParticipant(
                request.SurveyId,
                request.FirstName.Trim(),
                request.LastName.Trim(),
                request.ChildrenCount
            );

            await _surveyRepository.AddParticipantAsync(participant);

            _logger.LogInformation("Participant registered successfully with ID: {ParticipantId}", participant.Id);

            return MapToDto(participant);
        }

        private SurveyParticipantDto MapToDto(SurveyParticipant participant)
        {
            return new SurveyParticipantDto
            {
                Id = participant.Id,
                SurveyId = participant.SurveyId,
                FirstName = participant.FirstName,
                LastName = participant.LastName,
                ChildrenCount = participant.ChildrenCount,
                ParticipatedAt = participant.ParticipatedAt,
                CompletedAt = participant.CompletedAt,
                IsCompleted = participant.IsCompleted
            };
        }
    }
}