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
    /// Repository for UnitOfMeasure entity operations.
    /// </summary>
    public class UnitOfMeasureRepository : IUnitOfMeasureRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfMeasureRepository"/> class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public UnitOfMeasureRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets a unit of measure by its ID.
        /// </summary>
        /// <param name="id">The unit of measure ID.</param>
        /// <returns>The unit of measure if found; otherwise, null.</returns>
        public async Task<UnitOfMeasure> GetByIdAsync(Guid id)
        {
            return await _context.UnitOfMeasures.FindAsync(id);
        }

        /// <summary>
        /// Gets all units of measure.
        /// </summary>
        /// <returns>A collection of all units of measure.</returns>
        public async Task<IEnumerable<UnitOfMeasure>> GetAllAsync()
        {
            return await _context.UnitOfMeasures.ToListAsync();
        }

        /// <summary>
        /// Gets a unit of measure by name.
        /// </summary>
        /// <param name="name">The unit of measure name.</param>
        /// <returns>The unit of measure if found; otherwise, null.</returns>
        public async Task<UnitOfMeasure> GetByNameAsync(string name)
        {
            return await _context.UnitOfMeasures
                .FirstOrDefaultAsync(uom => uom.Name.ToLower() == name.ToLower());
        }

        /// <summary>
        /// Adds a new unit of measure.
        /// </summary>
        /// <param name="unitOfMeasure">The unit of measure to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AddAsync(UnitOfMeasure unitOfMeasure)
        {
            await _context.UnitOfMeasures.AddAsync(unitOfMeasure);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Updates an existing unit of measure.
        /// </summary>
        /// <param name="unitOfMeasure">The unit of measure to update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task UpdateAsync(UnitOfMeasure unitOfMeasure)
        {
            _context.UnitOfMeasures.Update(unitOfMeasure);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes a unit of measure.
        /// </summary>
        /// <param name="unitOfMeasure">The unit of measure to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DeleteAsync(UnitOfMeasure unitOfMeasure)
        {
            _context.UnitOfMeasures.Remove(unitOfMeasure);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Checks if a unit of measure exists by its ID.
        /// </summary>
        /// <param name="id">The unit of measure ID.</param>
        /// <returns>True if the unit of measure exists; otherwise, false.</returns>
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.UnitOfMeasures.AnyAsync(uom => uom.Id == id);
        }

        /// <summary>
        /// Checks if a unit of measure exists by name.
        /// </summary>
        /// <param name="name">The unit of measure name.</param>
        /// <returns>True if the unit of measure exists; otherwise, false.</returns>
        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.UnitOfMeasures.AnyAsync(uom => uom.Name.ToLower() == name.ToLower());
        }
    }
}