using System;
using MediatR;
using FunnyActivities.Application.DTOs.ActivityManagement;

namespace FunnyActivities.Application.Queries.ActivityManagement
{
    /// <summary>
    /// Query for retrieving all activity product variants for a specific activity.
    /// </summary>
    public class GetActivityProductVariantsByActivityIdQuery : IRequest<List<ActivityProductVariantDto>>
    {
        /// <summary>
        /// Gets or sets the activity ID.
        /// </summary>
        public Guid ActivityId { get; set; }
    }
}