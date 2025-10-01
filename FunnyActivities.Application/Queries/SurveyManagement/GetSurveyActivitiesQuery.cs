using System;
using System.Collections.Generic;
using MediatR;
using FunnyActivities.Application.DTOs.SurveyManagement;

namespace FunnyActivities.Application.Queries.SurveyManagement
{
    /// <summary>
    /// Query for getting survey activities by survey ID for public voting access.
    /// </summary>
    public class GetSurveyActivitiesQuery : IRequest<IEnumerable<SurveyActivityDto>>
    {
        /// <summary>
        /// Gets or sets the unique identifier of the survey.
        /// </summary>
        public Guid SurveyId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetSurveyActivitiesQuery"/> class.
        /// </summary>
        public GetSurveyActivitiesQuery()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetSurveyActivitiesQuery"/> class with specified parameters.
        /// </summary>
        /// <param name="surveyId">The unique identifier of the survey.</param>
        public GetSurveyActivitiesQuery(Guid surveyId)
        {
            SurveyId = surveyId;
        }
    }
}