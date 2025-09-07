using MediatR;
using FunnyActivities.Application.DTOs.MigrationManagement;

namespace FunnyActivities.Application.Commands.MigrationManagement
{
    /// <summary>
    /// Command for migrating a single material to the new BaseProduct/ProductVariant model.
    /// </summary>
    public class MigrateMaterialToProductVariantCommand : IRequest<MigrationResultDto>
    {
        /// <summary>
        /// Gets or sets the ID of the material to migrate.
        /// </summary>
        public Guid MaterialId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user performing the migration.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to skip validation checks.
        /// </summary>
        public bool SkipValidation { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to force migration even if data issues exist.
        /// </summary>
        public bool ForceMigration { get; set; } = false;
    }
}