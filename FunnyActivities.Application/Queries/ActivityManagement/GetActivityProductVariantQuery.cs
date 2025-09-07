using System;
using MediatR;
using FunnyActivities.Application.DTOs.ActivityManagement;

namespace FunnyActivities.Application.Queries.ActivityManagement
{
    /// <summary>
    /// Query for retrieving a single activity product variant by ID.
    /// </summary>
    public class GetActivityProductVariantQuery : IRequest<ActivityProductVariantDto>
    {
        /// <summary>
        /// Gets or sets the ID of the activity product variant to retrieve.
        /// </summary>
        public Guid Id { get; set; }
    }
}