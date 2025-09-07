using System;
using MediatR;
using FunnyActivities.Application.DTOs.ActivityManagement;

namespace FunnyActivities.Application.Queries.ActivityManagement
{
    /// <summary>
    /// Query for retrieving all steps for a specific activity.
    /// </summary>
    public class GetStepsByActivityIdQuery : IRequest<List<StepDto>>
    {
        /// <summary>
        /// Gets or sets the activity ID.
        /// </summary>
        public Guid ActivityId { get; set; }
    }
}