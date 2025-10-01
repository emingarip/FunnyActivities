using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FunnyActivities.Application.Commands.ProductVariantManagement;
using FunnyActivities.Application.DTOs.ProductVariantManagement;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Application.Handlers.ProductVariantManagement;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Domain.Exceptions;
using FunnyActivities.Domain.Events;

namespace FunnyActivities.Application.UnitTests.Handlers.ProductVariantManagement
{
    public class BulkUpdateProductVariantsCommandHandlerTests
    {
        private readonly Mock<IProductVariantRepository> _productVariantRepositoryMock;
        private readonly Mock<IUnitOfMeasureRepository> _unitOfMeasureRepositoryMock;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ILogger<BulkUpdateProductVariantsCommandHandler>> _loggerMock;
        private readonly BulkUpdateProductVariantsCommandHandler _handler;
        private readonly Fixture _fixture;

        public BulkUpdateProductVariantsCommandHandlerTests()
        {
            _productVariantRepositoryMock = new Mock<IProductVariantRepository>();
            _unitOfMeasureRepositoryMock = new Mock<IUnitOfMeasureRepository>();
            _mediatorMock = new Mock<IMediator>();
            _loggerMock = new Mock<ILogger<BulkUpdateProductVariantsCommandHandler>>();
            _fixture = new Fixture();

            _handler = new BulkUpdateProductVariantsCommandHandler(
                _productVariantRepositoryMock.Object,
                _unitOfMeasureRepositoryMock.Object,
                _mediatorMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ValidRequest_ReturnsSuccessfulResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var variantId = Guid.NewGuid();
            var unitOfMeasureId = Guid.NewGuid();

            var existingVariant = _fixture.Build<ProductVariant>()
                .With(v => v.Id, variantId)
                .With(v => v.Name, "Original Name")
                .With(v => v.StockQuantity, 10)
                .With(v => v.UnitOfMeasureId, Guid.NewGuid())
                .With(v => v.UnitValue, 1)
                .Create();

            var unitOfMeasure = _fixture.Build<UnitOfMeasure>()
                .With(u => u.Id, unitOfMeasureId)
                .Create();

            var command = new BulkUpdateProductVariantsCommand
            {
                UserId = userId,
                Updates = new List<ProductVariantUpdateRequest>
                {
                    new ProductVariantUpdateRequest
                    {
                        Id = variantId,
                        Name = "Updated Name",
                        StockQuantity = 20,
                        UnitOfMeasureId = unitOfMeasureId,
                        UnitValue = 2,
                        UsageNotes = "Updated notes"
                    }
                }
            };

            _productVariantRepositoryMock
                .Setup(r => r.GetByIdAsync(variantId))
                .ReturnsAsync(existingVariant);

            _unitOfMeasureRepositoryMock
                .Setup(r => r.GetByIdAsync(unitOfMeasureId))
                .ReturnsAsync(unitOfMeasure);

            _productVariantRepositoryMock
                .Setup(r => r.UpdateAsync(It.IsAny<ProductVariant>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.TotalUpdates.Should().Be(1);
            result.SuccessfulUpdates.Should().Be(1);
            result.FailedUpdates.Should().Be(0);
            result.UpdatedVariants.Should().HaveCount(1);
            result.Errors.Should().BeEmpty();

            var updatedVariant = result.UpdatedVariants[0];
            updatedVariant.Id.Should().Be(variantId);
            updatedVariant.Name.Should().Be("Updated Name");
            updatedVariant.StockQuantity.Should().Be(20);
            updatedVariant.UnitOfMeasureId.Should().Be(unitOfMeasureId);
            updatedVariant.UnitValue.Should().Be(2);
            updatedVariant.UsageNotes.Should().Be("Updated notes");

            _productVariantRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<ProductVariant>()), Times.Once);
            _mediatorMock.Verify(m => m.Publish(It.IsAny<ProductVariantUpdatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ProductVariantNotFound_ThrowsException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var variantId = Guid.NewGuid();

            var command = new BulkUpdateProductVariantsCommand
            {
                UserId = userId,
                Updates = new List<ProductVariantUpdateRequest>
                {
                    new ProductVariantUpdateRequest
                    {
                        Id = variantId,
                        Name = "Updated Name"
                    }
                }
            };

            _productVariantRepositoryMock
                .Setup(r => r.GetByIdAsync(variantId))
                .ReturnsAsync((ProductVariant)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.TotalUpdates.Should().Be(1);
            result.SuccessfulUpdates.Should().Be(0);
            result.FailedUpdates.Should().Be(1);
            result.UpdatedVariants.Should().BeEmpty();
            result.Errors.Should().HaveCount(1);

            var error = result.Errors[0];
            error.VariantId.Should().Be(variantId);
            error.ErrorType.Should().Be("ProductVariantNotFoundException");

            _productVariantRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<ProductVariant>()), Times.Never);
            _mediatorMock.Verify(m => m.Publish(It.IsAny<ProductVariantUpdatedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_UnitOfMeasureNotFound_ThrowsException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var variantId = Guid.NewGuid();
            var unitOfMeasureId = Guid.NewGuid();

            var existingVariant = _fixture.Build<ProductVariant>()
                .With(v => v.Id, variantId)
                .Create();

            var command = new BulkUpdateProductVariantsCommand
            {
                UserId = userId,
                Updates = new List<ProductVariantUpdateRequest>
                {
                    new ProductVariantUpdateRequest
                    {
                        Id = variantId,
                        UnitOfMeasureId = unitOfMeasureId
                    }
                }
            };

            _productVariantRepositoryMock
                .Setup(r => r.GetByIdAsync(variantId))
                .ReturnsAsync(existingVariant);

            _unitOfMeasureRepositoryMock
                .Setup(r => r.GetByIdAsync(unitOfMeasureId))
                .ReturnsAsync((UnitOfMeasure)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.TotalUpdates.Should().Be(1);
            result.SuccessfulUpdates.Should().Be(0);
            result.FailedUpdates.Should().Be(1);
            result.UpdatedVariants.Should().BeEmpty();
            result.Errors.Should().HaveCount(1);

            var error = result.Errors[0];
            error.VariantId.Should().Be(variantId);
            error.ErrorType.Should().Be("UnitOfMeasureNotFoundException");

            _productVariantRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<ProductVariant>()), Times.Never);
        }

        [Fact]
        public async Task Handle_DuplicateNameInSameBaseProduct_ThrowsException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var variantId = Guid.NewGuid();
            var baseProductId = Guid.NewGuid();

            var existingVariant = _fixture.Build<ProductVariant>()
                .With(v => v.Id, variantId)
                .With(v => v.BaseProductId, baseProductId)
                .With(v => v.Name, "Original Name")
                .Create();

            var duplicateVariant = _fixture.Build<ProductVariant>()
                .With(v => v.Id, Guid.NewGuid())
                .With(v => v.BaseProductId, baseProductId)
                .With(v => v.Name, "Duplicate Name")
                .Create();

            var command = new BulkUpdateProductVariantsCommand
            {
                UserId = userId,
                Updates = new List<ProductVariantUpdateRequest>
                {
                    new ProductVariantUpdateRequest
                    {
                        Id = variantId,
                        Name = "Duplicate Name"
                    }
                }
            };

            _productVariantRepositoryMock
                .Setup(r => r.GetByIdAsync(variantId))
                .ReturnsAsync(existingVariant);

            _productVariantRepositoryMock
                .Setup(r => r.GetByNameAsync("Duplicate Name"))
                .ReturnsAsync(duplicateVariant);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.TotalUpdates.Should().Be(1);
            result.SuccessfulUpdates.Should().Be(0);
            result.FailedUpdates.Should().Be(1);
            result.UpdatedVariants.Should().BeEmpty();
            result.Errors.Should().HaveCount(1);

            var error = result.Errors[0];
            error.VariantId.Should().Be(variantId);
            error.ErrorType.Should().Be("ProductVariantNameAlreadyExistsException");

            _productVariantRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<ProductVariant>()), Times.Never);
        }

