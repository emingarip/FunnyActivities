using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FunnyActivities.Application.Interfaces
{
    /// <summary>
    /// Repository interface for accessing Material data during migration.
    /// </summary>
    public interface IMaterialRepository
    {
        /// <summary>
        /// Gets a material by its ID.
        /// </summary>
        /// <param name="id">The material ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The material data if found; otherwise, null.</returns>
        Task<MaterialData?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all material IDs for migration.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A list of all material IDs.</returns>
        Task<List<Guid>> GetAllIdsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets materials in batches for migration.
        /// </summary>
        /// <param name="skip">The number of materials to skip.</param>
        /// <param name="take">The number of materials to take.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A list of material data.</returns>
        Task<List<MaterialData>> GetBatchAsync(int skip, int take, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Represents material data for migration purposes.
    /// </summary>
    public class MaterialData
    {
        /// <summary>
        /// Gets or sets the ID of the material.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the material.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the material.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the category ID of the material.
        /// </summary>
        public Guid? CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the unit type of the material.
        /// </summary>
        public string UnitType { get; set; }

        /// <summary>
        /// Gets or sets the unit value of the material.
        /// </summary>
        public decimal UnitValue { get; set; }

        /// <summary>
        /// Gets or sets the stock quantity of the material.
        /// </summary>
        public decimal StockQuantity { get; set; }

        /// <summary>
        /// Gets or sets the usage notes of the material.
        /// </summary>
        public string? UsageNotes { get; set; }

        /// <summary>
        /// Gets or sets the photos JSON of the material.
        /// </summary>
        public string Photos { get; set; }

        /// <summary>
        /// Gets or sets the dynamic properties JSON of the material.
        /// </summary>
        public string DynamicProperties { get; set; }
    }
}