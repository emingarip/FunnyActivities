using System;
using System.Collections.Generic;

namespace FunnyActivities.Application.DTOs.SurveyManagement
{
    /// <summary>
    /// Data transfer object for survey statistics.
    /// </summary>
    public class SurveyStatisticsDto
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
        /// Gets or sets the total number of votes.
        /// </summary>
        public int TotalVotes { get; set; }

        /// <summary>
        /// Gets or sets the overall average vote across all activities.
        /// </summary>
        public double OverallAverageVote { get; set; }

        /// <summary>
        /// Gets or sets the activity statistics.
        /// </summary>
        public List<ActivityStatisticsDto> ActivityStatistics { get; set; } = new List<ActivityStatisticsDto>();

        /// <summary>
        /// Gets or sets the daily participation statistics.
        /// </summary>
        public List<DailyStatisticsDto> DailyStatistics { get; set; } = new List<DailyStatisticsDto>();

        /// <summary>
        /// Gets or sets the vote distribution across all activities.
        /// </summary>
        public Dictionary<int, int> OverallVoteDistribution { get; set; } = new Dictionary<int, int>();

        /// <summary>
        /// Gets or sets the survey creation date.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the survey start date.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the survey end date.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the survey is currently active.
        /// </summary>
        public bool IsCurrentlyActive { get; set; }
    }

    /// <summary>
    /// Data transfer object for activity statistics.
    /// </summary>
    public class ActivityStatisticsDto
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
        /// Gets or sets the standard deviation of votes.
        /// </summary>
        public double StandardDeviation { get; set; }

        /// <summary>
        /// Gets or sets the minimum vote value.
        /// </summary>
        public int MinVote { get; set; }

        /// <summary>
        /// Gets or sets the maximum vote value.
        /// </summary>
        public int MaxVote { get; set; }
    }

    /// <summary>
    /// Data transfer object for daily statistics.
    /// </summary>
    public class DailyStatisticsDto
    {
        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the number of participants on this date.
        /// </summary>
        public int ParticipantCount { get; set; }

        /// <summary>
        /// Gets or sets the number of completed surveys on this date.
        /// </summary>
        public int CompletedCount { get; set; }

        /// <summary>
        /// Gets or sets the number of votes on this date.
        /// </summary>
        public int VoteCount { get; set; }
    }
}