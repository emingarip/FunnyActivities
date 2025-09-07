using MediatR;

namespace FunnyActivities.Application.Commands.ShoppingCartManagement
{
    /// <summary>
    /// Command for removing an item from the shopping cart.
    /// </summary>
    public class RemoveFromCartCommand : IRequest<Unit>
    {
        /// <summary>
        /// Gets or sets the cart item ID.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the user ID.
        /// </summary>
        public Guid UserId { get; set; }
    }
}