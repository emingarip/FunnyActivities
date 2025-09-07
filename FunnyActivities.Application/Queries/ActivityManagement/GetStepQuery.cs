using System;
using MediatR;
using FunnyActivities.Application.DTOs.ActivityManagement;

namespace FunnyActivities.Application.Queries.ActivityManagement
{
    /// <summary>
    /// Query for retrieving a single step by ID.
    /// </summary>
    public class GetStepQuery : IRequest<StepDto>
    {
        /// <summary>
        /// Gets or sets the ID of the step to retrieve.
        /// </summary>
        public Guid Id { get; set; }
    }
}