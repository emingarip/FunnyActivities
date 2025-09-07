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
    /// Repository for BaseProduct entity operations.
    /// </summary>
    public class BaseProductRepository : IBaseProductRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseProductRepository"/> class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public BaseProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets a base product by its ID.
        /// </summary>
        /// <param name="id">The base product ID.</param>
        /// <returns>The base product if found; otherwise, null.</returns>
        public async Task<BaseProduct> GetByIdAsync(Guid id)
        {
            return await _context.BaseProducts
                .Include(bp => bp.Category)
                .Include(bp => bp.Variants)
                .FirstOrDefaultAsync(bp => bp.Id == id);
        }

        /// <summary>
        /// Gets all base products.
        /// </summary>
        /// <returns>A collection of all base products.</returns>
        public async Task<IEnumerable<BaseProduct>> GetAllAsync()
        {
            return await _context.BaseProducts
                .Include(bp => bp.Category)
                .Include(bp => bp.Variants)
                .ToListAsync();
        }

        /// <summary>
        /// Gets base products by category ID.
        /// </summary>
        /// <param name="categoryId">The category ID.</param>
        /// <returns>A collection of base products for the specified category.</returns>
        public async Task<IEnumerable<BaseProduct>> GetByCategoryIdAsync(Guid categoryId)
        {
            return await _context.BaseProducts
                .Include(bp => bp.Category)
                .Include(bp => bp.Variants)
                .Where(bp => bp.CategoryId == categoryId)
                .ToListAsync();
        }

        /// <summary>
        /// Adds a new base product.
        /// </summary>
        /// <param name="baseProduct">The base product to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AddAsync(BaseProduct baseProduct)
        {
            await _context.BaseProducts.AddAsync(baseProduct);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Updates an existing base product.
        /// </summary>
        /// <param name="baseProduct">The base product to update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task UpdateAsync(BaseProduct baseProduct)
        {
            _context.BaseProducts.Update(baseProduct);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes a base product.
        /// </summary>
        /// <param name="baseProduct">The base product to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DeleteAsync(BaseProduct baseProduct)
        {
            _context.BaseProducts.Remove(baseProduct);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Checks if a base product exists by its ID.
        /// </summary>
        /// <param name="id">The base product ID.</param>
        /// <returns>True if the base product exists; otherwise, false.</returns>
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.BaseProducts.AnyAsync(bp => bp.Id == id);
        }

        /// <summary>
        /// Gets a base product by name.
        /// </summary>
        /// <param name="name">The base product name.</param>
        /// <returns>The base product if found; otherwise, null.</returns>
        public async Task<BaseProduct> GetByNameAsync(string name)
        {
            return await _context.BaseProducts
                .Include(bp => bp.Category)
                .Include(bp => bp.Variants)
                .FirstOrDefaultAsync(bp => bp.Name.ToLower() == name.ToLower());
        }

        /// <summary>
        /// Gets a paged list of base products with optional filtering.
        /// </summary>
        /// <param name="searchTerm">The search term for filtering by name or description.</param>
        /// <param name="categoryId">The category ID for filtering.</param>
        /// <param name="pageNumber">The page number (1-based).</param>
        /// <param name="pageSize">The page size.</param>
        /// <returns>A list of base products.</returns>
        public async Task<List<BaseProduct>> GetPagedAsync(string? searchTerm, Guid? categoryId, int pageNumber, int pageSize)
        {
            var query = _context.BaseProducts
                .Include(bp => bp.Category)
                .Include(bp => bp.Variants)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(bp =>
                    bp.Name.Contains(searchTerm) ||
                    (bp.Description != null && bp.Description.Contains(searchTerm)));
            }

            // Apply category filter
            if (categoryId.HasValue)
            {
                query = query.Where(bp => bp.CategoryId == categoryId.Value);
            }

            // Apply pagination
            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// Checks if a base product exists by name.
        /// </summary>
        /// <param name="name">The name to check.</param>
        /// <returns>True if exists; otherwise, false.</returns>
        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.BaseProducts
                .AnyAsync(bp => bp.Name.ToLower() == name.ToLower());
        }

        /// <summary>
        /// Checks if a base product exists by name excluding a specific ID.
        /// </summary>
        /// <param name="name">The name to check.</param>
        /// <param name="excludeId">The ID to exclude from the check.</param>
        /// <returns>True if exists; otherwise, false.</returns>
        public async Task<bool> ExistsByNameExcludingIdAsync(string name, Guid excludeId)
        {
            return await _context.BaseProducts
                .AnyAsync(bp => bp.Name.ToLower() == name.ToLower() && bp.Id != excludeId);
        }
    }
}