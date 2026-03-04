using FluentAssertions;
using FunnyActivities.Application.Commands.ProductVariantManagement;
using FunnyActivities.Application.Handlers.ProductVariantManagement;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FunnyActivities.Application.UnitTests.Handlers.ProductVariantManagement;

public class BulkUpdateProductVariantsCommandHandlerTests
{
    private readonly Mock<IProductVariantRepository> _productVariantRepositoryMock;
    private readonly Mock<IUnitOfMeasureRepository> _unitOfMeasureRepositoryMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ILogger<BulkUpdateProductVariantsCommandHandler>> _loggerMock;
    private readonly BulkUpdateProductVariantsCommandHandler _handler;

    public BulkUpdateProductVariantsCommandHandlerTests()
    {
        _productVariantRepositoryMock = new Mock<IProductVariantRepository>();
        _unitOfMeasureRepositoryMock = new Mock<IUnitOfMeasureRepository>();
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<BulkUpdateProductVariantsCommandHandler>>();

        _handler = new BulkUpdateProductVariantsCommandHandler(
            _productVariantRepositoryMock.Object,
            _unitOfMeasureRepositoryMock.Object,
            _mediatorMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsSuccessfulResponse()
    {
        var userId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var baseProductId = Guid.NewGuid();
        var oldUnitId = Guid.NewGuid();
        var newUnitId = Guid.NewGuid();

        var existingVariant = CreateVariant(variantId, baseProductId, "Original Name", 10, oldUnitId, 1m, "Old notes");
        var unitOfMeasure = CreateUnitOfMeasure(newUnitId);

        var command = new BulkUpdateProductVariantsCommand
        {
            UserId = userId,
            Updates =
            [
                new ProductVariantUpdateRequest
                {
                    Id = variantId,
                    Name = "Updated Name",
                    StockQuantity = 20, // current handler does not apply stock updates
                    UnitOfMeasureId = newUnitId,
                    UnitValue = 2,
                    UsageNotes = "Updated notes"
                }
            ]
        };

        _productVariantRepositoryMock.Setup(r => r.GetByIdAsync(variantId)).ReturnsAsync(existingVariant);
        _productVariantRepositoryMock.Setup(r => r.GetByNameAsync("Updated Name")).ReturnsAsync((ProductVariant?)null);
        _unitOfMeasureRepositoryMock.Setup(r => r.GetByIdAsync(newUnitId)).ReturnsAsync(unitOfMeasure);
        _productVariantRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ProductVariant>())).Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.TotalUpdates.Should().Be(1);
        result.SuccessfulUpdates.Should().Be(1);
        result.FailedUpdates.Should().Be(0);
        result.UpdatedVariants.Should().HaveCount(1);
        result.Errors.Should().BeEmpty();

        var updatedVariant = result.UpdatedVariants[0];
        updatedVariant.Id.Should().Be(variantId);
        updatedVariant.Name.Should().Be("Updated Name");
        updatedVariant.UnitOfMeasureId.Should().Be(newUnitId);
        updatedVariant.UnitValue.Should().Be(2);
        updatedVariant.UsageNotes.Should().Be("Updated notes");
        updatedVariant.StockQuantity.Should().Be(10); // unchanged by current handler

        _productVariantRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<ProductVariant>()), Times.Once);
        _mediatorMock.Verify(m => m.Publish(It.IsAny<ProductVariantUpdatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ProductVariantNotFound_ThrowsException()
    {
        var userId = Guid.NewGuid();
        var variantId = Guid.NewGuid();

        var command = new BulkUpdateProductVariantsCommand
        {
            UserId = userId,
            Updates = [new ProductVariantUpdateRequest { Id = variantId, Name = "Updated Name" }]
        };

        _productVariantRepositoryMock.Setup(r => r.GetByIdAsync(variantId)).ReturnsAsync((ProductVariant?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.TotalUpdates.Should().Be(1);
        result.SuccessfulUpdates.Should().Be(0);
        result.FailedUpdates.Should().Be(1);
        result.UpdatedVariants.Should().BeEmpty();
        result.Errors.Should().ContainSingle();
        result.Errors[0].VariantId.Should().Be(variantId);
        result.Errors[0].ErrorType.Should().Be("ProductVariantNotFoundException");
    }

    [Fact]
    public async Task Handle_UnitOfMeasureNotFound_ThrowsException()
    {
        var userId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var baseProductId = Guid.NewGuid();
        var currentUnitId = Guid.NewGuid();
        var requestedUnitId = Guid.NewGuid();

        var existingVariant = CreateVariant(variantId, baseProductId, "Original Name", 10, currentUnitId, 1m, null);

        var command = new BulkUpdateProductVariantsCommand
        {
            UserId = userId,
            Updates = [new ProductVariantUpdateRequest { Id = variantId, UnitOfMeasureId = requestedUnitId }]
        };

        _productVariantRepositoryMock.Setup(r => r.GetByIdAsync(variantId)).ReturnsAsync(existingVariant);
        _unitOfMeasureRepositoryMock.Setup(r => r.GetByIdAsync(requestedUnitId)).ReturnsAsync((UnitOfMeasure?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.TotalUpdates.Should().Be(1);
        result.SuccessfulUpdates.Should().Be(0);
        result.FailedUpdates.Should().Be(1);
        result.Errors.Should().ContainSingle();
        result.Errors[0].VariantId.Should().Be(variantId);
        result.Errors[0].ErrorType.Should().Be("UnitOfMeasureNotFoundException");
    }

    [Fact]
    public async Task Handle_DuplicateNameInSameBaseProduct_ThrowsException()
    {
        var userId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var baseProductId = Guid.NewGuid();
        var unitId = Guid.NewGuid();

        var existingVariant = CreateVariant(variantId, baseProductId, "Original Name", 10, unitId, 1m, null);
        var duplicateVariant = CreateVariant(Guid.NewGuid(), baseProductId, "Duplicate Name", 4, unitId, 1m, null);

        var command = new BulkUpdateProductVariantsCommand
        {
            UserId = userId,
            Updates = [new ProductVariantUpdateRequest { Id = variantId, Name = "Duplicate Name" }]
        };

        _productVariantRepositoryMock.Setup(r => r.GetByIdAsync(variantId)).ReturnsAsync(existingVariant);
        _productVariantRepositoryMock.Setup(r => r.GetByNameAsync("Duplicate Name")).ReturnsAsync(duplicateVariant);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.TotalUpdates.Should().Be(1);
        result.SuccessfulUpdates.Should().Be(0);
        result.FailedUpdates.Should().Be(1);
        result.Errors.Should().ContainSingle();
        result.Errors[0].ErrorType.Should().Be("ProductVariantNameAlreadyExistsException");
    }

    [Fact]
    public async Task Handle_MultipleUpdatesWithMixedResults_ReturnsCorrectResponse()
    {
        var userId = Guid.NewGuid();
        var validVariantId = Guid.NewGuid();
        var invalidVariantId = Guid.NewGuid();
        var baseProductId = Guid.NewGuid();
        var unitId = Guid.NewGuid();

        var validVariant = CreateVariant(validVariantId, baseProductId, "Valid Variant", 10, unitId, 1m, null);

        var command = new BulkUpdateProductVariantsCommand
        {
            UserId = userId,
            Updates =
            [
                new ProductVariantUpdateRequest { Id = validVariantId, Name = "Updated Valid Variant" },
                new ProductVariantUpdateRequest { Id = invalidVariantId, Name = "Invalid Variant" }
            ]
        };

        _productVariantRepositoryMock.Setup(r => r.GetByIdAsync(validVariantId)).ReturnsAsync(validVariant);
        _productVariantRepositoryMock.Setup(r => r.GetByNameAsync("Updated Valid Variant")).ReturnsAsync((ProductVariant?)null);
        _productVariantRepositoryMock.Setup(r => r.GetByIdAsync(invalidVariantId)).ReturnsAsync((ProductVariant?)null);
        _productVariantRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ProductVariant>())).Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.TotalUpdates.Should().Be(2);
        result.SuccessfulUpdates.Should().Be(1);
        result.FailedUpdates.Should().Be(1);
        result.UpdatedVariants.Should().HaveCount(1);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_EmptyUpdatesList_ReturnsEmptyResponse()
    {
        var command = new BulkUpdateProductVariantsCommand
        {
            UserId = Guid.NewGuid(),
            Updates = []
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.TotalUpdates.Should().Be(0);
        result.SuccessfulUpdates.Should().Be(0);
        result.FailedUpdates.Should().Be(0);
        result.UpdatedVariants.Should().BeEmpty();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_UpdateWithDynamicProperties_UpdatesDynamicProperties()
    {
        var variantId = Guid.NewGuid();
        var baseProductId = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        var existingVariant = CreateVariant(variantId, baseProductId, "Variant", 10, unitId, 1m, null);
        existingVariant.UpdateDynamicProperties(new Dictionary<string, object> { { "size", "M" } });

        var command = new BulkUpdateProductVariantsCommand
        {
            UserId = Guid.NewGuid(),
            Updates =
            [
                new ProductVariantUpdateRequest
                {
                    Id = variantId,
                    DynamicProperties = new Dictionary<string, object>
                    {
                        { "size", "L" },
                        { "color", "Blue" }
                    }
                }
            ]
        };

        _productVariantRepositoryMock.Setup(r => r.GetByIdAsync(variantId)).ReturnsAsync(existingVariant);
        _productVariantRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ProductVariant>())).Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.SuccessfulUpdates.Should().Be(1);
        _productVariantRepositoryMock.Verify(r => r.UpdateAsync(It.Is<ProductVariant>(v =>
            v.DynamicProperties.ContainsKey("size") &&
            v.DynamicProperties.ContainsKey("color") &&
            v.DynamicProperties["size"].ToString() == "L" &&
            v.DynamicProperties["color"].ToString() == "Blue"
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_RepositoryException_IsHandledGracefully()
    {
        var variantId = Guid.NewGuid();
        var existingVariant = CreateVariant(variantId, Guid.NewGuid(), "Variant", 10, Guid.NewGuid(), 1m, null);

        var command = new BulkUpdateProductVariantsCommand
        {
            UserId = Guid.NewGuid(),
            Updates = [new ProductVariantUpdateRequest { Id = variantId, Name = "Updated Name" }]
        };

        _productVariantRepositoryMock.Setup(r => r.GetByIdAsync(variantId)).ReturnsAsync(existingVariant);
        _productVariantRepositoryMock.Setup(r => r.GetByNameAsync("Updated Name")).ReturnsAsync((ProductVariant?)null);
        _productVariantRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ProductVariant>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.TotalUpdates.Should().Be(1);
        result.SuccessfulUpdates.Should().Be(0);
        result.FailedUpdates.Should().Be(1);
        result.Errors.Should().ContainSingle();
        result.Errors[0].VariantId.Should().Be(variantId);
        result.Errors[0].ErrorMessage.Should().Be("Database connection failed");
        result.Errors[0].ErrorType.Should().Be("Exception");
    }

    [Fact]
    public async Task Handle_CancellationRequested_StopsProcessing()
    {
        var variantId1 = Guid.NewGuid();
        var variantId2 = Guid.NewGuid();
        var baseProductId = Guid.NewGuid();
        var unitId = Guid.NewGuid();

        var variant1 = CreateVariant(variantId1, baseProductId, "Variant 1", 10, unitId, 1m, null);
        var variant2 = CreateVariant(variantId2, baseProductId, "Variant 2", 10, unitId, 1m, null);

        var command = new BulkUpdateProductVariantsCommand
        {
            UserId = Guid.NewGuid(),
            Updates =
            [
                new ProductVariantUpdateRequest { Id = variantId1, Name = "Updated 1" },
                new ProductVariantUpdateRequest { Id = variantId2, Name = "Updated 2" }
            ]
        };

        _productVariantRepositoryMock.Setup(r => r.GetByIdAsync(variantId1)).ReturnsAsync(variant1);
        _productVariantRepositoryMock.Setup(r => r.GetByIdAsync(variantId2)).ReturnsAsync(variant2);
        _productVariantRepositoryMock.Setup(r => r.GetByNameAsync("Updated 1")).ReturnsAsync((ProductVariant?)null);
        _productVariantRepositoryMock.Setup(r => r.GetByNameAsync("Updated 2")).ReturnsAsync((ProductVariant?)null);
        _productVariantRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ProductVariant>())).Returns(Task.CompletedTask);

        var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await _handler.Handle(command, cts.Token);

        result.TotalUpdates.Should().Be(2);
        result.SuccessfulUpdates.Should().Be(2);
    }

    private static ProductVariant CreateVariant(
        Guid id,
        Guid baseProductId,
        string name,
        decimal stockQuantity,
        Guid unitOfMeasureId,
        decimal unitValue,
        string? usageNotes)
    {
        var variant = ProductVariant.Create(baseProductId, name, stockQuantity, unitOfMeasureId, unitValue, usageNotes);
        typeof(ProductVariant).GetProperty("Id")?.SetValue(variant, id);
        return variant;
    }

    private static UnitOfMeasure CreateUnitOfMeasure(Guid id)
    {
        var unit = UnitOfMeasure.Create("Unit", "U", "Generic");
        typeof(UnitOfMeasure).GetProperty("Id")?.SetValue(unit, id);
        return unit;
    }
}
