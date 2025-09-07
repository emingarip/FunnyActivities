using System;
using MediatR;
using FunnyActivities.Application.DTOs.ActivityManagement;

namespace FunnyActivities.Application.Queries.ActivityManagement
{
    /// <summary>
    /// Query for retrieving a single activity with full details including steps and product variants.
    /// </summary>
    public class GetActivityWithDetailsQuery : IRequest<ActivityWithDetailsDto>
    {
        /// <summary>
        /// Gets or sets the ID of the activity to retrieve.
        /// </summary>
        public Guid Id { get; set; }
    }
}