using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FunnyActivities.Domain.Entities
{
    /// <summary>
    /// Represents a survey vote entity that stores votes for survey activities.
    /// </summary>
    public class SurveyVote
    {
        /// <summary>
        /// Gets or sets the unique identifier of the survey vote.
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
        /// Gets or sets the ID of the survey activity.
        /// </summary>
        public Guid SurveyActivityId { get; set; }

        /// <summary>
        /// Gets or sets the survey activity.
        /// </summary>
        public SurveyActivity SurveyActivity { get; set; }

        /// <summary>
        /// Gets or sets the ID of the survey participant who voted.
        /// </summary>
        public Guid SurveyParticipantId { get; set; }

        /// <summary>
        /// Gets or sets the survey participant who voted.
        /// </summary>
        public SurveyParticipant SurveyParticipant { get; set; }

        /// <summary>
        /// Gets or sets the vote value (1-5 scale).
        /// </summary>
        [Range(1, 5)]
        public int VoteValue { get; set; }

        /// <summary>
        /// Gets or sets the optional comment for the vote.
        /// </summary>
        public string? Comment { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the vote was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the date and time when the vote was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Initializes a new instance of the <see cref="SurveyVote"/> class.
        /// </summary>
        public SurveyVote()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SurveyVote"/> class with specified parameters.
        /// </summary>
        /// <param name="surveyId">The ID of the survey.</param>
        /// <param name="surveyActivityId">The ID of the survey activity.</param>
        /// <param name="surveyParticipantId">The ID of the survey participant who voted.</param>
        /// <param name="voteValue">The vote value (1-5).</param>
        /// <param name="comment">The optional comment.</param>
        public SurveyVote(Guid surveyId, Guid surveyActivityId, Guid surveyParticipantId, int voteValue, string? comment = null)
        {
            Id = Guid.NewGuid();
            SurveyId = surveyId;
            SurveyActivityId = surveyActivityId;
            SurveyParticipantId = surveyParticipantId;
            VoteValue = voteValue;
            Comment = comment;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the vote information.
        /// </summary>
        /// <param name="voteValue">The new vote value.</param>
        /// <param name="comment">The new comment.</param>
        public void UpdateVote(int voteValue, string? comment = null)
        {
            VoteValue = voteValue;
            Comment = comment;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Validates the vote value.
        /// </summary>
        /// <returns>True if the vote value is valid (1-5); otherwise, false.</returns>
        public bool IsValidVoteValue()
        {
            return VoteValue >= 1 && VoteValue <= 5;
        }
    }
}