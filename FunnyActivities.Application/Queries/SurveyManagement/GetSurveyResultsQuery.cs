using MediatR;
using FunnyActivities.Application.DTOs.SurveyManagement;

namespace FunnyActivities.Application.Queries.SurveyManagement
{
    /// <summary>
    /// Query for getting survey results.
    /// </summary>
    public class GetSurveyResultsQuery : IRequest<SurveyResultsDto>
    {
        /// <summary>
        /// Gets or sets the unique identifier of the survey.
        /// </summary>
        public Guid SurveyId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user making the request (for authorization).
        /// </summary>
        public Guid RequestedByUserId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include individual votes.
        /// </summary>
        public bool IncludeIndividualVotes { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to include comments.
        /// </summary>
        public bool IncludeComments { get; set; } = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetSurveyResultsQuery"/> class.
        /// </summary>
        public GetSurveyResultsQuery()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetSurveyResultsQuery"/> class with specified parameters.
        /// </summary>
        /// <param name="surveyId">The unique identifier of the survey.</param>
        /// <param name="requestedByUserId">The ID of the user making the request.</param>
        /// <param name="includeIndividualVotes">Whether to include individual votes.</param>
        /// <param name="includeComments">Whether to include comments.</param>
        public GetSurveyResultsQuery(Guid surveyId, Guid requestedByUserId, bool includeIndividualVotes = false, bool includeComments = true)
        {
            SurveyId = surveyId;
            RequestedByUserId = requestedByUserId;
            IncludeIndividualVotes = includeIndividualVotes;
            IncludeComments = includeComments;
        }
    }
}