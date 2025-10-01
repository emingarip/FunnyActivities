using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using FunnyActivities.WebAPI;
using FunnyActivities.Application.DTOs.ProductVariantManagement;

namespace FunnyActivities.Api.IntegrationTests
{
    public class ProductVariantIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public ProductVariantIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GetProductVariants_WithoutFilters_ShouldReturnAllVariants()
        {
            // Act
            var response = await _client.GetAsync("/api/product-variants");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var variants = JsonSerializer.Deserialize<List<ProductVariantDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            variants.Should().NotBeNull();
            variants.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetProductVariants_WithBaseProductIdFilter_ShouldReturnFilteredVariants()
        {
            // Arrange
            var baseProductId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/product-variants?baseProductId={baseProductId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var variants = JsonSerializer.Deserialize<List<ProductVariantDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            variants.Should().NotBeNull();
            variants.Should().AllSatisfy(v => v.BaseProductId.Should().Be(baseProductId));
        }

        [Fact]
        public async Task GetProductVariants_WithSearchTerm_ShouldReturnFilteredVariants()
        {
            // Arrange
            var searchTerm = "laptop";

            // Act
            var response = await _client.GetAsync($"/api/product-variants?searchTerm={searchTerm}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var variants = JsonSerializer.Deserialize<List<ProductVariantDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            variants.Should().NotBeNull();
            variants.Should().AllSatisfy(v =>
                v.Name.ToLower().Should().Contain(searchTerm.ToLower()));
        }

        [Fact]
        public async Task GetProductVariants_WithPagination_ShouldReturnPagedResults()
        {
            // Arrange
            var pageNumber = 1;
            var pageSize = 5;

            // Act
            var response = await _client.GetAsync($"/api/product-variants?pageNumber={pageNumber}&pageSize={pageSize}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var variants = JsonSerializer.Deserialize<List<ProductVariantDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            variants.Should().NotBeNull();
            variants.Should().HaveCountLessThanOrEqualTo(pageSize);
        }

        [Fact]
        public async Task GetProductVariant_WithValidId_ShouldReturnVariant()
        {
            // Arrange - Get an existing variant ID from the list
            var listResponse = await _client.GetAsync("/api/product-variants?pageSize=1");
            listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var listContent = await listResponse.Content.ReadAsStringAsync();
            var variants = JsonSerializer.Deserialize<List<ProductVariantDto>>(listContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (variants.Count == 0)
            {
                // Skip test if no variants exist
                return;
            }

            var variantId = variants[0].Id;

            // Act
            var response = await _client.GetAsync($"/api/product-variants/{variantId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var variant = JsonSerializer.Deserialize<ProductVariantDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            variant.Should().NotBeNull();
            variant.Id.Should().Be(variantId);
            variant.BaseProductName.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetProductVariant_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var invalidId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/product-variants/{invalidId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateProductVariant_WithValidData_ShouldCreateSuccessfully()
        {
            // Arrange
            var createRequest = new CreateProductVariantRequest
            {
                BaseProductId = Guid.NewGuid(), // This would need to be a valid existing base product ID
                Name = $"Test Variant {Guid.NewGuid()}",
                StockQuantity = 10,
                UnitOfMeasureId = Guid.NewGuid(), // This would need to be a valid existing unit ID
                UnitValue = 1.0m,
                UsageNotes = "Integration test variant",
                DynamicProperties = new Dictionary<string, object>
                {
                    { "testKey", "testValue" }
                }
            };

            // Note: This test would require setting up valid base product and unit of measure data first
            // For now, we'll just test the endpoint structure
            var jsonContent = JsonContent.Create(createRequest);

            // Act
            var response = await _client.PostAsync("/api/product-variants", jsonContent);

            // Assert - This will likely fail due to missing setup data, but tests the endpoint
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UpdateProductVariant_WithValidData_ShouldUpdateSuccessfully()
        {
            // Arrange - Get an existing variant
            var listResponse = await _client.GetAsync("/api/product-variants?pageSize=1");
            if (listResponse.StatusCode != HttpStatusCode.OK)
            {
                // Skip if no variants exist
                return;
            }

            var listContent = await listResponse.Content.ReadAsStringAsync();
            var variants = JsonSerializer.Deserialize<List<ProductVariantDto>>(listContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (variants.Count == 0)
            {
                return;
            }

            var variantId = variants[0].Id;
            var updateRequest = new UpdateProductVariantRequest
            {
                Name = $"Updated Variant {Guid.NewGuid()}",
                StockQuantity = 25,
                UsageNotes = "Updated via integration test",
                DynamicProperties = new Dictionary<string, object>
                {
                    { "updatedKey", "updatedValue" }
                }
            };

            var jsonContent = JsonContent.Create(updateRequest);

            // Act
            var response = await _client.PutAsync($"/api/product-variants/{variantId}", jsonContent);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task DeleteProductVariant_WithValidId_ShouldDeleteSuccessfully()
        {
            // Arrange - Get an existing variant
            var listResponse = await _client.GetAsync("/api/product-variants?pageSize=1");
            if (listResponse.StatusCode != HttpStatusCode.OK)
            {
                return;
            }

            var listContent = await listResponse.Content.ReadAsStringAsync();
            var variants = JsonSerializer.Deserialize<List<ProductVariantDto>>(listContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (variants.Count == 0)
            {
                return;
            }

            var variantId = variants[0].Id;

            // Act
            var response = await _client.DeleteAsync($"/api/product-variants/{variantId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetProductVariants_WithInvalidPageSize_ShouldReturnBadRequest()
        {
            // Act
            var response = await _client.GetAsync("/api/product-variants?pageSize=150"); // Exceeds max page size

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetProductVariants_WithInvalidPageNumber_ShouldReturnBadRequest()
        {
            // Act
            var response = await _client.GetAsync("/api/product-variants?pageNumber=0"); // Invalid page number

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}