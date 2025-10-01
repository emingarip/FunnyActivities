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

namespace FunnyActivities.Application.Handlers.SurveyManagement
{
    /// <summary>
    /// Handler for getting survey participants.
    /// </summary>
    public class GetSurveyParticipantsQueryHandler : IRequestHandler<GetSurveyParticipantsQuery, IEnumerable<SurveyParticipantDto>>
    {
        private readonly ISurveyRepository _surveyRepository;
        private readonly ILogger<GetSurveyParticipantsQueryHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetSurveyParticipantsQueryHandler"/> class.
        /// </summary>
        /// <param name="surveyRepository">The survey repository.</param>
        /// <param name="logger">The logger.</param>
        public GetSurveyParticipantsQueryHandler(
            ISurveyRepository surveyRepository,
            ILogger<GetSurveyParticipantsQueryHandler> logger)
        {
            _surveyRepository = surveyRepository;
            _logger = logger;
        }

        /// <summary>
        /// Handles the get survey participants query.
        /// </summary>
        /// <param name="request">The get survey participants query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The collection of survey participant DTOs.</returns>
        public async Task<IEnumerable<SurveyParticipantDto>> Handle(GetSurveyParticipantsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting survey participants for survey ID: {SurveyId}", request.SurveyId);

            var survey = await _surveyRepository.GetByIdAsync(request.SurveyId);
            if (survey == null)
            {
                _logger.LogWarning("Survey not found: {SurveyId}", request.SurveyId);
                throw new KeyNotFoundException("Survey not found");
            }

            var surveyWithParticipants = await _surveyRepository.GetWithParticipantsAsync(request.SurveyId);
            if (!surveyWithParticipants.Any())
            {
                _logger.LogInformation("No participants found for survey ID: {SurveyId}", request.SurveyId);
                return new List<SurveyParticipantDto>();
            }

            var surveyData = surveyWithParticipants.First();
            var participants = surveyData.SurveyParticipants.ToList();

            // Map to DTOs
            var participantDtos = participants.Select(p => new SurveyParticipantDto
            {
                Id = p.Id,
                SurveyId = p.SurveyId,
                FirstName = p.FirstName,
                LastName = p.LastName,
                ChildrenCount = p.ChildrenCount,
                ParticipatedAt = p.ParticipatedAt,
                CompletedAt = p.CompletedAt,
                IsCompleted = p.IsCompleted
            }).ToList();

            _logger.LogInformation("Survey participants retrieved successfully for survey ID: {SurveyId}. Count: {Count}", request.SurveyId, participantDtos.Count);

            return participantDtos;
        }
    }
}