using System;

namespace FunnyActivities.Application.DTOs.SurveyManagement
{
    /// <summary>
    /// Data transfer object for vote information.
    /// </summary>
    public class VoteDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the vote.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the ID of the survey.
        /// </summary>
        public Guid SurveyId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the survey activity.
        /// </summary>
        public Guid SurveyActivityId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the survey participant who voted.
        /// </summary>
        public Guid SurveyParticipantId { get; set; }

        /// <summary>
        /// Gets or sets the name of the survey participant who voted.
        /// </summary>
        public string ParticipantName { get; set; }

        /// <summary>
        /// Gets or sets the vote value (1-5 scale).
        /// </summary>
        public int VoteValue { get; set; }


        /// <summary>
        /// Gets or sets the date and time when the vote was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the vote was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}