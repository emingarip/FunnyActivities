using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Application.Queries.CategoryManagement;
using FunnyActivities.Application.Handlers.CategoryManagement;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Application.DTOs.CategoryManagement;

namespace FunnyActivities.Application.UnitTests.Handlers.CategoryManagement
{
    public class GetCategoryQueryHandlerTests
    {
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly Mock<ILogger<GetCategoryQueryHandler>> _loggerMock;
        private readonly GetCategoryQueryHandler _handler;
        private readonly Fixture _fixture;

        public GetCategoryQueryHandlerTests()
        {
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _loggerMock = new Mock<ILogger<GetCategoryQueryHandler>>();
            _fixture = new Fixture();

            _handler = new GetCategoryQueryHandler(
                _categoryRepositoryMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ValidId_ShouldReturnCategoryDto()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var category = Category.Create("Test Category", "Test Description");
            typeof(Category).GetProperty("Id")?.SetValue(category, categoryId);

            var query = new GetCategoryQuery { Id = categoryId };

            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId)).ReturnsAsync(category);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(categoryId);
            result.Name.Should().Be("Test Category");
            result.Description.Should().Be("Test Description");
            result.CreatedAt.Should().Be(category.CreatedAt);
            result.UpdatedAt.Should().Be(category.UpdatedAt);
            result.ProductCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_CategoryWithProducts_ShouldReturnCorrectProductCount()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var category = Category.Create("Test Category", "Test Description");
            typeof(Category).GetProperty("Id")?.SetValue(category, categoryId);

            // Add some products to the category
            var products = new System.Collections.Generic.List<BaseProduct>
            {
                new BaseProduct(Guid.NewGuid(), "Product 1", "Description", categoryId),
                new BaseProduct(Guid.NewGuid(), "Product 2", "Description", categoryId),
                new BaseProduct(Guid.NewGuid(), "Product 3", "Description", categoryId)
            };
            typeof(Category).GetProperty("BaseProducts")?.SetValue(category, products);

            var query = new GetCategoryQuery { Id = categoryId };

            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId)).ReturnsAsync(category);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.ProductCount.Should().Be(3);
        }

        [Fact]
        public async Task Handle_CategoryNotFound_ShouldReturnNull()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var query = new GetCategoryQuery { Id = categoryId };

            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId)).ReturnsAsync((Category)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_EmptyGuid_ShouldReturnNull()
        {
            // Arrange
            var query = new GetCategoryQuery { Id = Guid.Empty };

            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(Guid.Empty)).ReturnsAsync((Category)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_CategoryWithNullDescription_ShouldReturnDtoWithNullDescription()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var category = Category.Create("Test Category", null);
            typeof(Category).GetProperty("Id")?.SetValue(category, categoryId);

            var query = new GetCategoryQuery { Id = categoryId };

            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId)).ReturnsAsync(category);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Description.Should().BeNull();
        }

        [Fact]
        public async Task Handle_CancellationRequested_ShouldCancelOperation()
        {
            // Arrange
            var query = new GetCategoryQuery { Id = Guid.NewGuid() };

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            // Act & Assert
            await Assert.ThrowsAsync<TaskCanceledException>(() =>
                _handler.Handle(query, cancellationTokenSource.Token));
        }
    }
}