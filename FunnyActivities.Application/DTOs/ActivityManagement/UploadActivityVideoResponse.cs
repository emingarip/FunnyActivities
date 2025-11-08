namespace FunnyActivities.Application.DTOs.ActivityManagement
{
    /// <summary>
    /// Response model for video upload operation.
    /// </summary>
    public class UploadActivityVideoResponse
    {
        /// <summary>
        /// Gets or sets the activity ID.
        /// </summary>
        public Guid ActivityId { get; set; }

        /// <summary>
        /// Gets or sets the video object key in storage.
        /// </summary>
        public string VideoObjectKey { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of video that was uploaded (main or intro).
        /// </summary>
        public string VideoType { get; set; } = "main";

        /// <summary>
        /// Gets or sets the signed URL for accessing the video.
        /// </summary>
        public string SignedVideoUrl { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the expiry time of the signed URL in seconds.
        /// </summary>
        public int UrlExpirySeconds { get; set; }

        /// <summary>
        /// Gets or sets the upload timestamp.
        /// </summary>
        public DateTime UploadedAt { get; set; }
    }
}
