using System;

namespace FunnyActivities.Application.DTOs.SurveyManagement
{
    /// <summary>
    /// Data transfer object for survey participant information.
    /// </summary>
    public class SurveyParticipantDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the survey participant.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the survey ID.
        /// </summary>
        public Guid SurveyId { get; set; }

        /// <summary>
        /// Gets or sets the participant's first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the participant's last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the number of children.
        /// </summary>
        public int ChildrenCount { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the participant started participating.
        /// </summary>
        public DateTime ParticipatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the participant completed the survey.
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the survey is completed.
        /// </summary>
        public bool IsCompleted { get; set; }
    }
}