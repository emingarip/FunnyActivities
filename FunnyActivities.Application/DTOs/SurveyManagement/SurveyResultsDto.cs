using System;
using System.Collections.Generic;

namespace FunnyActivities.Application.DTOs.SurveyManagement
{
    /// <summary>
    /// Data transfer object for survey results.
    /// </summary>
    public class SurveyResultsDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the survey.
        /// </summary>
        public Guid SurveyId { get; set; }

        /// <summary>
        /// Gets or sets the title of the survey.
        /// </summary>
        public string SurveyTitle { get; set; }

        /// <summary>
        /// Gets or sets the total number of participants.
        /// </summary>
        public int TotalParticipants { get; set; }

        /// <summary>
        /// Gets or sets the total number of completed surveys.
        /// </summary>
        public int CompletedCount { get; set; }

        /// <summary>
        /// Gets or sets the completion rate as a percentage.
        /// </summary>
        public double CompletionRate { get; set; }

        /// <summary>
        /// Gets or sets the activity results.
        /// </summary>
        public List<ActivityResultDto> ActivityResults { get; set; } = new List<ActivityResultDto>();

        /// <summary>
        /// Gets or sets the individual votes.
        /// </summary>
        public List<VoteDto> Votes { get; set; } = new List<VoteDto>();
    }

    /// <summary>
    /// Data transfer object for activity results.
    /// </summary>
    public class ActivityResultDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the survey activity.
        /// </summary>
        public Guid SurveyActivityId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the activity.
        /// </summary>
        public Guid ActivityId { get; set; }

        /// <summary>
        /// Gets or sets the name of the activity.
        /// </summary>
        public string ActivityName { get; set; }

        /// <summary>
        /// Gets or sets the description of the activity.
        /// </summary>
        public string ActivityDescription { get; set; }

        /// <summary>
        /// Gets or sets the average vote value.
        /// </summary>
        public double AverageVote { get; set; }

        /// <summary>
        /// Gets or sets the total number of votes.
        /// </summary>
        public int VoteCount { get; set; }

        /// <summary>
        /// Gets or sets the vote distribution (1-5 scale).
        /// </summary>
        public Dictionary<int, int> VoteDistribution { get; set; } = new Dictionary<int, int>();

        /// <summary>
        /// Gets or sets the comments for this activity.
        /// </summary>
        public List<string> Comments { get; set; } = new List<string>();
    }
}