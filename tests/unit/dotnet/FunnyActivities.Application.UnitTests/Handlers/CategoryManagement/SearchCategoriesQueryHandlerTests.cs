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
    public class SearchCategoriesQueryHandlerTests
    {
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly Mock<ILogger<SearchCategoriesQueryHandler>> _loggerMock;
        private readonly SearchCategoriesQueryHandler _handler;
        private readonly Fixture _fixture;

        public SearchCategoriesQueryHandlerTests()
        {
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _loggerMock = new Mock<ILogger<SearchCategoriesQueryHandler>>();
            _fixture = new Fixture();

            _handler = new SearchCategoriesQueryHandler(
                _categoryRepositoryMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ValidSearchTerm_ShouldReturnFilteredCategories()
        {
            // Arrange
            var categories = new List<Category>
            {
                CreateCategory("Apple Products", "Fresh apples"),
                CreateCategory("Banana Products", "Fresh bananas"),
                CreateCategory("Orange Products", "Fresh oranges"),
                CreateCategory("Dairy Products", "Milk and cheese")
            };

            var query = new SearchCategoriesQuery
            {
                SearchTerm = "apple",
                PageNumber = 1,
                PageSize = 10
            };

            _categoryRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(categories);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
            result.Page.Should().Be(1);
            result.PageSize.Should().Be(10);
            result.Items.First().Name.Should().Be("Apple Products");
        }

        [Fact]
        public async Task Handle_SearchTermInDescription_ShouldReturnFilteredCategories()
        {
            // Arrange
            var categories = new List<Category>
            {
                CreateCategory("Fruits", "Fresh apples and bananas"),
                CreateCategory("Vegetables", "Fresh carrots and tomatoes"),
                CreateCategory("Dairy", "Milk and cheese")
            };

            var query = new SearchCategoriesQuery
            {
                SearchTerm = "fresh",
                PageNumber = 1,
                PageSize = 10
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
        public async Task Handle_CaseInsensitiveSearch_ShouldReturnFilteredCategories()
        {
            // Arrange
            var categories = new List<Category>
            {
                CreateCategory("Apple Products", "Fresh apples"),
                CreateCategory("BANANA Products", "Fresh bananas"),
                CreateCategory("Orange Products", "Fresh oranges")
            };

            var query = new SearchCategoriesQuery
            {
                SearchTerm = "APPLE",
                PageNumber = 1,
                PageSize = 10
            };

            _categoryRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(categories);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.Items.First().Name.Should().Be("Apple Products");
        }

        [Fact]
        public async Task Handle_PartialSearchTerm_ShouldReturnFilteredCategories()
        {
            // Arrange
            var categories = new List<Category>
            {
                CreateCategory("Apple Products", "Fresh apples"),
                CreateCategory("Pineapple Products", "Fresh pineapples"),
                CreateCategory("Orange Products", "Fresh oranges")
            };

            var query = new SearchCategoriesQuery
            {
                SearchTerm = "apple",
                PageNumber = 1,
                PageSize = 10
            };

            _categoryRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(categories);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
            result.Items.Select(x => x.Name).Should().BeEquivalentTo(new[] { "Apple Products", "Pineapple Products" });
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

            var query = new SearchCategoriesQuery
            {
                SearchTerm = "Category",
                PageNumber = 2,
                PageSize = 10
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
        public async Task Handle_NoMatchingResults_ShouldReturnEmptyPagedResult()
        {
            // Arrange
            var categories = new List<Category>
            {
                CreateCategory("Apple Products", "Fresh apples"),
                CreateCategory("Banana Products", "Fresh bananas")
            };

            var query = new SearchCategoriesQuery
            {
                SearchTerm = "orange",
                PageNumber = 1,
                PageSize = 10
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
        public async Task Handle_EmptySearchTerm_ShouldThrowValidationException()
        {
            // Arrange
            var query = new SearchCategoriesQuery
            {
                SearchTerm = "",
                PageNumber = 1,
                PageSize = 10
            };

            // Act & Assert
            await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
                _handler.Handle(query, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_NullSearchTerm_ShouldThrowValidationException()
        {
            // Arrange
            var query = new SearchCategoriesQuery
            {
                SearchTerm = null,
                PageNumber = 1,
                PageSize = 10
            };

            // Act & Assert
            await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
                _handler.Handle(query, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_CancellationRequested_ShouldCancelOperation()
        {
            // Arrange
            var query = new SearchCategoriesQuery
            {
                SearchTerm = "test",
                PageNumber = 1,
                PageSize = 10
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
    }
}