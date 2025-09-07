using System;

namespace FunnyActivities.Application.DTOs.MigrationManagement
{
    /// <summary>
    /// DTO for migration operation result.
    /// </summary>
    public class MigrationResultDto
    {
        /// <summary>
        /// Gets or sets the ID of the migrated material.
        /// </summary>
        public Guid MaterialId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the created base product.
        /// </summary>
        public Guid BaseProductId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the created product variant.
        /// </summary>
        public Guid ProductVariantId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the migration was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the error message if migration failed.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the migration.
        /// </summary>
        public DateTime MigratedAt { get; set; }
    }
}