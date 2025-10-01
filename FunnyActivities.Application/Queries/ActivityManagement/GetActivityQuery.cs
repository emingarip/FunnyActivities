using System;
using MediatR;
using FunnyActivities.Application.DTOs.ActivityManagement;

namespace FunnyActivities.Application.Queries.ActivityManagement
{
    /// <summary>
    /// Query for retrieving a single activity by ID.
    /// </summary>
    public class GetActivityQuery : IRequest<ActivityDto>
    {
        /// <summary>
        /// Gets or sets the ID of the activity to retrieve.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is a public query (no authentication required).
        /// </summary>
        public bool IsPublicRequest { get; set; } = false;
    }
}