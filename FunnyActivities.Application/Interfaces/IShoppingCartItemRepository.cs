using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Application.Interfaces
{
    /// <summary>
    /// Repository interface for ShoppingCartItem entity operations.
    /// </summary>
    public interface IShoppingCartItemRepository
    {
        /// <summary>
        /// Gets a shopping cart item by its ID.
        /// </summary>
        /// <param name="id">The shopping cart item ID.</param>
        /// <returns>The shopping cart item if found; otherwise, null.</returns>
        Task<ShoppingCartItem> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets all shopping cart items.
        /// </summary>
        /// <returns>A collection of all shopping cart items.</returns>
        Task<IEnumerable<ShoppingCartItem>> GetAllAsync();

        /// <summary>
        /// Gets shopping cart items by user ID.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>A collection of shopping cart items for the specified user.</returns>
        Task<IEnumerable<ShoppingCartItem>> GetByUserIdAsync(Guid userId);

        /// <summary>
        /// Gets a shopping cart item by user ID and product variant ID.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="productVariantId">The product variant ID.</param>
        /// <returns>The shopping cart item if found; otherwise, null.</returns>
        Task<ShoppingCartItem> GetByUserAndProductVariantAsync(Guid userId, Guid productVariantId);

        /// <summary>
        /// Adds a new shopping cart item.
        /// </summary>
        /// <param name="shoppingCartItem">The shopping cart item to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddAsync(ShoppingCartItem shoppingCartItem);

        /// <summary>
        /// Updates an existing shopping cart item.
        /// </summary>
        /// <param name="shoppingCartItem">The shopping cart item to update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateAsync(ShoppingCartItem shoppingCartItem);

        /// <summary>
        /// Deletes a shopping cart item.
        /// </summary>
        /// <param name="shoppingCartItem">The shopping cart item to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeleteAsync(ShoppingCartItem shoppingCartItem);

        /// <summary>
        /// Checks if a shopping cart item exists by its ID.
        /// </summary>
        /// <param name="id">The shopping cart item ID.</param>
        /// <returns>True if the shopping cart item exists; otherwise, false.</returns>
        Task<bool> ExistsAsync(Guid id);

        /// <summary>
        /// Deletes all shopping cart items for a user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeleteByUserIdAsync(Guid userId);
    }
}