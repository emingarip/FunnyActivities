using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using FunnyActivities.Application.DTOs.ActivityManagement;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Xunit;

namespace FunnyActivities.Api.IntegrationTests
{
    public class ActivityControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public ActivityControllerTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetActivities_ShouldReturnOk_WhenCalled()
        {
            // Act
            var response = await _client.GetAsync("/api/activities");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetActivity_ShouldReturnNotFound_WhenActivityDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/activities/{nonExistentId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateActivity_ShouldReturnCreated_WhenValidRequest()
        {
            // Arrange
            var request = new CreateActivityRequest
            {
                Name = "Test Activity",
                Description = "Test Description",
                VideoUrl = "https://example.com/video.mp4",
                DurationHours = 1,
                DurationMinutes = 30,
                DurationSeconds = 0,
                ActivityCategoryId = Guid.NewGuid()
            };

            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/activities", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task CreateActivity_ShouldReturnBadRequest_WhenInvalidRequest()
        {
            // Arrange
            var request = new CreateActivityRequest
            {
                Name = "", // Invalid: empty name
                ActivityCategoryId = Guid.NewGuid()
            };

            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/activities", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}