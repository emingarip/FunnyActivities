using System;
using System.Collections.Generic;
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
using FunnyActivities.Application.DTOs.BaseProductManagement;

namespace FunnyActivities.Application.UnitTests.Handlers.CategoryManagement
{
    public class GetCategoryWithProductsQueryHandlerTests
    {
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly Mock<ILogger<GetCategoryWithProductsQueryHandler>> _loggerMock;
        private readonly GetCategoryWithProductsQueryHandler _handler;
        private readonly Fixture _fixture;

        public GetCategoryWithProductsQueryHandlerTests()
        {
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _loggerMock = new Mock<ILogger<GetCategoryWithProductsQueryHandler>>();
            _fixture = new Fixture();

            _handler = new GetCategoryWithProductsQueryHandler(
                _categoryRepositoryMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ValidId_ShouldReturnCategoryWithProductsDto()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var category = Category.Create("Test Category", "Test Description");
            typeof(Category).GetProperty("Id")?.SetValue(category, categoryId);

            var products = new List<BaseProduct>
            {
                CreateBaseProduct("Product 1", "Description 1", categoryId),
                CreateBaseProduct("Product 2", "Description 2", categoryId)
            };
            typeof(Category).GetProperty("BaseProducts")?.SetValue(category, products);

            var query = new GetCategoryWithProductsQuery { Id = categoryId };

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
            result.TotalProducts.Should().Be(2);
            result.Products.Should().HaveCount(2);
            result.Products.Select(p => p.Name).Should().BeEquivalentTo(new[] { "Product 1", "Product 2" });
        }

        [Fact]
        public async Task Handle_CategoryWithNoProducts_ShouldReturnEmptyProductsList()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var category = Category.Create("Test Category", "Test Description");
            typeof(Category).GetProperty("Id")?.SetValue(category, categoryId);

            var query = new GetCategoryWithProductsQuery { Id = categoryId };

            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId)).ReturnsAsync(category);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.TotalProducts.Should().Be(0);
            result.Products.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CategoryNotFound_ShouldReturnNull()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var query = new GetCategoryWithProductsQuery { Id = categoryId };

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
            var query = new GetCategoryWithProductsQuery { Id = Guid.Empty };

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

            var query = new GetCategoryWithProductsQuery { Id = categoryId };

            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId)).ReturnsAsync(category);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Description.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ProductsWithNullDescriptions_ShouldHandleCorrectly()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var category = Category.Create("Test Category", "Test Description");
            typeof(Category).GetProperty("Id")?.SetValue(category, categoryId);

            var products = new List<BaseProduct>
            {
                CreateBaseProduct("Product 1", null, categoryId),
                CreateBaseProduct("Product 2", "Description 2", categoryId)
            };
            typeof(Category).GetProperty("BaseProducts")?.SetValue(category, products);

            var query = new GetCategoryWithProductsQuery { Id = categoryId };

            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId)).ReturnsAsync(category);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.TotalProducts.Should().Be(2);
            result.Products.Should().HaveCount(2);
            result.Products.First(p => p.Name == "Product 1").Description.Should().BeNull();
            result.Products.First(p => p.Name == "Product 2").Description.Should().Be("Description 2");
        }

        [Fact]
        public async Task Handle_CancellationRequested_ShouldCancelOperation()
        {
            // Arrange
            var query = new GetCategoryWithProductsQuery { Id = Guid.NewGuid() };

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            // Act & Assert
            await Assert.ThrowsAsync<TaskCanceledException>(() =>
                _handler.Handle(query, cancellationTokenSource.Token));
        }

        private BaseProduct CreateBaseProduct(string name, string? description, Guid categoryId)
        {
            var product = new BaseProduct(Guid.NewGuid(), name, description, categoryId);
            return product;
        }
    }
}