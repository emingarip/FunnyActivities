using MediatR;
using FunnyActivities.Application.DTOs.SurveyManagement;

namespace FunnyActivities.Application.Queries.SurveyManagement
{
    /// <summary>
    /// Query for getting a public survey by share token.
    /// </summary>
    public class GetSurveyByShareTokenQuery : IRequest<SurveyDto>
    {
        /// <summary>
        /// Gets or sets the share token of the survey.
        /// </summary>
        public string ShareToken { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include activities in the result.
        /// </summary>
        public bool IncludeActivities { get; set; } = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetSurveyByShareTokenQuery"/> class.
        /// </summary>
        public GetSurveyByShareTokenQuery()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetSurveyByShareTokenQuery"/> class with specified parameters.
        /// </summary>
        /// <param name="shareToken">The share token of the survey.</param>
        /// <param name="includeActivities">Whether to include activities in the result.</param>
        public GetSurveyByShareTokenQuery(string shareToken, bool includeActivities = true)
        {
            ShareToken = shareToken;
            IncludeActivities = includeActivities;
        }
    }
}