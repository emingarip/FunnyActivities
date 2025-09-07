using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Domain.Events;

namespace FunnyActivities.Application.Handlers.CategoryManagement
{
    /// <summary>
    /// Handler for CategoryUpdatedEvent.
    /// </summary>
    public class CategoryUpdatedEventHandler : INotificationHandler<CategoryUpdatedEvent>
    {
        private readonly ILogger<CategoryUpdatedEventHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryUpdatedEventHandler"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public CategoryUpdatedEventHandler(ILogger<CategoryUpdatedEventHandler> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Handles the category updated event.
        /// </summary>
        /// <param name="notification">The category updated event.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task Handle(CategoryUpdatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Category updated: {CategoryId} - {Name} at {OccurredOn}",
                notification.CategoryId, notification.Name, notification.OccurredOn);

            // Additional event handling logic can be added here
            // For example: invalidating caches, sending notifications, etc.

            return Task.CompletedTask;
        }
    }
}