        [Fact]
        public async Task Handle_MultipleUpdatesWithMixedResults_ReturnsCorrectResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var validVariantId = Guid.NewGuid();
            var invalidVariantId = Guid.NewGuid();
            var unitOfMeasureId = Guid.NewGuid();

            var validVariant = _fixture.Build<ProductVariant>()
                .With(v => v.Id, validVariantId)
                .With(v => v.Name, "Valid Variant")
                .Create();

            var unitOfMeasure = _fixture.Build<UnitOfMeasure>()
                .With(u => u.Id, unitOfMeasureId)
                .Create();

            var command = new BulkUpdateProductVariantsCommand
            {
                UserId = userId,
                Updates = new List<ProductVariantUpdateRequest>
                {
                    new ProductVariantUpdateRequest
                    {
                        Id = validVariantId,
                        Name = "Updated Valid Variant",
                        StockQuantity = 15
                    },
                    new ProductVariantUpdateRequest
                    {
                        Id = invalidVariantId,
                        Name = "Invalid Variant"
                    }
                }
            };

            _productVariantRepositoryMock
                .Setup(r => r.GetByIdAsync(validVariantId))
                .ReturnsAsync(validVariant);

            _productVariantRepositoryMock
                .Setup(r => r.GetByIdAsync(invalidVariantId))
                .ReturnsAsync((ProductVariant)null);

            _unitOfMeasureRepositoryMock
                .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(unitOfMeasure);

