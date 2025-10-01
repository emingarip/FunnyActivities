using System.ComponentModel.DataAnnotations;

namespace FunnyActivities.Application.DTOs.SurveyManagement
{
    /// <summary>
    /// Request DTO for registering a survey participant.
    /// </summary>
    public class RegisterParticipantRequest
    {
        /// <summary>
        /// Gets or sets the participant's first name.
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the participant's last name.
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the number of children.
        /// </summary>
        [Range(0, 20)]
        public int ChildrenCount { get; set; }
    }
}