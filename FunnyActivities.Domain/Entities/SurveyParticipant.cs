using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FunnyActivities.Domain.Entities
{
    /// <summary>
    /// Represents a survey participant entity that tracks who participated in surveys.
    /// </summary>
    public class SurveyParticipant
    {
        /// <summary>
        /// Gets or sets the unique identifier of the survey participant.
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
        /// Gets or sets the participant's first name.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the participant's last name.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the number of children the participant has.
        /// </summary>
        [Range(0, 20)]
        public int ChildrenCount { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the participant started participating.
        /// </summary>
        public DateTime ParticipatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the date and time when the participant completed the survey.
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the survey is completed.
        /// </summary>
        public bool IsCompleted { get; set; } = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="SurveyParticipant"/> class.
        /// </summary>
        public SurveyParticipant()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SurveyParticipant"/> class with specified parameters.
        /// </summary>
        /// <param name="surveyId">The ID of the survey.</param>
        /// <param name="firstName">The participant's first name.</param>
        /// <param name="lastName">The participant's last name.</param>
        /// <param name="childrenCount">The number of children.</param>
        public SurveyParticipant(Guid surveyId, string firstName, string lastName, int childrenCount)
        {
            Id = Guid.NewGuid();
            SurveyId = surveyId;
            FirstName = firstName;
            LastName = lastName;
            ChildrenCount = childrenCount;
            ParticipatedAt = DateTime.UtcNow;
            IsCompleted = false;
        }

        /// <summary>
        /// Marks the survey as completed.
        /// </summary>
        public void MarkAsCompleted()
        {
            IsCompleted = true;
            CompletedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Marks the survey as incomplete.
        /// </summary>
        public void MarkAsIncomplete()
        {
            IsCompleted = false;
            CompletedAt = null;
        }

        /// <summary>
        /// Gets the completion status of the survey.
        /// </summary>
        /// <returns>True if the survey is completed; otherwise, false.</returns>
        public bool IsSurveyCompleted()
        {
            return IsCompleted && CompletedAt.HasValue;
        }
    }
}