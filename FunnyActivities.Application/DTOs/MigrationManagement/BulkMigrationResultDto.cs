using System;
using System.Collections.Generic;

namespace FunnyActivities.Application.DTOs.MigrationManagement
{
    /// <summary>
    /// DTO for bulk migration operation result.
    /// </summary>
    public class BulkMigrationResultDto
    {
        /// <summary>
        /// Gets or sets the total number of materials processed.
        /// </summary>
        public int TotalProcessed { get; set; }

        /// <summary>
        /// Gets or sets the number of successful migrations.
        /// </summary>
        public int SuccessfulMigrations { get; set; }

        /// <summary>
        /// Gets or sets the number of failed migrations.
        /// </summary>
        public int FailedMigrations { get; set; }

        /// <summary>
        /// Gets or sets the list of migration results.
        /// </summary>
        public List<MigrationResultDto> Results { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the bulk migration.
        /// </summary>
        public DateTime MigratedAt { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BulkMigrationResultDto"/> class.
        /// </summary>
        public BulkMigrationResultDto()
        {
            Results = new List<MigrationResultDto>();
            MigratedAt = DateTime.UtcNow;
        }
    }
}