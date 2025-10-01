using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using FunnyActivities.WebAPI;
using Newtonsoft.Json;

namespace FunnyActivities.Api.IntegrationTests
{
    public class ProductManagementIntegrationTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;
        private readonly HttpClient _client;

        public ProductManagementIntegrationTests(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task CreateBaseProduct_ValidRequest_ReturnsCreatedProduct()
        {
            // Arrange
            var request = new
            {
                name = "Test Product",
                description = "Test product description",
                categoryId = Guid.NewGuid().ToString()
            };

            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/products/base", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseData = JsonConvert.DeserializeObject<ApiResponse<dynamic>>(responseContent);

            responseData.success.Should().BeTrue();
            responseData.data.Should().NotBeNull();
            responseData.data.id.Should().NotBeNull();
            responseData.data.name.Should().Be("Test Product");
            responseData.data.description.Should().Be("Test product description");
        }

        [Fact]
        public async Task CreateBaseProduct_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var request = new
            {
                name = "", // Invalid: empty name
                description = "Test product description"
            };

            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/products/base", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseData = JsonConvert.DeserializeObject<ApiResponse<dynamic>>(responseContent);

            responseData.success.Should().BeFalse();
            responseData.message.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task CreateProductVariant_ValidRequest_ReturnsCreatedVariant()
        {
            // Arrange - First create a base product
            var baseProductRequest = new
            {
                name = "Base Product for Variant",
                description = "Base product description"
            };

            var baseProductContent = new StringContent(JsonConvert.SerializeObject(baseProductRequest), Encoding.UTF8, "application/json");
            var baseProductResponse = await _client.PostAsync("/api/products/base", baseProductContent);
            var baseProductResponseContent = await baseProductResponse.Content.ReadAsStringAsync();
            var baseProductData = JsonConvert.DeserializeObject<ApiResponse<dynamic>>(baseProductResponseContent);
            var baseProductId = baseProductData.data.id;

            // Create unit of measure first
            var unitRequest = new
            {
                name = "Test Unit",
                symbol = "TU",
                type = "count"
            };

            var unitContent = new StringContent(JsonConvert.SerializeObject(unitRequest), Encoding.UTF8, "application/json");
            var unitResponse = await _client.PostAsync("/api/units-of-measure", unitContent);
            var unitResponseContent = await unitResponse.Content.ReadAsStringAsync();
            var unitData = JsonConvert.DeserializeObject<ApiResponse<dynamic>>(unitResponseContent);
            var unitId = unitData.data.id;

            // Now create variant
            var variantRequest = new
            {
                baseProductId = baseProductId,
                name = "Test Variant",
                stockQuantity = 10,
                unitOfMeasureId = unitId,
                unitValue = 1,
                usageNotes = "Test usage notes",
                dynamicProperties = new Dictionary<string, object>
                {
                    { "size", "M" },
                    { "color", "Blue" }
                }
            };

            var variantContent = new StringContent(JsonConvert.SerializeObject(variantRequest), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/products/variants", variantContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseData = JsonConvert.DeserializeObject<ApiResponse<dynamic>>(responseContent);

            responseData.success.Should().BeTrue();
            responseData.data.Should().NotBeNull();
            responseData.data.id.Should().NotBeNull();
            responseData.data.name.Should().Be("Test Variant");
            responseData.data.stockQuantity.Should().Be(10);
            responseData.data.unitOfMeasureId.Should().Be(unitId);
            responseData.data.unitValue.Should().Be(1);
            responseData.data.usageNotes.Should().Be("Test usage notes");
        }

        [Fact]
        public async Task GetProductVariants_ValidBaseProductId_ReturnsVariants()
        {
            // Arrange - Create base product and variant first
            var baseProductRequest = new
            {
                name = "Base Product for Get Test",
                description = "Base product description"
            };

            var baseProductContent = new StringContent(JsonConvert.SerializeObject(baseProductRequest), Encoding.UTF8, "application/json");
            var baseProductResponse = await _client.PostAsync("/api/products/base", baseProductContent);
            var baseProductResponseContent = await baseProductResponse.Content.ReadAsStringAsync();
            var baseProductData = JsonConvert.DeserializeObject<ApiResponse<dynamic>>(baseProductResponseContent);
            var baseProductId = baseProductData.data.id;

            // Act
            var response = await _client.GetAsync($"/api/products/{baseProductId}/variants");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseData = JsonConvert.DeserializeObject<ApiResponse<List<dynamic>>>(responseContent);

            responseData.success.Should().BeTrue();
            responseData.data.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateProductVariant_ValidRequest_ReturnsUpdatedVariant()
        {
            // Arrange - Create base product and variant first
            var baseProductRequest = new
            {
                name = "Base Product for Update Test",
                description = "Base product description"
            };

            var baseProductContent = new StringContent(JsonConvert.SerializeObject(baseProductRequest), Encoding.UTF8, "application/json");
            var baseProductResponse = await _client.PostAsync("/api/products/base", baseProductContent);
            var baseProductResponseContent = await baseProductResponse.Content.ReadAsStringAsync();
            var baseProductData = JsonConvert.DeserializeObject<ApiResponse<dynamic>>(baseProductResponseContent);
            var baseProductId = baseProductData.data.id;

            // Create unit of measure
            var unitRequest = new
            {
                name = "Test Unit for Update",
                symbol = "TUU",
                type = "count"
            };

            var unitContent = new StringContent(JsonConvert.SerializeObject(unitRequest), Encoding.UTF8, "application/json");
            var unitResponse = await _client.PostAsync("/api/units-of-measure", unitContent);
            var unitResponseContent = await unitResponse.Content.ReadAsStringAsync();
            var unitData = JsonConvert.DeserializeObject<ApiResponse<dynamic>>(unitResponseContent);
            var unitId = unitData.data.id;

            // Create variant
            var variantRequest = new
            {
                baseProductId = baseProductId,
                name = "Original Variant",
                stockQuantity = 5,
                unitOfMeasureId = unitId,
                unitValue = 1,
                usageNotes = "Original notes"
            };

            var variantContent = new StringContent(JsonConvert.SerializeObject(variantRequest), Encoding.UTF8, "application/json");
            var variantResponse = await _client.PostAsync("/api/products/variants", variantContent);
            var variantResponseContent = await variantResponse.Content.ReadAsStringAsync();
            var variantData = JsonConvert.DeserializeObject<ApiResponse<dynamic>>(variantResponseContent);
            var variantId = variantData.data.id;

            // Update variant
            var updateRequest = new
            {
                name = "Updated Variant",
                stockQuantity = 15,
                unitValue = 2,
                usageNotes = "Updated notes"
            };

            var updateContent = new StringContent(JsonConvert.SerializeObject(updateRequest), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/api/products/variants/{variantId}", updateContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseData = JsonConvert.DeserializeObject<ApiResponse<dynamic>>(responseContent);

            responseData.success.Should().BeTrue();
            responseData.data.Should().NotBeNull();
            responseData.data.name.Should().Be("Updated Variant");
            responseData.data.stockQuantity.Should().Be(15);
            responseData.data.unitValue.Should().Be(2);
            responseData.data.usageNotes.Should().Be("Updated notes");
        }

        [Fact]
        public async Task BulkUpdateProductVariants_ValidRequest_ReturnsBulkUpdateResponse()
        {
            // Arrange - Create base product and variants first
            var baseProductRequest = new
            {
                name = "Base Product for Bulk Update Test",
                description = "Base product description"
            };

            var baseProductContent = new StringContent(JsonConvert.SerializeObject(baseProductRequest), Encoding.UTF8, "application/json");
            var baseProductResponse = await _client.PostAsync("/api/products/base", baseProductContent);
            var baseProductResponseContent = await baseProductResponse.Content.ReadAsStringAsync();
            var baseProductData = JsonConvert.DeserializeObject<ApiResponse<dynamic>>(baseProductResponseContent);
            var baseProductId = baseProductData.data.id;

            // Create unit of measure
            var unitRequest = new
            {
                name = "Test Unit for Bulk",
                symbol = "TUB",
                type = "count"
            };

            var unitContent = new StringContent(JsonConvert.SerializeObject(unitRequest), Encoding.UTF8, "application/json");
            var unitResponse = await _client.PostAsync("/api/units-of-measure", unitContent);
            var unitResponseContent = await unitResponse.Content.ReadAsStringAsync();
            var unitData = JsonConvert.DeserializeObject<ApiResponse<dynamic>>(unitResponseContent);
            var unitId = unitData.data.id;

            // Create two variants
            var variant1Request = new
            {
                baseProductId = baseProductId,
                name = "Variant 1",
                stockQuantity = 10,
                unitOfMeasureId = unitId,
                unitValue = 1
            };

            var variant2Request = new
            {
                baseProductId = baseProductId,
                name = "Variant 2",
                stockQuantity = 20,
                unitOfMeasureId = unitId,
                unitValue = 1
            };

            var variant1Content = new StringContent(JsonConvert.SerializeObject(variant1Request), Encoding.UTF8, "application/json");
            var variant2Content = new StringContent(JsonConvert.SerializeObject(variant2Request), Encoding.UTF8, "application/json");

            var variant1Response = await _client.PostAsync("/api/products/variants", variant1Content);
            var variant2Response = await _client.PostAsync("/api/products/variants", variant2Content);

            var variant1ResponseContent = await variant1Response.Content.ReadAsStringAsync();
            var variant2ResponseContent = await variant2Response.Content.ReadAsStringAsync();

            var variant1Data = JsonConvert.DeserializeObject<ApiResponse<dynamic>>(variant1ResponseContent);
            var variant2Data = JsonConvert.DeserializeObject<ApiResponse<dynamic>>(variant2ResponseContent);

            var variant1Id = variant1Data.data.id;
            var variant2Id = variant2Data.data.id;

            // Bulk update request
            var bulkUpdateRequest = new
            {
                updates = new[]
                {
                    new
                    {
                        id = variant1Id,
                        stockQuantity = 15,
                        usageNotes = "Updated variant 1"
                    },
                    new
                    {
                        id = variant2Id,
                        stockQuantity = 25,
                        usageNotes = "Updated variant 2"
                    }
                }
            };

            var bulkUpdateContent = new StringContent(JsonConvert.SerializeObject(bulkUpdateRequest), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync("/api/products/variants/bulk", bulkUpdateContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseData = JsonConvert.DeserializeObject<ApiResponse<dynamic>>(responseContent);

            responseData.success.Should().BeTrue();
            responseData.data.Should().NotBeNull();
            responseData.data.totalUpdates.Should().Be(2);
            responseData.data.successfulUpdates.Should().Be(2);
            responseData.data.failedUpdates.Should().Be(0);
            responseData.data.updatedVariants.Should().HaveCount(2);
            responseData.data.errors.Should().BeEmpty();
        }

        [Fact]
        public async Task GetProducts_WithPagination_ReturnsPagedResults()
        {
            // Act
            var response = await _client.GetAsync("/api/products?page=1&pageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseData = JsonConvert.DeserializeObject<ApiResponse<dynamic>>(responseContent);

            responseData.success.Should().BeTrue();
            responseData.data.Should().NotBeNull();
            responseData.data.items.Should().NotBeNull();
            responseData.data.page.Should().Be(1);
            responseData.data.pageSize.Should().Be(10);
        }

        [Fact]
        public async Task GetProductCategories_ReturnsCategoriesList()
        {
            // Act
            var response = await _client.GetAsync("/api/products/categories");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseData = JsonConvert.DeserializeObject<ApiResponse<List<dynamic>>>(responseContent);

            responseData.success.Should().BeTrue();
            responseData.data.Should().NotBeNull();
        }

        [Fact]
        public async Task GetUnitsOfMeasure_ReturnsUnitsList()
        {
            // Act
            var response = await _client.GetAsync("/api/units-of-measure");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseData = JsonConvert.DeserializeObject<ApiResponse<List<dynamic>>>(responseContent);

            responseData.success.Should().BeTrue();
            responseData.data.Should().NotBeNull();
        }

        [Fact]
        public async Task DeleteProductVariant_ValidId_ReturnsSuccess()
        {
            // Arrange - Create base product and variant first
            var baseProductRequest = new
            {
                name = "Base Product for Delete Test",
                description = "Base product description"
            };

            var baseProductContent = new StringContent(JsonConvert.SerializeObject(baseProductRequest), Encoding.UTF8, "application/json");
            var baseProductResponse = await _client.PostAsync("/api/products/base", baseProductContent);
            var baseProductResponseContent = await baseProductResponse.Content.ReadAsStringAsync();
            var baseProductData = JsonConvert.DeserializeObject<ApiResponse<dynamic>>(baseProductResponseContent);
            var baseProductId = baseProductData.data.id;

            // Create unit of measure
            var unitRequest = new
            {
                name = "Test Unit for Delete",
                symbol = "TUD",
                type = "count"
            };

            var unitContent = new StringContent(JsonConvert.SerializeObject(unitRequest), Encoding.UTF8, "application/json");
            var unitResponse = await _client.PostAsync("/api/units-of-measure", unitContent);
            var unitResponseContent = await unitResponse.Content.ReadAsStringAsync();
            var unitData = JsonConvert.DeserializeObject<ApiResponse<dynamic>>(unitResponseContent);
            var unitId = unitData.data.id;

            // Create variant
            var variantRequest = new
            {
                baseProductId = baseProductId,
                name = "Variant to Delete",
                stockQuantity = 5,
                unitOfMeasureId = unitId,
                unitValue = 1
            };

            var variantContent = new StringContent(JsonConvert.SerializeObject(variantRequest), Encoding.UTF8, "application/json");
            var variantResponse = await _client.PostAsync("/api/products/variants", variantContent);
            var variantResponseContent = await variantResponse.Content.ReadAsStringAsync();
            var variantData = JsonConvert.DeserializeObject<ApiResponse<dynamic>>(variantResponseContent);
            var variantId = variantData.data.id;

            // Act
            var response = await _client.DeleteAsync($"/api/products/variants/{variantId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseData = JsonConvert.DeserializeObject<ApiResponse<dynamic>>(responseContent);

            responseData.success.Should().BeTrue();
        }

        [Fact]
        public async Task GetNonExistentProduct_ReturnsNotFound()
        {
            // Act
            var response = await _client.GetAsync($"/api/products/{Guid.NewGuid()}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseData = JsonConvert.DeserializeObject<ApiResponse<dynamic>>(responseContent);

            responseData.success.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateNonExistentProductVariant_ReturnsNotFound()
        {
            // Arrange
            var updateRequest = new
            {
                name = "Updated Name"
            };

            var content = new StringContent(JsonConvert.SerializeObject(updateRequest), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/api/products/variants/{Guid.NewGuid()}", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseData = JsonConvert.DeserializeObject<ApiResponse<dynamic>>(responseContent);

            responseData.success.Should().BeFalse();
        }

        // Helper class for API response deserialization
        private class ApiResponse<T>
        {
            public bool success { get; set; }
            public string message { get; set; }
            public T data { get; set; }
            public string error { get; set; }
        }
    }
}