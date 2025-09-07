using System.ComponentModel.DataAnnotations;

namespace FunnyActivities.Application.DTOs.ActivityManagement
{
    /// <summary>
    /// Request model for uploading a video for an activity.
    /// </summary>
    public class UploadActivityVideoRequest
    {
        /// <summary>
        /// Gets or sets the activity ID.
        /// </summary>
        [Required(ErrorMessage = "Activity ID is required.")]
        public Guid ActivityId { get; set; }

        /// <summary>
        /// Gets or sets the video file data.
        /// </summary>
        [Required(ErrorMessage = "Video file is required.")]
        public byte[] VideoData { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// Gets or sets the video file name.
        /// </summary>
        [Required(ErrorMessage = "Video file name is required.")]
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the video content type.
        /// </summary>
        [Required(ErrorMessage = "Video content type is required.")]
        public string ContentType { get; set; } = string.Empty;
    }
}