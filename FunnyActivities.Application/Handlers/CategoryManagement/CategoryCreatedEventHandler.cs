using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Domain.Events;

namespace FunnyActivities.Application.Handlers.CategoryManagement
{
    /// <summary>
    /// Handler for CategoryCreatedEvent.
    /// </summary>
    public class CategoryCreatedEventHandler : INotificationHandler<CategoryCreatedEvent>
    {
        private readonly ILogger<CategoryCreatedEventHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryCreatedEventHandler"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public CategoryCreatedEventHandler(ILogger<CategoryCreatedEventHandler> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Handles the category created event.
        /// </summary>
        /// <param name="notification">The category created event.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task Handle(CategoryCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Category created: {CategoryId} - {Name} at {OccurredOn}",
                notification.CategoryId, notification.Name, notification.OccurredOn);

            // Additional event handling logic can be added here
            // For example: sending notifications, updating caches, etc.

            return Task.CompletedTask;
        }
    }
}