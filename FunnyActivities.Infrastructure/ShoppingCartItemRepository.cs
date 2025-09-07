using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Application.Interfaces;

namespace FunnyActivities.Infrastructure
{
    /// <summary>
    /// Repository for ShoppingCartItem entity operations.
    /// </summary>
    public class ShoppingCartItemRepository : IShoppingCartItemRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShoppingCartItemRepository"/> class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public ShoppingCartItemRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets a shopping cart item by its ID.
        /// </summary>
        /// <param name="id">The shopping cart item ID.</param>
        /// <returns>The shopping cart item if found; otherwise, null.</returns>
        public async Task<ShoppingCartItem> GetByIdAsync(Guid id)
        {
            return await _context.ShoppingCartItems
                .Include(sci => sci.ProductVariant)
                .Include(sci => sci.User)
                .FirstOrDefaultAsync(sci => sci.Id == id);
        }

        /// <summary>
        /// Gets all shopping cart items.
        /// </summary>
        /// <returns>A collection of all shopping cart items.</returns>
        public async Task<IEnumerable<ShoppingCartItem>> GetAllAsync()
        {
            return await _context.ShoppingCartItems
                .Include(sci => sci.ProductVariant)
                .Include(sci => sci.User)
                .ToListAsync();
        }

        /// <summary>
        /// Gets shopping cart items by user ID.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>A collection of shopping cart items for the specified user.</returns>
        public async Task<IEnumerable<ShoppingCartItem>> GetByUserIdAsync(Guid userId)
        {
            return await _context.ShoppingCartItems
                .Include(sci => sci.ProductVariant)
                .Include(sci => sci.User)
                .Where(sci => sci.UserId == userId)
                .ToListAsync();
        }

        /// <summary>
        /// Gets a shopping cart item by user ID and product variant ID.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="productVariantId">The product variant ID.</param>
        /// <returns>The shopping cart item if found; otherwise, null.</returns>
        public async Task<ShoppingCartItem> GetByUserAndProductVariantAsync(Guid userId, Guid productVariantId)
        {
            return await _context.ShoppingCartItems
                .Include(sci => sci.ProductVariant)
                .Include(sci => sci.User)
                .FirstOrDefaultAsync(sci => sci.UserId == userId && sci.ProductVariantId == productVariantId);
        }

        /// <summary>
        /// Adds a new shopping cart item.
        /// </summary>
        /// <param name="shoppingCartItem">The shopping cart item to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AddAsync(ShoppingCartItem shoppingCartItem)
        {
            await _context.ShoppingCartItems.AddAsync(shoppingCartItem);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Updates an existing shopping cart item.
        /// </summary>
        /// <param name="shoppingCartItem">The shopping cart item to update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task UpdateAsync(ShoppingCartItem shoppingCartItem)
        {
            _context.ShoppingCartItems.Update(shoppingCartItem);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes a shopping cart item.
        /// </summary>
        /// <param name="shoppingCartItem">The shopping cart item to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DeleteAsync(ShoppingCartItem shoppingCartItem)
        {
            _context.ShoppingCartItems.Remove(shoppingCartItem);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Checks if a shopping cart item exists by its ID.
        /// </summary>
        /// <param name="id">The shopping cart item ID.</param>
        /// <returns>True if the shopping cart item exists; otherwise, false.</returns>
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.ShoppingCartItems.AnyAsync(sci => sci.Id == id);
        }

        /// <summary>
        /// Deletes all shopping cart items for a user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DeleteByUserIdAsync(Guid userId)
        {
            var items = await _context.ShoppingCartItems
                .Where(sci => sci.UserId == userId)
                .ToListAsync();

            _context.ShoppingCartItems.RemoveRange(items);
            await _context.SaveChangesAsync();
        }
    }
}