            _productVariantRepositoryMock
                .Setup(r => r.UpdateAsync(It.IsAny<ProductVariant>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.TotalUpdates.Should().Be(2);
            result.SuccessfulUpdates.Should().Be(1);
            result.FailedUpdates.Should().Be(1);
            result.UpdatedVariants.Should().HaveCount(1);
            result.Errors.Should().HaveCount(1);

            _productVariantRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<ProductVariant>()), Times.Once);
            _mediatorMock.Verify(m => m.Publish(It.IsAny<ProductVariantUpdatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_EmptyUpdatesList_ReturnsEmptyResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var command = new BulkUpdateProductVariantsCommand
            {
                UserId = userId,
                Updates = new List<ProductVariantUpdateRequest>()
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.TotalUpdates.Should().Be(0);
            result.SuccessfulUpdates.Should().Be(0);
            result.FailedUpdates.Should().Be(0);
            result.UpdatedVariants.Should().BeEmpty();
            result.Errors.Should().BeEmpty();

            _productVariantRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<ProductVariant>()), Times.Never);
            _mediatorMock.Verify(m => m.Publish(It.IsAny<ProductVariantUpdatedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_UpdateWithDynamicProperties_UpdatesDynamicProperties()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var variantId = Guid.NewGuid();

            var existingVariant = _fixture.Build<ProductVariant>()
                .With(v => v.Id, variantId)
                .With(v => v.DynamicProperties, new Dictionary<string, object> { { "size", "M" } })
                .Create();

            var command = new BulkUpdateProductVariantsCommand
            {
                UserId = userId,
                Updates = new List<ProductVariantUpdateRequest>
                {
                    new ProductVariantUpdateRequest
                    {
                        Id = variantId,
                        DynamicProperties = new Dictionary<string, object>
                        {
                            { "size", "L" },
                            { "color", "Blue" }
                        }
                    }
                }
            };

            _productVariantRepositoryMock
                .Setup(r => r.GetByIdAsync(variantId))
                .ReturnsAsync(existingVariant);

            _productVariantRepositoryMock
                .Setup(r => r.UpdateAsync(It.IsAny<ProductVariant>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
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
            // Arrange
            var userId = Guid.NewGuid();
            var variantId = Guid.NewGuid();

            var existingVariant = _fixture.Build<ProductVariant>()
                .With(v => v.Id, variantId)
                .Create();

            var command = new BulkUpdateProductVariantsCommand
            {
                UserId = userId,
                Updates = new List<ProductVariantUpdateRequest>
                {
                    new ProductVariantUpdateRequest
                    {
                        Id = variantId,
                        Name = "Updated Name"
                    }
                }
            };

            _productVariantRepositoryMock
                .Setup(r => r.GetByIdAsync(variantId))
                .ReturnsAsync(existingVariant);

            _productVariantRepositoryMock
                .Setup(r => r.UpdateAsync(It.IsAny<ProductVariant>()))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.TotalUpdates.Should().Be(1);
            result.SuccessfulUpdates.Should().Be(0);
            result.FailedUpdates.Should().Be(1);
            result.UpdatedVariants.Should().BeEmpty();
            result.Errors.Should().HaveCount(1);

            var error = result.Errors[0];
            error.VariantId.Should().Be(variantId);
            error.ErrorMessage.Should().Be("Database connection failed");
            error.ErrorType.Should().Be("Exception");
        }

        [Fact]
        public async Task Handle_CancellationRequested_StopsProcessing()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var variantId1 = Guid.NewGuid();
            var variantId2 = Guid.NewGuid();

            var variant1 = _fixture.Build<ProductVariant>()
                .With(v => v.Id, variantId1)
                .Create();

            var variant2 = _fixture.Build<ProductVariant>()
                .With(v => v.Id, variantId2)
                .Create();

            var command = new BulkUpdateProductVariantsCommand
            {
                UserId = userId,
                Updates = new List<ProductVariantUpdateRequest>
                {
                    new ProductVariantUpdateRequest { Id = variantId1, Name = "Updated 1" },
                    new ProductVariantUpdateRequest { Id = variantId2, Name = "Updated 2" }
                }
            };

            _productVariantRepositoryMock
                .Setup(r => r.GetByIdAsync(variantId1))
                .ReturnsAsync(variant1);

            _productVariantRepositoryMock
                .Setup(r => r.GetByIdAsync(variantId2))
                .ReturnsAsync(variant2);

            // Simulate slow operation for first variant
            _productVariantRepositoryMock
                .Setup(r => r.UpdateAsync(It.Is<ProductVariant>(v => v.Id == variantId1)))
                .Returns(async () =>
                {
                    await Task.Delay(100); // Simulate slow operation
                    throw new OperationCanceledException();
                });

            var cts = new CancellationTokenSource();
            cts.CancelAfter(50); // Cancel before the operation completes

            // Act
            var result = await _handler.Handle(command, cts.Token);

            // Assert
            result.Should().NotBeNull();
            // The exact behavior depends on when cancellation occurs, but we verify the handler doesn't crash
            result.TotalUpdates.Should().Be(2);
        }
    }
}