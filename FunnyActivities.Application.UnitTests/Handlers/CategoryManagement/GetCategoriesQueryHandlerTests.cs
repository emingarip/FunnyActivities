using System;
using System.Collections.Generic;
using System.Linq;
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
using FunnyActivities.Application.DTOs.Shared;

namespace FunnyActivities.Application.UnitTests.Handlers.CategoryManagement
{
    public class GetCategoriesQueryHandlerTests
    {
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly Mock<ILogger<GetCategoriesQueryHandler>> _loggerMock;
        private readonly GetCategoriesQueryHandler _handler;
        private readonly Fixture _fixture;

        public GetCategoriesQueryHandlerTests()
        {
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _loggerMock = new Mock<ILogger<GetCategoriesQueryHandler>>();
            _fixture = new Fixture();

            _handler = new GetCategoriesQueryHandler(
                _categoryRepositoryMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_NoSearchTerm_ShouldReturnAllCategories()
        {
            // Arrange
            var categories = new List<Category>
            {
                CreateCategory("Category A", "Description A"),
                CreateCategory("Category B", "Description B"),
                CreateCategory("Category C", "Description C")
            };

            var query = new GetCategoriesQuery
            {
                PageNumber = 1,
                PageSize = 10,
                SearchTerm = null,
                SortBy = null,
                SortOrder = null
            };

            _categoryRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(categories);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(3);
            result.TotalCount.Should().Be(3);
            result.Page.Should().Be(1);
            result.PageSize.Should().Be(10);
            result.Items.Select(x => x.Name).Should().BeEquivalentTo(new[] { "Category A", "Category B", "Category C" });
        }

        [Fact]
        public async Task Handle_WithSearchTerm_ShouldFilterCategories()
        {
            // Arrange
            var categories = new List<Category>
            {
                CreateCategory("Apple Products", "Fresh apples"),
                CreateCategory("Banana Products", "Fresh bananas"),
                CreateCategory("Orange Products", "Fresh oranges")
            };

            var query = new GetCategoriesQuery
            {
                PageNumber = 1,
                PageSize = 10,
                SearchTerm = "apple",
                SortBy = null,
                SortOrder = null
            };

            _categoryRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(categories);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
            result.Items.First().Name.Should().Be("Apple Products");
        }

        [Fact]
        public async Task Handle_WithSearchTermInDescription_ShouldFilterCategories()
        {
            // Arrange
            var categories = new List<Category>
            {
                CreateCategory("Fruits", "Fresh apples and bananas"),
                CreateCategory("Vegetables", "Fresh carrots and tomatoes"),
                CreateCategory("Dairy", "Milk and cheese")
            };

            var query = new GetCategoriesQuery
            {
                PageNumber = 1,
                PageSize = 10,
                SearchTerm = "fresh",
                SortBy = null,
                SortOrder = null
            };

            _categoryRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(categories);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
            result.Items.Select(x => x.Name).Should().BeEquivalentTo(new[] { "Fruits", "Vegetables" });
        }

        [Fact]
        public async Task Handle_WithPagination_ShouldReturnCorrectPage()
        {
            // Arrange
            var categories = new List<Category>();
            for (int i = 1; i <= 25; i++)
            {
                categories.Add(CreateCategory($"Category {i}", $"Description {i}"));
            }

            var query = new GetCategoriesQuery
            {
                PageNumber = 2,
                PageSize = 10,
                SearchTerm = null,
                SortBy = null,
                SortOrder = null
            };

            _categoryRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(categories);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(10);
            result.TotalCount.Should().Be(25);
            result.Page.Should().Be(2);
            result.PageSize.Should().Be(10);
            result.Items.First().Name.Should().Be("Category 11");
            result.Items.Last().Name.Should().Be("Category 20");
        }

        [Fact]
        public async Task Handle_SortByNameAscending_ShouldSortCorrectly()
        {
            // Arrange
            var categories = new List<Category>
            {
                CreateCategory("Zebra", "Description"),
                CreateCategory("Apple", "Description"),
                CreateCategory("Banana", "Description")
            };

            var query = new GetCategoriesQuery
            {
                PageNumber = 1,
                PageSize = 10,
                SearchTerm = null,
                SortBy = "name",
                SortOrder = "asc"
            };

            _categoryRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(categories);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(3);
            result.Items.Select(x => x.Name).Should().BeEquivalentTo(new[] { "Apple", "Banana", "Zebra" });
        }

        [Fact]
        public async Task Handle_SortByNameDescending_ShouldSortCorrectly()
        {
            // Arrange
            var categories = new List<Category>
            {
                CreateCategory("Apple", "Description"),
                CreateCategory("Banana", "Description"),
                CreateCategory("Zebra", "Description")
            };

            var query = new GetCategoriesQuery
            {
                PageNumber = 1,
                PageSize = 10,
                SearchTerm = null,
                SortBy = "name",
                SortOrder = "desc"
            };

            _categoryRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(categories);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(3);
            result.Items.Select(x => x.Name).Should().BeEquivalentTo(new[] { "Zebra", "Banana", "Apple" });
        }

        [Fact]
        public async Task Handle_SortByCreatedAt_ShouldSortCorrectly()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var categories = new List<Category>
            {
                CreateCategoryWithDate("Category 1", "Description", now.AddDays(-2)),
                CreateCategoryWithDate("Category 2", "Description", now.AddDays(-1)),
                CreateCategoryWithDate("Category 3", "Description", now)
            };

            var query = new GetCategoriesQuery
            {
                PageNumber = 1,
                PageSize = 10,
                SearchTerm = null,
                SortBy = "createdat",
                SortOrder = "asc"
            };

            _categoryRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(categories);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(3);
            result.Items.Select(x => x.Name).Should().BeEquivalentTo(new[] { "Category 1", "Category 2", "Category 3" });
        }

        [Fact]
        public async Task Handle_InvalidSortBy_ShouldDefaultToName()
        {
            // Arrange
            var categories = new List<Category>
            {
                CreateCategory("Zebra", "Description"),
                CreateCategory("Apple", "Description")
            };

            var query = new GetCategoriesQuery
            {
                PageNumber = 1,
                PageSize = 10,
                SearchTerm = null,
                SortBy = "invalid",
                SortOrder = "asc"
            };

            _categoryRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(categories);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.Items.Select(x => x.Name).Should().BeEquivalentTo(new[] { "Apple", "Zebra" });
        }

        [Fact]
        public async Task Handle_EmptyResult_ShouldReturnEmptyPagedResult()
        {
            // Arrange
            var categories = new List<Category>();
            var query = new GetCategoriesQuery
            {
                PageNumber = 1,
                PageSize = 10,
                SearchTerm = "nonexistent",
                SortBy = null,
                SortOrder = null
            };

            _categoryRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(categories);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
            result.Page.Should().Be(1);
            result.PageSize.Should().Be(10);
        }

        [Fact]
        public async Task Handle_CancellationRequested_ShouldCancelOperation()
        {
            // Arrange
            var query = new GetCategoriesQuery
            {
                PageNumber = 1,
                PageSize = 10,
                SearchTerm = null,
                SortBy = null,
                SortOrder = null
            };

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            // Act & Assert
            await Assert.ThrowsAsync<TaskCanceledException>(() =>
                _handler.Handle(query, cancellationTokenSource.Token));
        }

        private Category CreateCategory(string name, string description)
        {
            var category = Category.Create(name, description);
            return category;
        }

        private Category CreateCategoryWithDate(string name, string description, DateTime createdAt)
        {
            var category = Category.Create(name, description);
            typeof(Category).GetProperty("CreatedAt")?.SetValue(category, createdAt);
            return category;
        }
    }
}