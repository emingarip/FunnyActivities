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
    }
}