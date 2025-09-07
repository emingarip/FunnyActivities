using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Application.Interfaces
{
    /// <summary>
    /// Repository interface for UnitOfMeasure entity operations.
    /// </summary>
    public interface IUnitOfMeasureRepository
    {
        /// <summary>
        /// Gets a unit of measure by its ID.
        /// </summary>
        /// <param name="id">The unit of measure ID.</param>
        /// <returns>The unit of measure if found; otherwise, null.</returns>
        Task<UnitOfMeasure> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets all units of measure.
        /// </summary>
        /// <returns>A collection of all units of measure.</returns>
        Task<IEnumerable<UnitOfMeasure>> GetAllAsync();

        /// <summary>
        /// Gets a unit of measure by name.
        /// </summary>
        /// <param name="name">The unit of measure name.</param>
        /// <returns>The unit of measure if found; otherwise, null.</returns>
        Task<UnitOfMeasure> GetByNameAsync(string name);

        /// <summary>
        /// Adds a new unit of measure.
        /// </summary>
        /// <param name="unitOfMeasure">The unit of measure to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddAsync(UnitOfMeasure unitOfMeasure);

        /// <summary>
        /// Updates an existing unit of measure.
        /// </summary>
        /// <param name="unitOfMeasure">The unit of measure to update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateAsync(UnitOfMeasure unitOfMeasure);

        /// <summary>
        /// Deletes a unit of measure.
        /// </summary>
        /// <param name="unitOfMeasure">The unit of measure to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeleteAsync(UnitOfMeasure unitOfMeasure);

        /// <summary>
        /// Checks if a unit of measure exists by its ID.
        /// </summary>
        /// <param name="id">The unit of measure ID.</param>
        /// <returns>True if the unit of measure exists; otherwise, false.</returns>
        Task<bool> ExistsAsync(Guid id);

        /// <summary>
        /// Checks if a unit of measure exists by name.
        /// </summary>
        /// <param name="name">The unit of measure name.</param>
        /// <returns>True if the unit of measure exists; otherwise, false.</returns>
        Task<bool> ExistsByNameAsync(string name);
    }
}