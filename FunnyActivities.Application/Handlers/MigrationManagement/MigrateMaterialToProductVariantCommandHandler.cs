using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;
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
    /// Handler for migrating a single material to the new BaseProduct/ProductVariant model.
    /// </summary>
    public class MigrateMaterialToProductVariantCommandHandler : IRequestHandler<MigrateMaterialToProductVariantCommand, MigrationResultDto>
    {
        private readonly IMaterialRepository _materialRepository;
        private readonly IBaseProductRepository _baseProductRepository;
        private readonly IProductVariantRepository _productVariantRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfMeasureRepository _unitOfMeasureRepository;
        private readonly ILogger<MigrateMaterialToProductVariantCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MigrateMaterialToProductVariantCommandHandler"/> class.
        /// </summary>
        /// <param name="materialRepository">The material repository.</param>
        /// <param name="baseProductRepository">The base product repository.</param>
        /// <param name="productVariantRepository">The product variant repository.</param>
        /// <param name="categoryRepository">The category repository.</param>
        /// <param name="unitOfMeasureRepository">The unit of measure repository.</param>
        /// <param name="logger">The logger.</param>
        public MigrateMaterialToProductVariantCommandHandler(
            IMaterialRepository materialRepository,
            IBaseProductRepository baseProductRepository,
            IProductVariantRepository productVariantRepository,
            ICategoryRepository categoryRepository,
            IUnitOfMeasureRepository unitOfMeasureRepository,
            ILogger<MigrateMaterialToProductVariantCommandHandler> logger)
        {
            _materialRepository = materialRepository;
            _baseProductRepository = baseProductRepository;
            _productVariantRepository = productVariantRepository;
            _categoryRepository = categoryRepository;
            _unitOfMeasureRepository = unitOfMeasureRepository;
            _logger = logger;
        }

        /// <summary>
        /// Handles the migrate material to product variant command.
        /// </summary>
        /// <param name="request">The migrate material command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The migration result.</returns>
        public async Task<MigrationResultDto> Handle(MigrateMaterialToProductVariantCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting migration of material {MaterialId} by user {UserId}", request.MaterialId, request.UserId);

            var result = new MigrationResultDto
            {
                MaterialId = request.MaterialId,
                MigratedAt = DateTime.UtcNow
            };

            try
            {
                // Check if material exists and get its data
                var materialData = await GetMaterialDataAsync(request.MaterialId, cancellationToken);
                if (materialData == null)
                {
                    _logger.LogWarning("Material {MaterialId} not found", request.MaterialId);
                    result.Success = false;
                    result.ErrorMessage = $"Material with ID {request.MaterialId} not found";
                    return result;
                }

                // Validate material data if not skipping validation
                if (!request.SkipValidation)
                {
                    var validationResult = ValidateMaterialData(materialData);
                    if (!validationResult.IsValid)
                    {
                        if (!request.ForceMigration)
                        {
                            _logger.LogWarning("Material {MaterialId} validation failed: {Errors}", request.MaterialId, string.Join(", ", validationResult.Errors));
                            result.Success = false;
                            result.ErrorMessage = $"Validation failed: {string.Join(", ", validationResult.Errors)}";
                            return result;
                        }
                        _logger.LogWarning("Material {MaterialId} validation failed but proceeding with force migration: {Errors}", request.MaterialId, string.Join(", ", validationResult.Errors));
                    }
                }

                // Convert unit type to unit of measure
                var unitOfMeasure = await GetOrCreateUnitOfMeasureAsync(materialData.UnitType, cancellationToken);
                if (unitOfMeasure == null)
                {
                    _logger.LogError("Failed to get or create unit of measure for type {UnitType}", materialData.UnitType);
                    result.Success = false;
                    result.ErrorMessage = $"Failed to process unit type: {materialData.UnitType}";
                    return result;
                }

                // Get or create category
                Category? category = null;
                if (materialData.CategoryId.HasValue)
                {
                    category = await _categoryRepository.GetByIdAsync(materialData.CategoryId.Value);
                    if (category == null)
                    {
                        _logger.LogWarning("Category {CategoryId} not found for material {MaterialId}", materialData.CategoryId, request.MaterialId);
                    }
                }

                // Check if a base product with the same name already exists
                var existingBaseProduct = await _baseProductRepository.GetByNameAsync(materialData.Name);
                BaseProduct baseProduct;

                if (existingBaseProduct != null)
                {
                    _logger.LogInformation("Base product with name '{Name}' already exists, using existing one", materialData.Name);
                    baseProduct = existingBaseProduct;

                    // Update description and category if they differ
                    if (baseProduct.Description != materialData.Description || baseProduct.CategoryId != category?.Id)
                    {
                        baseProduct.UpdateDetails(materialData.Name, materialData.Description, category?.Id);
                        await _baseProductRepository.UpdateAsync(baseProduct);
                    }
                }
                else
                {
                    // Create new base product
                    baseProduct = BaseProduct.Create(materialData.Name, materialData.Description, category?.Id);
                    await _baseProductRepository.AddAsync(baseProduct);
                }

                // Deserialize photos and dynamic properties
                var photos = DeserializeJsonList(materialData.Photos);
                var dynamicProperties = DeserializeJsonDictionary(materialData.DynamicProperties);

                // Check if a product variant with the same name already exists for this base product
                var existingVariant = await _productVariantRepository.GetByNameAndBaseProductAsync(materialData.Name, baseProduct.Id);
                ProductVariant productVariant;

                if (existingVariant != null)
                {
                    if (!request.ForceMigration)
                    {
                        _logger.LogWarning("Product variant with name '{Name}' already exists for base product '{BaseProductId}'", materialData.Name, baseProduct.Id);
                        result.Success = false;
                        result.ErrorMessage = $"Product variant with name '{materialData.Name}' already exists for this base product";
                        return result;
                    }

                    _logger.LogInformation("Product variant with name '{Name}' already exists, updating existing one", materialData.Name);
                    productVariant = existingVariant;

                    // Update the existing variant
                    productVariant.UpdateDetails(materialData.Name, unitOfMeasure.Id, materialData.UnitValue, materialData.UsageNotes);
                    productVariant.UpdateStock(materialData.StockQuantity);
                    productVariant.UpdatePhotos(photos);
                    productVariant.UpdateDynamicProperties(dynamicProperties);

                    await _productVariantRepository.UpdateAsync(productVariant);
                }
                else
                {
                    // Create new product variant
                    productVariant = ProductVariant.Create(
                        baseProduct.Id,
                        materialData.Name,
                        materialData.StockQuantity,
                        unitOfMeasure.Id,
                        materialData.UnitValue,
                        materialData.UsageNotes);

                    // Set photos and dynamic properties
                    productVariant.UpdatePhotos(photos);
                    productVariant.UpdateDynamicProperties(dynamicProperties);

                    await _productVariantRepository.AddAsync(productVariant);
                }

                // Mark migration as successful
                result.Success = true;
                result.BaseProductId = baseProduct.Id;
                result.ProductVariantId = productVariant.Id;

                _logger.LogInformation("Successfully migrated material {MaterialId} to BaseProduct {BaseProductId} and ProductVariant {ProductVariantId}",
                    request.MaterialId, baseProduct.Id, productVariant.Id);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error migrating material {MaterialId}", request.MaterialId);
                result.Success = false;
                result.ErrorMessage = ex.Message;
                return result;
            }
        }

        private async Task<MaterialData?> GetMaterialDataAsync(Guid materialId, CancellationToken cancellationToken)
        {
            return await _materialRepository.GetByIdAsync(materialId, cancellationToken);
        }

        private ValidationResult ValidateMaterialData(MaterialData data)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(data.Name))
                errors.Add("Material name is required");

            if (string.IsNullOrWhiteSpace(data.UnitType))
                errors.Add("Unit type is required");

            if (data.UnitValue <= 0)
                errors.Add("Unit value must be greater than 0");

            if (data.StockQuantity < 0)
                errors.Add("Stock quantity cannot be negative");

            return new ValidationResult
            {
                IsValid = errors.Count == 0,
                Errors = errors
            };
        }

        private async Task<UnitOfMeasure?> GetOrCreateUnitOfMeasureAsync(string unitType, CancellationToken cancellationToken)
        {
            // Try to find existing unit of measure by name
            var existingUnit = await _unitOfMeasureRepository.GetByNameAsync(unitType);
            if (existingUnit != null)
                return existingUnit;

            // Try to map common unit types
            var (name, symbol, type) = MapUnitType(unitType);

            // Check if a unit with this name already exists
            existingUnit = await _unitOfMeasureRepository.GetByNameAsync(name);
            if (existingUnit != null)
                return existingUnit;

            // Create new unit of measure
            var newUnit = UnitOfMeasure.Create(name, symbol, type);
            await _unitOfMeasureRepository.AddAsync(newUnit);
            return newUnit;
        }

        private (string name, string symbol, string type) MapUnitType(string unitType)
        {
            // Common mappings for unit types
            return unitType.ToLower() switch
            {
                "kg" or "kilogram" or "kilograms" => ("Kilogram", "kg", "Weight"),
                "g" or "gram" or "grams" => ("Gram", "g", "Weight"),
                "lb" or "pound" or "pounds" => ("Pound", "lb", "Weight"),
                "l" or "liter" or "liters" => ("Liter", "L", "Volume"),
                "ml" or "milliliter" or "milliliters" => ("Milliliter", "mL", "Volume"),
                "m" or "meter" or "meters" => ("Meter", "m", "Length"),
                "cm" or "centimeter" or "centimeters" => ("Centimeter", "cm", "Length"),
                "mm" or "millimeter" or "millimeters" => ("Millimeter", "mm", "Length"),
                "pcs" or "pieces" or "piece" => ("Piece", "pcs", "Count"),
                "box" or "boxes" => ("Box", "box", "Count"),
                _ => (unitType, unitType, "Other") // Default fallback
            };
        }

        private List<string> DeserializeJsonList(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new List<string>();

            try
            {
                return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
            }
            catch
            {
                _logger.LogWarning("Failed to deserialize photos JSON: {Json}", json);
                return new List<string>();
            }
        }

        private Dictionary<string, object> DeserializeJsonDictionary(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new Dictionary<string, object>();

            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? new Dictionary<string, object>();
            }
            catch
            {
                _logger.LogWarning("Failed to deserialize dynamic properties JSON: {Json}", json);
                return new Dictionary<string, object>();
            }
        }


        private class ValidationResult
        {
            public bool IsValid { get; set; }
            public List<string> Errors { get; set; } = new List<string>();
        }
    }
}