using System;

namespace FunnyActivities.Application.DTOs.SurveyManagement
{
    /// <summary>
    /// DTO for survey activity information.
    /// </summary>
    public class SurveyActivityDto
    {
        /// <summary>
        /// Gets or sets the survey activity ID.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the survey ID.
        /// </summary>
        public Guid SurveyId { get; set; }

        /// <summary>
        /// Gets or sets the activity ID.
        /// </summary>
        public Guid ActivityId { get; set; }

        /// <summary>
        /// Gets or sets the activity name.
        /// </summary>
        public string ActivityName { get; set; }

        /// <summary>
        /// Gets or sets the activity description.
        /// </summary>
        public string ActivityDescription { get; set; }

        /// <summary>
        /// Gets or sets the activity duration in minutes.
        /// </summary>
        public int? DurationMinutes { get; set; }

        /// <summary>
        /// Gets or sets the average vote for this activity.
        /// </summary>
        public double AverageVote { get; set; }

        /// <summary>
        /// Gets or sets the total vote count for this activity.
        /// </summary>
        public int VoteCount { get; set; }

        /// <summary>
        /// Gets or sets the order of this activity in the survey.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the video URL of the activity.
        /// </summary>
        public string? VideoUrl { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SurveyActivityDto"/> class.
        /// </summary>
        public SurveyActivityDto()
        {
        }
    }
}