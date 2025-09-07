using System;
using MediatR;
using FunnyActivities.Application.DTOs.ActivityManagement;

namespace FunnyActivities.Application.Queries.ActivityManagement
{
    /// <summary>
    /// Query for retrieving a single activity category by ID.
    /// </summary>
    public class GetActivityCategoryQuery : IRequest<ActivityCategoryDto>
    {
        /// <summary>
        /// Gets or sets the ID of the activity category to retrieve.
        /// </summary>
        public Guid Id { get; set; }
    }
}