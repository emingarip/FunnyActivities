using MediatR;
using FunnyActivities.Application.DTOs.SurveyManagement;

namespace FunnyActivities.Application.Queries.SurveyManagement
{
    /// <summary>
    /// Query for getting a specific survey by ID.
    /// </summary>
    public class GetSurveyQuery : IRequest<SurveyDto>
    {
        /// <summary>
        /// Gets or sets the unique identifier of the survey to retrieve.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user making the request (optional, for authorization).
        /// </summary>
        public Guid? RequestedByUserId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include activities in the result.
        /// </summary>
        public bool IncludeActivities { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to include participants in the result.
        /// </summary>
        public bool IncludeParticipants { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to include votes in the result.
        /// </summary>
        public bool IncludeVotes { get; set; } = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetSurveyQuery"/> class.
        /// </summary>
        public GetSurveyQuery()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetSurveyQuery"/> class with specified parameters.
        /// </summary>
        /// <param name="id">The unique identifier of the survey to retrieve.</param>
        /// <param name="requestedByUserId">The ID of the user making the request.</param>
        /// <param name="includeActivities">Whether to include activities in the result.</param>
        /// <param name="includeParticipants">Whether to include participants in the result.</param>
        /// <param name="includeVotes">Whether to include votes in the result.</param>
        public GetSurveyQuery(Guid id, Guid? requestedByUserId = null, bool includeActivities = true, bool includeParticipants = false, bool includeVotes = false)
        {
            Id = id;
            RequestedByUserId = requestedByUserId;
            IncludeActivities = includeActivities;
            IncludeParticipants = includeParticipants;
            IncludeVotes = includeVotes;
        }
    }
}