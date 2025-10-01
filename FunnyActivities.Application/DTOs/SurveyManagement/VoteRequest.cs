using System;
using System.ComponentModel.DataAnnotations;

namespace FunnyActivities.Application.DTOs.SurveyManagement
{
    /// <summary>
    /// Request DTO for voting on survey activities.
    /// </summary>
    public class VoteRequest
    {
        /// <summary>
        /// Gets or sets the survey activity ID to vote for.
        /// </summary>
        [Required(ErrorMessage = "Survey activity ID is required")]
        public Guid SurveyActivityId { get; set; }

        /// <summary>
        /// Gets or sets the vote value (1-5 scale).
        /// </summary>
        [Required(ErrorMessage = "Vote value is required")]
        [Range(1, 5, ErrorMessage = "Vote value must be between 1 and 5")]
        public int VoteValue { get; set; }

        /// <summary>
        /// Gets or sets the survey participant ID.
        /// </summary>
        [Required(ErrorMessage = "Survey participant ID is required")]
        public Guid SurveyParticipantId { get; set; }


        /// <summary>
        /// Validates the request data.
        /// </summary>
        /// <returns>True if the request is valid; otherwise, false.</returns>
        public bool IsValid()
        {
            if (SurveyActivityId == Guid.Empty)
                return false;

            if (VoteValue < 1 || VoteValue > 5)
                return false;

            // Comment is optional - no validation required

            return true;
        }
    }
}