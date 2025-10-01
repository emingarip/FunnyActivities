using System;
using System.Collections.Generic;
using MediatR;
using FunnyActivities.Application.DTOs.SurveyManagement;

namespace FunnyActivities.Application.Queries.SurveyManagement
{
    /// <summary>
    /// Query for getting survey participants by survey ID.
    /// </summary>
    public class GetSurveyParticipantsQuery : IRequest<IEnumerable<SurveyParticipantDto>>
    {
        /// <summary>
        /// Gets or sets the unique identifier of the survey.
        /// </summary>
        public Guid SurveyId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetSurveyParticipantsQuery"/> class.
        /// </summary>
        public GetSurveyParticipantsQuery()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetSurveyParticipantsQuery"/> class with specified parameters.
        /// </summary>
        /// <param name="surveyId">The unique identifier of the survey.</param>
        public GetSurveyParticipantsQuery(Guid surveyId)
        {
            SurveyId = surveyId;
        }
    }
}