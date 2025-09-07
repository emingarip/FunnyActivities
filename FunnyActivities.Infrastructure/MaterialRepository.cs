using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using FunnyActivities.Application.Interfaces;

namespace FunnyActivities.Infrastructure
{
    /// <summary>
    /// Repository for accessing Material data during migration.
    /// </summary>
    public class MaterialRepository : IMaterialRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialRepository"/> class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public MaterialRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets a material by its ID.
        /// </summary>
        /// <param name="id">The material ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The material data if found; otherwise, null.</returns>
        public async Task<MaterialData?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var sql = @"
                SELECT ""Id"", ""Name"", ""Description"", ""CategoryId"", ""UnitType"", ""UnitValue"", ""StockQuantity"", ""UsageNotes"", ""Photos"", ""DynamicProperties""
                FROM ""Materials""
                WHERE ""Id"" = @materialId";

            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;
            command.Parameters.Add(new NpgsqlParameter("@materialId", id));

            await _context.Database.OpenConnectionAsync(cancellationToken);

            try
            {
                using var reader = await command.ExecuteReaderAsync(cancellationToken);
                if (await reader.ReadAsync(cancellationToken))
                {
                    return new MaterialData
                    {
                        Id = reader.GetGuid(0),
                        Name = reader.GetString(1),
                        Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                        CategoryId = reader.IsDBNull(3) ? null : (Guid?)reader.GetGuid(3),
                        UnitType = reader.GetString(4),
                        UnitValue = reader.GetDecimal(5),
                        StockQuantity = reader.GetDecimal(6),
                        UsageNotes = reader.IsDBNull(7) ? null : reader.GetString(7),
                        Photos = reader.GetString(8),
                        DynamicProperties = reader.GetString(9)
                    };
                }
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }

            return null;
        }

        /// <summary>
        /// Gets all material IDs for migration.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A list of all material IDs.</returns>
        public async Task<List<Guid>> GetAllIdsAsync(CancellationToken cancellationToken = default)
        {
            var sql = @"SELECT ""Id"" FROM ""Materials"" ORDER BY ""CreatedAt""";

            var materialIds = new List<Guid>();

            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;

            await _context.Database.OpenConnectionAsync(cancellationToken);

            try
            {
                using var reader = await command.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    materialIds.Add(reader.GetGuid(0));
                }
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }

            return materialIds;
        }

        /// <summary>
        /// Gets materials in batches for migration.
        /// </summary>
        /// <param name="skip">The number of materials to skip.</param>
        /// <param name="take">The number of materials to take.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A list of material data.</returns>
        public async Task<List<MaterialData>> GetBatchAsync(int skip, int take, CancellationToken cancellationToken = default)
        {
            var sql = @"
                SELECT ""Id"", ""Name"", ""Description"", ""CategoryId"", ""UnitType"", ""UnitValue"", ""StockQuantity"", ""UsageNotes"", ""Photos"", ""DynamicProperties""
                FROM ""Materials""
                ORDER BY ""CreatedAt""
                OFFSET @skip LIMIT @take";

            var materials = new List<MaterialData>();

            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;
            command.Parameters.Add(new NpgsqlParameter("@skip", skip));
            command.Parameters.Add(new NpgsqlParameter("@take", take));

            await _context.Database.OpenConnectionAsync(cancellationToken);

            try
            {
                using var reader = await command.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    materials.Add(new MaterialData
                    {
                        Id = reader.GetGuid(0),
                        Name = reader.GetString(1),
                        Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                        CategoryId = reader.IsDBNull(3) ? null : (Guid?)reader.GetGuid(3),
                        UnitType = reader.GetString(4),
                        UnitValue = reader.GetDecimal(5),
                        StockQuantity = reader.GetDecimal(6),
                        UsageNotes = reader.IsDBNull(7) ? null : reader.GetString(7),
                        Photos = reader.GetString(8),
                        DynamicProperties = reader.GetString(9)
                    });
                }
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }

            return materials;
        }
    }
}