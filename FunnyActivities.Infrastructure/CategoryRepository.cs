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
    /// Repository for Category entity operations.
    /// </summary>
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryRepository"/> class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public CategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets a category by its ID.
        /// </summary>
        /// <param name="id">The category ID.</param>
        /// <returns>The category if found; otherwise, null.</returns>
        public async Task<Category?> GetByIdAsync(Guid id)
        {
            return await _context.Categories.FindAsync(id);
        }

        /// <summary>
        /// Gets a category by its name.
        /// </summary>
        /// <param name="name">The category name.</param>
        /// <returns>The category if found; otherwise, null.</returns>
        public async Task<Category?> GetByNameAsync(string name)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
        }

        /// <summary>
        /// Gets all categories.
        /// </summary>
        /// <returns>A list of all categories.</returns>
        public async Task<List<Category>> GetAllAsync()
        {
            return await _context.Categories.ToListAsync();
        }

        /// <summary>
        /// Checks if a category exists by name.
        /// </summary>
        /// <param name="name">The name to check.</param>
        /// <returns>True if exists; otherwise, false.</returns>
        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.Categories.AnyAsync(c => c.Name.ToLower() == name.ToLower());
        }

        /// <summary>
        /// Checks if a category exists by name excluding a specific ID.
        /// </summary>
        /// <param name="name">The name to check.</param>
        /// <param name="excludeId">The ID to exclude from the check.</param>
        /// <returns>True if exists; otherwise, false.</returns>
        public async Task<bool> ExistsByNameExcludingIdAsync(string name, Guid excludeId)
        {
            return await _context.Categories.AnyAsync(c => c.Name.ToLower() == name.ToLower() && c.Id != excludeId);
        }

        /// <summary>
        /// Adds a new category.
        /// </summary>
        /// <param name="category">The category to add.</param>
        public async Task AddAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Updates an existing category.
        /// </summary>
        /// <param name="category">The category to update.</param>
        public async Task UpdateAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes a category.
        /// </summary>
        /// <param name="category">The category to delete.</param>
        public async Task DeleteAsync(Category category)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }
}