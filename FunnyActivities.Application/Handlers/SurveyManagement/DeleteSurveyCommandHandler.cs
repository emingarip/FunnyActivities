using MediatR;
using Microsoft.Extensions.Logging;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Domain.Interfaces;
using FunnyActivities.Application.Commands.SurveyManagement;

namespace FunnyActivities.Application.Handlers.SurveyManagement
{
    /// <summary>
    /// Handler for deleting a survey.
    /// </summary>
    public class DeleteSurveyCommandHandler : IRequestHandler<DeleteSurveyCommand, bool>
    {
        private readonly ISurveyRepository _surveyRepository;
        private readonly ILogger<DeleteSurveyCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteSurveyCommandHandler"/> class.
        /// </summary>
        /// <param name="surveyRepository">The survey repository.</param>
        /// <param name="logger">The logger.</param>
        public DeleteSurveyCommandHandler(
            ISurveyRepository surveyRepository,
            ILogger<DeleteSurveyCommandHandler> logger)
        {
            _surveyRepository = surveyRepository;
            _logger = logger;
        }

        /// <summary>
        /// Handles the delete survey command.
        /// </summary>
        /// <param name="request">The delete survey command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if the survey was deleted successfully.</returns>
        public async Task<bool> Handle(DeleteSurveyCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting survey: {SurveyId}", request.Id);

            // Get the existing survey
            var survey = await _surveyRepository.GetByIdAsync(request.Id);
            if (survey == null)
            {
                _logger.LogWarning("Survey not found: {SurveyId}", request.Id);
                throw new KeyNotFoundException("Survey not found");
            }

            // Delete the survey (this will cascade delete related entities)
            await _surveyRepository.DeleteAsync(survey);

            _logger.LogInformation("Survey deleted successfully: {SurveyId}", request.Id);

            return true;
        }
    }
}