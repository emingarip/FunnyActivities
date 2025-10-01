using MediatR;
using FunnyActivities.Application.DTOs.SurveyManagement;

namespace FunnyActivities.Application.Queries.SurveyManagement
{
    /// <summary>
    /// Query for getting survey statistics.
    /// </summary>
    public class GetSurveyStatisticsQuery : IRequest<SurveyStatisticsDto>
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
        /// Gets or sets a value indicating whether to include daily statistics.
        /// </summary>
        public bool IncludeDailyStatistics { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to include detailed activity statistics.
        /// </summary>
        public bool IncludeDetailedActivityStats { get; set; } = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetSurveyStatisticsQuery"/> class.
        /// </summary>
        public GetSurveyStatisticsQuery()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetSurveyStatisticsQuery"/> class with specified parameters.
        /// </summary>
        /// <param name="surveyId">The unique identifier of the survey.</param>
        /// <param name="requestedByUserId">The ID of the user making the request.</param>
        /// <param name="includeDailyStatistics">Whether to include daily statistics.</param>
        /// <param name="includeDetailedActivityStats">Whether to include detailed activity statistics.</param>
        public GetSurveyStatisticsQuery(Guid surveyId, Guid requestedByUserId, bool includeDailyStatistics = false, bool includeDetailedActivityStats = true)
        {
            SurveyId = surveyId;
            RequestedByUserId = requestedByUserId;
            IncludeDailyStatistics = includeDailyStatistics;
            IncludeDetailedActivityStats = includeDetailedActivityStats;
        }
    }
}