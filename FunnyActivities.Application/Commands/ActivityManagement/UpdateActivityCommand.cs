using System;
using MediatR;
using FunnyActivities.Application.DTOs.ActivityManagement;

namespace FunnyActivities.Application.Commands.ActivityManagement
{
    /// <summary>
    /// Command for updating an existing activity.
    /// </summary>
    public class UpdateActivityCommand : IRequest<ActivityDto>
    {
        /// <summary>
        /// Gets or sets the ID of the activity to update.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the activity.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the activity.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the video URL of the activity.
        /// </summary>
        public string? VideoUrl { get; set; }

        /// <summary>
        /// Gets or sets the duration of the activity in hours.
        /// </summary>
        public int? DurationHours { get; set; }

        /// <summary>
        /// Gets or sets the duration of the activity in minutes.
        /// </summary>
        public int? DurationMinutes { get; set; }

        /// <summary>
        /// Gets or sets the duration of the activity in seconds.
        /// </summary>
        public int? DurationSeconds { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user updating the activity.
        /// </summary>
        public Guid UserId { get; set; }
    }
}