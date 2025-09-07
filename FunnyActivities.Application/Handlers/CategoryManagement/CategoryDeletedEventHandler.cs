using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Domain.Events;

namespace FunnyActivities.Application.Handlers.CategoryManagement
{
    /// <summary>
    /// Handler for CategoryDeletedEvent.
    /// </summary>
    public class CategoryDeletedEventHandler : INotificationHandler<CategoryDeletedEvent>
    {
        private readonly ILogger<CategoryDeletedEventHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryDeletedEventHandler"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public CategoryDeletedEventHandler(ILogger<CategoryDeletedEventHandler> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Handles the category deleted event.
        /// </summary>
        /// <param name="notification">The category deleted event.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task Handle(CategoryDeletedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Category deleted: {CategoryId} - {Name} at {OccurredOn}",
                notification.CategoryId, notification.Name, notification.OccurredOn);

            // Additional event handling logic can be added here
            // For example: cleaning up related data, sending notifications, etc.

            return Task.CompletedTask;
        }
    }
}