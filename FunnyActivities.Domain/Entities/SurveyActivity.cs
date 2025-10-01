using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FunnyActivities.Domain.Entities
{
    /// <summary>
    /// Represents a survey activity entity that links surveys to activities.
    /// </summary>
    public class SurveyActivity
    {
        /// <summary>
        /// Gets or sets the unique identifier of the survey activity.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the ID of the survey.
        /// </summary>
        public Guid SurveyId { get; set; }

        /// <summary>
        /// Gets or sets the survey.
        /// </summary>
        public Survey Survey { get; set; }

        /// <summary>
        /// Gets or sets the ID of the activity.
        /// </summary>
        public Guid ActivityId { get; set; }

        /// <summary>
        /// Gets or sets the activity.
        /// </summary>
        public Activity Activity { get; set; }

        /// <summary>
        /// Gets or sets the order of the activity in the survey.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the survey activity was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the date and time when the survey activity was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the survey votes associated with this survey activity.
        /// </summary>
        public ICollection<SurveyVote> SurveyVotes { get; set; } = new List<SurveyVote>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SurveyActivity"/> class.
        /// </summary>
        public SurveyActivity()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SurveyActivity"/> class with specified parameters.
        /// </summary>
        /// <param name="surveyId">The ID of the survey.</param>
        /// <param name="activityId">The ID of the activity.</param>
        /// <param name="order">The order of the activity in the survey.</param>
        public SurveyActivity(Guid surveyId, Guid activityId, int order)
        {
            Id = Guid.NewGuid();
            SurveyId = surveyId;
            ActivityId = activityId;
            Order = order;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the order of the survey activity.
        /// </summary>
        /// <param name="order">The new order.</param>
        public void UpdateOrder(int order)
        {
            Order = order;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Gets the average vote value for this survey activity.
        /// </summary>
        /// <returns>The average vote value, or 0 if no votes exist.</returns>
        public double GetAverageVote()
        {
            if (SurveyVotes == null || !SurveyVotes.Any())
                return 0;

            return SurveyVotes.Average(v => v.VoteValue);
        }

        /// <summary>
        /// Gets the total number of votes for this survey activity.
        /// </summary>
        /// <returns>The total number of votes.</returns>
        public int GetVoteCount()
        {
            return SurveyVotes?.Count ?? 0;
        }
    }
}