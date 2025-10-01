using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FunnyActivities.Domain.Entities
{
    /// <summary>
    /// Represents a survey entity in the system.
    /// </summary>
    public class Survey
    {
        /// <summary>
        /// Gets or sets the unique identifier of the survey.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the title of the survey.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the description of the survey.
        /// </summary>
        [MaxLength(1000)]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user who created the survey.
        /// </summary>
        public Guid CreatedByUserId { get; set; }

        /// <summary>
        /// Gets or sets the user who created the survey.
        /// </summary>
        public User CreatedByUser { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the survey is active.
        /// </summary>
        public bool IsActive { get; set; } = true;

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
        /// Gets or sets the date and time when the survey was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the date and time when the survey was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the share token for public URL access.
        /// </summary>
        [MaxLength(50)]
        public string ShareToken { get; set; }

        /// <summary>
        /// Gets or sets the row version for concurrency control.
        /// </summary>
        [Timestamp]
        public byte[] RowVersion { get; set; }

        /// <summary>
        /// Gets or sets the survey activities associated with this survey.
        /// </summary>
        public ICollection<SurveyActivity> SurveyActivities { get; set; } = new List<SurveyActivity>();

        /// <summary>
        /// Gets or sets the survey participants associated with this survey.
        /// </summary>
        public ICollection<SurveyParticipant> SurveyParticipants { get; set; } = new List<SurveyParticipant>();

        /// <summary>
        /// Gets or sets the survey votes associated with this survey.
        /// </summary>
        public ICollection<SurveyVote> SurveyVotes { get; set; } = new List<SurveyVote>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Survey"/> class.
        /// </summary>
        public Survey()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Survey"/> class with specified parameters.
        /// </summary>
        /// <param name="title">The title of the survey.</param>
        /// <param name="description">The description of the survey.</param>
        /// <param name="createdByUserId">The ID of the user who created the survey.</param>
        /// <param name="startDate">The start date of the survey.</param>
        /// <param name="endDate">The end date of the survey.</param>
        /// <param name="maxParticipants">The maximum number of participants.</param>
        public Survey(string title, string description, Guid createdByUserId, DateTime startDate, DateTime? endDate = null, int? maxParticipants = null)
        {
            Id = Guid.NewGuid();
            Title = title;
            Description = description;
            CreatedByUserId = createdByUserId;
            IsActive = true;
            StartDate = startDate;
            EndDate = endDate;
            MaxParticipants = maxParticipants;
            ShareToken = GenerateShareToken();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the survey information.
        /// </summary>
        /// <param name="title">The new title.</param>
        /// <param name="description">The new description.</param>
        /// <param name="startDate">The new start date.</param>
        /// <param name="endDate">The new end date.</param>
        /// <param name="maxParticipants">The new maximum participants.</param>
        public void Update(string title, string description, DateTime startDate, DateTime? endDate = null, int? maxParticipants = null)
        {
            Title = title;
            Description = description;
            StartDate = startDate;
            EndDate = endDate;
            MaxParticipants = maxParticipants;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Activates the survey.
        /// </summary>
        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Deactivates the survey.
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Checks if the survey is currently active.
        /// </summary>
        /// <returns>True if the survey is active and within the date range; otherwise, false.</returns>
        public bool IsCurrentlyActive()
        {
            var now = DateTime.UtcNow;
            return IsActive &&
                   StartDate <= now &&
                   (!EndDate.HasValue || EndDate.Value >= now);
        }

        /// <summary>
        /// Gets the current participant count.
        /// </summary>
        /// <returns>The number of participants who have completed the survey.</returns>
        public int GetParticipantCount()
        {
            return SurveyParticipants.Count(sp => sp.IsCompleted);
        }

        /// <summary>
        /// Checks if the survey has reached maximum participants.
        /// </summary>
        /// <returns>True if maximum participants is reached; otherwise, false.</returns>
        public bool HasReachedMaxParticipants()
        {
            if (!MaxParticipants.HasValue)
                return false;

            return GetParticipantCount() >= MaxParticipants.Value;
        }

        /// <summary>
        /// Generates a unique share token for the survey.
        /// </summary>
        /// <returns>A unique share token.</returns>
        private static string GenerateShareToken()
        {
            // Generate a URL-safe base64 string from a GUID
            var guid = Guid.NewGuid();
            var bytes = guid.ToByteArray();
            return Convert.ToBase64String(bytes)
                .Replace("/", "_")
                .Replace("+", "-")
                .Replace("=", "")
                .Substring(0, 22); // Take first 22 characters for a shorter token
        }
    }
}