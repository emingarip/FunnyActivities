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
using FunnyActivities.Application.Queries.ProductVariantManagement;
using FunnyActivities.Application.Handlers.ProductVariantManagement;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Application.DTOs.ProductVariantManagement;

namespace FunnyActivities.Application.UnitTests.Handlers
{
    public class GetProductVariantsQueryHandlerTests
    {
        private readonly Mock<IProductVariantRepository> _productVariantRepositoryMock;
        private readonly Mock<ILogger<GetProductVariantsQueryHandler>> _loggerMock;
        private readonly GetProductVariantsQueryHandler _handler;
        private readonly Fixture _fixture;

        public GetProductVariantsQueryHandlerTests()
        {
            _productVariantRepositoryMock = new Mock<IProductVariantRepository>();
            _loggerMock = new Mock<ILogger<GetProductVariantsQueryHandler>>();
            _fixture = new Fixture();

            _handler = new GetProductVariantsQueryHandler(
                _productVariantRepositoryMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_NoFilters_ShouldReturnAllProductVariants()
        {
            // Arrange
            var baseProduct = BaseProduct.Create("Test Base Product", "Description", null);
            var unitOfMeasure = UnitOfMeasure.Create("Test Unit", "TU", "Test");
            var variants = new List<ProductVariant>
            {
                ProductVariant.Create(baseProduct.Id, "Variant 1", 10, unitOfMeasure.Id, 1, "Notes 1"),
                ProductVariant.Create(baseProduct.Id, "Variant 2", 20, unitOfMeasure.Id, 2, "Notes 2")
            };

            var query = new GetProductVariantsQuery();

            _productVariantRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(variants);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Items.Should().HaveCount(2);
            result.Items.First().Name.Should().Be("Variant 1");
            result.Items.Last().Name.Should().Be("Variant 2");
        }

        [Fact]
        public async Task Handle_BaseProductIdFilter_ShouldReturnFilteredVariants()
        {
            // Arrange
            var baseProduct1 = BaseProduct.Create("Base Product 1", "Description 1", null);
            var baseProduct2 = BaseProduct.Create("Base Product 2", "Description 2", null);
            var unitOfMeasure = UnitOfMeasure.Create("Test Unit", "TU", "Test");

            var variants = new List<ProductVariant>
            {
                ProductVariant.Create(baseProduct1.Id, "Variant 1", 10, unitOfMeasure.Id, 1, null),
                ProductVariant.Create(baseProduct2.Id, "Variant 2", 20, unitOfMeasure.Id, 2, null)
            };

            var query = new GetProductVariantsQuery { BaseProductId = baseProduct1.Id };

            _productVariantRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(variants);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Items.Should().HaveCount(1);
            result.Items.First().Name.Should().Be("Variant 1");
            result.Items.First().BaseProductId.Should().Be(baseProduct1.Id);
        }

        [Fact]
        public async Task Handle_SearchTermFilter_ShouldReturnFilteredVariants()
        {
            // Arrange
            var baseProduct = BaseProduct.Create("Test Base Product", "Description", null);
            var unitOfMeasure = UnitOfMeasure.Create("Test Unit", "TU", "Test");

            var variants = new List<ProductVariant>
            {
                ProductVariant.Create(baseProduct.Id, "Red Laptop", 10, unitOfMeasure.Id, 1, null),
                ProductVariant.Create(baseProduct.Id, "Blue Phone", 20, unitOfMeasure.Id, 2, null),
                ProductVariant.Create(baseProduct.Id, "Green Tablet", 30, unitOfMeasure.Id, 3, null)
            };

            var query = new GetProductVariantsQuery { SearchTerm = "phone" };

            _productVariantRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(variants);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Items.Should().HaveCount(1);
            result.Items.First().Name.Should().Be("Blue Phone");
        }

        [Fact]
        public async Task Handle_UnitOfMeasureIdFilter_ShouldReturnFilteredVariants()
        {
            // Arrange
            var baseProduct = BaseProduct.Create("Test Base Product", "Description", null);
            var unitOfMeasure1 = UnitOfMeasure.Create("Unit 1", "U1", "Test");
            var unitOfMeasure2 = UnitOfMeasure.Create("Unit 2", "U2", "Test");

            var variants = new List<ProductVariant>
            {
                ProductVariant.Create(baseProduct.Id, "Variant 1", 10, unitOfMeasure1.Id, 1, null),
                ProductVariant.Create(baseProduct.Id, "Variant 2", 20, unitOfMeasure2.Id, 2, null)
            };

            var query = new GetProductVariantsQuery { UnitOfMeasureId = unitOfMeasure1.Id };

            _productVariantRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(variants);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Items.Should().HaveCount(1);
            result.Items.First().Name.Should().Be("Variant 1");
            result.Items.First().UnitOfMeasureId.Should().Be(unitOfMeasure1.Id);
        }

        [Fact]
        public async Task Handle_Pagination_ShouldReturnPagedResults()
        {
            // Arrange
            var baseProduct = BaseProduct.Create("Test Base Product", "Description", null);
            var unitOfMeasure = UnitOfMeasure.Create("Test Unit", "TU", "Test");

            var variants = new List<ProductVariant>();
            for (int i = 1; i <= 10; i++)
            {
                variants.Add(ProductVariant.Create(baseProduct.Id, $"Variant {i}", i * 10, unitOfMeasure.Id, i, null));
            }

            var query = new GetProductVariantsQuery { PageNumber = 2, PageSize = 3 };

            _productVariantRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(variants);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Items.Should().HaveCount(3);
            result.Items.First().Name.Should().Be("Variant 4"); // Page 2, PageSize 3: items 4, 5, 6
            result.Items.Last().Name.Should().Be("Variant 6");
        }

        [Fact]
        public async Task Handle_AllFilters_ShouldReturnCorrectlyFilteredAndPagedResults()
        {
            // Arrange
            var baseProduct1 = BaseProduct.Create("Laptop Base", "Laptop products", null);
            var baseProduct2 = BaseProduct.Create("Phone Base", "Phone products", null);
            var unitOfMeasure1 = UnitOfMeasure.Create("Pieces", "pcs", "Count");
            var unitOfMeasure2 = UnitOfMeasure.Create("Kilograms", "kg", "Weight");

            var variants = new List<ProductVariant>
            {
                ProductVariant.Create(baseProduct1.Id, "Gaming Laptop", 5, unitOfMeasure1.Id, 1, null),
                ProductVariant.Create(baseProduct1.Id, "Business Laptop", 10, unitOfMeasure1.Id, 1, null),
                ProductVariant.Create(baseProduct2.Id, "Smartphone", 20, unitOfMeasure1.Id, 1, null),
                ProductVariant.Create(baseProduct2.Id, "Heavy Phone", 15, unitOfMeasure2.Id, 0.5m, null)
            };

            var query = new GetProductVariantsQuery
            {
                BaseProductId = baseProduct1.Id,
                SearchTerm = "laptop",
                UnitOfMeasureId = unitOfMeasure1.Id,
                PageNumber = 1,
                PageSize = 10
            };

            _productVariantRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(variants);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Items.Should().HaveCount(2);
            result.Items.All(v => v.BaseProductId == baseProduct1.Id).Should().BeTrue();
            result.Items.All(v => v.Name.ToLower().Contains("laptop")).Should().BeTrue();
            result.Items.All(v => v.UnitOfMeasureId == unitOfMeasure1.Id).Should().BeTrue();
        }
    }
}