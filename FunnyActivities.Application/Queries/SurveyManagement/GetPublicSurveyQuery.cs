using MediatR;
using FunnyActivities.Application.DTOs.SurveyManagement;

namespace FunnyActivities.Application.Queries.SurveyManagement
{
    /// <summary>
    /// Query for getting a public survey by ID (for public access).
    /// </summary>
    public class GetPublicSurveyQuery : IRequest<SurveyDto>
    {
        /// <summary>
        /// Gets or sets the unique identifier of the survey.
        /// </summary>
        public Guid SurveyId { get; set; }

        /// <summary>
        /// Gets or sets the public access token for the survey.
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include activities in the result.
        /// </summary>
        public bool IncludeActivities { get; set; } = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetPublicSurveyQuery"/> class.
        /// </summary>
        public GetPublicSurveyQuery()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetPublicSurveyQuery"/> class with specified parameters.
        /// </summary>
        /// <param name="surveyId">The unique identifier of the survey.</param>
        /// <param name="accessToken">The public access token for the survey.</param>
        /// <param name="includeActivities">Whether to include activities in the result.</param>
        public GetPublicSurveyQuery(Guid surveyId, string accessToken, bool includeActivities = true)
        {
            SurveyId = surveyId;
            AccessToken = accessToken;
            IncludeActivities = includeActivities;
        }
    }
}