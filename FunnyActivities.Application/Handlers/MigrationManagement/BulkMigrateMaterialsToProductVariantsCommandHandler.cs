using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Application.Commands.MigrationManagement;
using FunnyActivities.Application.DTOs.MigrationManagement;
using FunnyActivities.Domain.Exceptions;

namespace FunnyActivities.Application.Handlers.MigrationManagement
{
    /// <summary>
    /// Handler for bulk migrating materials to the new BaseProduct/ProductVariant model.
    /// </summary>
    public class BulkMigrateMaterialsToProductVariantsCommandHandler : IRequestHandler<BulkMigrateMaterialsToProductVariantsCommand, BulkMigrationResultDto>
    {
        private readonly IMaterialRepository _materialRepository;
        private readonly IBaseProductRepository _baseProductRepository;
        private readonly IProductVariantRepository _productVariantRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfMeasureRepository _unitOfMeasureRepository;
        private readonly IMediator _mediator;
        private readonly ILogger<BulkMigrateMaterialsToProductVariantsCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BulkMigrateMaterialsToProductVariantsCommandHandler"/> class.
        /// </summary>
        /// <param name="materialRepository">The material repository.</param>
        /// <param name="baseProductRepository">The base product repository.</param>
        /// <param name="productVariantRepository">The product variant repository.</param>
        /// <param name="categoryRepository">The category repository.</param>
        /// <param name="unitOfMeasureRepository">The unit of measure repository.</param>
        /// <param name="mediator">The mediator for sending commands.</param>
        /// <param name="logger">The logger.</param>
        public BulkMigrateMaterialsToProductVariantsCommandHandler(
            IMaterialRepository materialRepository,
            IBaseProductRepository baseProductRepository,
            IProductVariantRepository productVariantRepository,
            ICategoryRepository categoryRepository,
            IUnitOfMeasureRepository unitOfMeasureRepository,
            IMediator mediator,
            ILogger<BulkMigrateMaterialsToProductVariantsCommandHandler> _logger)
        {
            _materialRepository = materialRepository;
            _baseProductRepository = baseProductRepository;
            _productVariantRepository = productVariantRepository;
            _categoryRepository = categoryRepository;
            _unitOfMeasureRepository = unitOfMeasureRepository;
            _mediator = mediator;
            this._logger = _logger;
        }

        /// <summary>
        /// Handles the bulk migrate materials command.
        /// </summary>
        /// <param name="request">The bulk migrate materials command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The bulk migration result.</returns>
        public async Task<BulkMigrationResultDto> Handle(BulkMigrateMaterialsToProductVariantsCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting bulk migration of materials by user {UserId}. Batch size: {BatchSize}, Continue on error: {ContinueOnError}",
                request.UserId, request.BatchSize, request.ContinueOnError);

            var result = new BulkMigrationResultDto();

            // Get material IDs to process
            var materialIds = request.MaterialIds ?? await GetAllMaterialIdsAsync(cancellationToken);
            result.TotalProcessed = materialIds.Count;

            _logger.LogInformation("Found {Count} materials to migrate", materialIds.Count);

            // Process materials in batches
            var batches = materialIds.Chunk(request.BatchSize);

            foreach (var batch in batches)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Bulk migration cancelled");
                    break;
                }

                var batchResults = await ProcessBatchAsync(batch, request, cancellationToken);
                result.Results.AddRange(batchResults);

                var successfulInBatch = batchResults.Count(r => r.Success);
                var failedInBatch = batchResults.Count(r => !r.Success);

                result.SuccessfulMigrations += successfulInBatch;
                result.FailedMigrations += failedInBatch;

                _logger.LogInformation("Processed batch: {Successful} successful, {Failed} failed",
                    successfulInBatch, failedInBatch);

                // If not continuing on error and we had failures, stop processing
                if (!request.ContinueOnError && failedInBatch > 0)
                {
                    _logger.LogWarning("Stopping bulk migration due to errors in batch");
                    break;
                }
            }

            _logger.LogInformation("Bulk migration completed. Total: {Total}, Successful: {Successful}, Failed: {Failed}",
                result.TotalProcessed, result.SuccessfulMigrations, result.FailedMigrations);

            return result;
        }

        private async Task<List<Guid>> GetAllMaterialIdsAsync(CancellationToken cancellationToken)
        {
            return await _materialRepository.GetAllIdsAsync(cancellationToken);
        }

        private async Task<List<MigrationResultDto>> ProcessBatchAsync(Guid[] batch, BulkMigrateMaterialsToProductVariantsCommand request, CancellationToken cancellationToken)
        {
            var results = new List<MigrationResultDto>();

            foreach (var materialId in batch)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    var migrateCommand = new MigrateMaterialToProductVariantCommand
                    {
                        MaterialId = materialId,
                        UserId = request.UserId,
                        SkipValidation = request.SkipValidation,
                        ForceMigration = request.ForceMigration
                    };

                    var result = await _mediator.Send(migrateCommand, cancellationToken);
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error migrating material {MaterialId} in batch", materialId);

                    var errorResult = new MigrationResultDto
                    {
                        MaterialId = materialId,
                        Success = false,
                        ErrorMessage = ex.Message,
                        MigratedAt = DateTime.UtcNow
                    };

                    results.Add(errorResult);
                }
            }

            return results;
        }
    }
}