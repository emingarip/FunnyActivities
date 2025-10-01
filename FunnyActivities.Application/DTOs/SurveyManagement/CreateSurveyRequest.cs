using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FunnyActivities.Application.DTOs.SurveyManagement
{
    /// <summary>
    /// Request DTO for creating a new survey.
    /// </summary>
    public class CreateSurveyRequest
    {
        /// <summary>
        /// Gets or sets the title of the survey.
        /// </summary>
        [Required(ErrorMessage = "Survey title is required")]
        [StringLength(200, ErrorMessage = "Survey title cannot exceed 200 characters")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the description of the survey.
        /// </summary>
        [StringLength(1000, ErrorMessage = "Survey description cannot exceed 1000 characters")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the start date of the survey.
        /// </summary>
        [Required(ErrorMessage = "Survey start date is required")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date of the survey.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of participants allowed.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Maximum participants must be greater than 0")]
        public int? MaxParticipants { get; set; }

        /// <summary>
        /// Gets or sets the activity IDs to include in the survey.
        /// </summary>
        [Required(ErrorMessage = "At least one activity is required")]
        [MinLength(1, ErrorMessage = "At least one activity is required")]
        public List<Guid> ActivityIds { get; set; } = new List<Guid>();

        /// <summary>
        /// Validates the request data.
        /// </summary>
        /// <returns>True if the request is valid; otherwise, false.</returns>
        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(Title))
                return false;

            if (StartDate == default)
                return false;

            if (EndDate.HasValue && EndDate.Value <= StartDate)
                return false;

            if (ActivityIds == null || ActivityIds.Count == 0)
                return false;

            return true;
        }
    }
}