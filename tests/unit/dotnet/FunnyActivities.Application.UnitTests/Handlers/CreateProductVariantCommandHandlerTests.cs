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
using FunnyActivities.Domain.Entities;
using FunnyActivities.Application.Commands.ProductVariantManagement;
using FunnyActivities.Application.Handlers.ProductVariantManagement;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Application.DTOs.ProductVariantManagement;
using FunnyActivities.Domain.Exceptions;

namespace FunnyActivities.Application.UnitTests.Handlers
{
    public class CreateProductVariantCommandHandlerTests
    {
        private readonly Mock<IProductVariantRepository> _productVariantRepositoryMock;
        private readonly Mock<IBaseProductRepository> _baseProductRepositoryMock;
        private readonly Mock<IUnitOfMeasureRepository> _unitOfMeasureRepositoryMock;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ILogger<CreateProductVariantCommandHandler>> _loggerMock;
        private readonly CreateProductVariantCommandHandler _handler;
        private readonly Fixture _fixture;

        public CreateProductVariantCommandHandlerTests()
        {
            _productVariantRepositoryMock = new Mock<IProductVariantRepository>();
            _baseProductRepositoryMock = new Mock<IBaseProductRepository>();
            _unitOfMeasureRepositoryMock = new Mock<IUnitOfMeasureRepository>();
            _mediatorMock = new Mock<IMediator>();
            _loggerMock = new Mock<ILogger<CreateProductVariantCommandHandler>>();
            _fixture = new Fixture();

            _handler = new CreateProductVariantCommandHandler(
                _productVariantRepositoryMock.Object,
                _baseProductRepositoryMock.Object,
                _unitOfMeasureRepositoryMock.Object,
                _mediatorMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_ShouldCreateProductVariant()
        {
            // Arrange
            var baseProductId = Guid.NewGuid();
            var unitOfMeasureId = Guid.NewGuid();
            var command = new CreateProductVariantCommand
            {
                BaseProductId = baseProductId,
                Name = "Test Variant",
                StockQuantity = 10,
                UnitOfMeasureId = unitOfMeasureId,
                UnitValue = 1.0m,
                UsageNotes = "Test notes",
                DynamicProperties = new Dictionary<string, object> { { "key", "value" } },
                UserId = Guid.NewGuid()
            };

            var baseProduct = BaseProduct.Create("Test Base Product", "Description", null);
            var unitOfMeasure = UnitOfMeasure.Create("Test Unit", "TU", "Test");

            _baseProductRepositoryMock.Setup(x => x.GetByIdAsync(baseProductId)).ReturnsAsync(baseProduct);
            _unitOfMeasureRepositoryMock.Setup(x => x.GetByIdAsync(unitOfMeasureId)).ReturnsAsync(unitOfMeasure);
            _productVariantRepositoryMock.Setup(x => x.GetByNameAsync(command.Name)).ReturnsAsync((ProductVariant)null);
            _productVariantRepositoryMock.Setup(x => x.AddAsync(It.IsAny<ProductVariant>())).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().NotBeEmpty();
            result.BaseProductId.Should().Be(baseProductId);
            result.Name.Should().Be(command.Name);
            result.StockQuantity.Should().Be(command.StockQuantity);
            result.UnitOfMeasureId.Should().Be(unitOfMeasureId);
            result.UnitValue.Should().Be(command.UnitValue);
            result.UsageNotes.Should().Be(command.UsageNotes);
            result.DynamicProperties.Should().ContainKey("key");

            _productVariantRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ProductVariant>()), Times.Once);
            _mediatorMock.Verify(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_BaseProductNotFound_ShouldThrowException()
        {
            // Arrange
            var command = _fixture.Create<CreateProductVariantCommand>();
            _baseProductRepositoryMock.Setup(x => x.GetByIdAsync(command.BaseProductId)).ReturnsAsync((BaseProduct)null);

            // Act & Assert
            await Assert.ThrowsAsync<BaseProductNotFoundException>(() =>
                _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_UnitOfMeasureNotFound_ShouldThrowException()
        {
            // Arrange
            var command = _fixture.Create<CreateProductVariantCommand>();
            var baseProduct = BaseProduct.Create("Test", null, null);

            _baseProductRepositoryMock.Setup(x => x.GetByIdAsync(command.BaseProductId)).ReturnsAsync(baseProduct);
            _unitOfMeasureRepositoryMock.Setup(x => x.GetByIdAsync(command.UnitOfMeasureId)).ReturnsAsync((UnitOfMeasure)null);

            // Act & Assert
            await Assert.ThrowsAsync<UnitOfMeasureNotFoundException>(() =>
                _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_DuplicateName_ShouldThrowException()
        {
            // Arrange
            var command = _fixture.Create<CreateProductVariantCommand>();
            var baseProduct = BaseProduct.Create("Test", null, null);
            var unitOfMeasure = UnitOfMeasure.Create("Test", "T", "Test");
            var existingVariant = ProductVariant.Create(baseProduct.Id, command.Name, 5, unitOfMeasure.Id, 1, null);

            _baseProductRepositoryMock.Setup(x => x.GetByIdAsync(command.BaseProductId)).ReturnsAsync(baseProduct);
            _unitOfMeasureRepositoryMock.Setup(x => x.GetByIdAsync(command.UnitOfMeasureId)).ReturnsAsync(unitOfMeasure);
            _productVariantRepositoryMock.Setup(x => x.GetByNameAsync(command.Name)).ReturnsAsync(existingVariant);

            // Act & Assert
            await Assert.ThrowsAsync<ProductVariantNameAlreadyExistsException>(() =>
                _handler.Handle(command, CancellationToken.None));
        }
    }
}