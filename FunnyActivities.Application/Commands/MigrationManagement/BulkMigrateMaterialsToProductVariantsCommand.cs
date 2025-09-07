using MediatR;
using FunnyActivities.Application.DTOs.MigrationManagement;

namespace FunnyActivities.Application.Commands.MigrationManagement
{
    /// <summary>
    /// Command for bulk migrating materials to the new BaseProduct/ProductVariant model.
    /// </summary>
    public class BulkMigrateMaterialsToProductVariantsCommand : IRequest<BulkMigrationResultDto>
    {
        /// <summary>
        /// Gets or sets the list of material IDs to migrate. If null or empty, migrates all materials.
        /// </summary>
        public List<Guid>? MaterialIds { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user performing the migration.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the batch size for processing materials.
        /// </summary>
        public int BatchSize { get; set; } = 10;

        /// <summary>
        /// Gets or sets a value indicating whether to skip validation checks.
        /// </summary>
        public bool SkipValidation { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to continue processing on individual failures.
        /// </summary>
        public bool ContinueOnError { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to force migration even if data issues exist.
        /// </summary>
        public bool ForceMigration { get; set; } = false;
    }
}