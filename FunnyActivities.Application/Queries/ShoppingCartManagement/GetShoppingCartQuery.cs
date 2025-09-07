using MediatR;
using FunnyActivities.Application.DTOs.ShoppingCartManagement;

namespace FunnyActivities.Application.Queries.ShoppingCartManagement
{
    /// <summary>
    /// Query for retrieving a user's shopping cart items.
    /// </summary>
    public class GetShoppingCartQuery : IRequest<List<ShoppingCartItemDto>>
    {
        /// <summary>
        /// Gets or sets the user ID.
        /// </summary>
        public Guid UserId { get; set; }
    }
}