using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FunnyActivities.Application.DTOs.SurveyManagement
{
    /// <summary>
    /// Request DTO for updating an existing survey.
    /// </summary>
    public class UpdateSurveyRequest
    {
        /// <summary>
        /// Gets or sets the title of the survey.
        /// </summary>
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
        public DateTime? StartDate { get; set; }

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
        public List<Guid> ActivityIds { get; set; } = new List<Guid>();

        /// <summary>
        /// Validates the request data.
        /// </summary>
        /// <returns>True if the request is valid; otherwise, false.</returns>
        public bool IsValid()
        {
            // At least one field should be provided for update
            if (string.IsNullOrWhiteSpace(Title) &&
                string.IsNullOrWhiteSpace(Description) &&
                !StartDate.HasValue &&
                !EndDate.HasValue &&
                !MaxParticipants.HasValue &&
                (ActivityIds == null || ActivityIds.Count == 0))
            {
                return false;
            }

            // Validate date logic if both dates are provided
            if (StartDate.HasValue && EndDate.HasValue && EndDate.Value <= StartDate.Value)
                return false;

            return true;
        }
    }
}