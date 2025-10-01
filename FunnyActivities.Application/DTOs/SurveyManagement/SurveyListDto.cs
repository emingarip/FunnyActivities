using System;
using System.Collections.Generic;

namespace FunnyActivities.Application.DTOs.SurveyManagement
{
    /// <summary>
    /// Data transfer object for survey list information.
    /// </summary>
    public class SurveyListDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the survey.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the title of the survey.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the description of the survey.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user who created the survey.
        /// </summary>
        public Guid CreatedByUserId { get; set; }

        /// <summary>
        /// Gets or sets the name of the user who created the survey.
        /// </summary>
        public string CreatedByUserName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the survey is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the start date of the survey.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date of the survey.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of participants allowed.
        /// </summary>
        public int? MaxParticipants { get; set; }

        /// <summary>
        /// Gets or sets the current participant count.
        /// </summary>
        public int ParticipantCount { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the survey was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the survey was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the number of activities in the survey.
        /// </summary>
        public int ActivityCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the survey is currently active.
        /// </summary>
        public bool IsCurrentlyActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the survey has reached maximum participants.
        /// </summary>
        public bool HasReachedMaxParticipants { get; set; }

        /// <summary>
        /// Gets or sets the share token for public URL access.
        /// </summary>
        public string ShareToken { get; set; }
    }
}