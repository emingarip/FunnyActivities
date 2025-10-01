using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using FunnyActivities.WebAPI.Controllers;
using Xunit;

namespace FunnyActivities.WebAPI.UnitTests.Controllers
{
    public class SurveyControllerTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ILogger<SurveyController>> _loggerMock;
        private readonly SurveyController _controller;
        private readonly Mock<HttpContext> _httpContextMock;
        private readonly Mock<ClaimsPrincipal> _userMock;

        public SurveyControllerTests()
        {
            _fixture = new Fixture();
            _fixture.Customize<SurveyDto>(s => s
                .With(s => s.Id, Guid.NewGuid)
                .With(s => s.CreatedAt, DateTime.UtcNow)
                .With(s => s.UpdatedAt, DateTime.UtcNow));

            _fixture.Customize<CreateSurveyRequest>(r => r
                .With(r => r.StartDate, DateTime.UtcNow.AddDays(1))
                .With(r => r.EndDate, (DateTime?)DateTime.UtcNow.AddDays(7)));

            _fixture.Customize<UpdateSurveyRequest>(r => r
                .With(r => r.StartDate, DateTime.UtcNow.AddDays(1))
                .With(r => r.EndDate, (DateTime?)DateTime.UtcNow.AddDays(7)));

            _mediatorMock = new Mock<IMediator>();
            _loggerMock = new Mock<ILogger<SurveyController>>();
            _controller = new SurveyController(_mediatorMock.Object, _loggerMock.Object);

            // Setup HttpContext and User
            _httpContextMock = new Mock<HttpContext>();
            _userMock = new Mock<ClaimsPrincipal>();

            var userId = Guid.NewGuid().ToString();
            _userMock.Setup(u => u.FindFirst("sub")).Returns(new Claim("sub", userId));
            _userMock.Setup(u => u.FindFirst(ClaimTypes.NameIdentifier)).Returns(new Claim(ClaimTypes.NameIdentifier, userId));
            _httpContextMock.Setup(c => c.User).Returns(_userMock.Object);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _httpContextMock.Object
            };
        }

        [Fact]
        public async Task GetSurveys_ShouldReturnOkResult_WithSurveyList()
        {
            // Arrange
            var expectedSurveys = _fixture.CreateMany<SurveyListDto>(3).ToList();
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetSurveysQuery>(), default))
                .ReturnsAsync(expectedSurveys);

            // Act
            var result = await _controller.GetSurveys();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().BeEquivalentTo(expectedSurveys);
        }

        [Fact]
        public async Task GetSurveys_WithPagination_ShouldReturnOkResult_WithPaginatedSurveys()
        {
            // Arrange
            var pageNumber = 2;
            var pageSize = 5;
            var expectedSurveys = _fixture.CreateMany<SurveyListDto>(pageSize).ToList();

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetSurveysQuery>(), default))
                .ReturnsAsync(expectedSurveys);

            // Act
            var result = await _controller.GetSurveys(pageNumber, pageSize);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().BeEquivalentTo(expectedSurveys);
        }

        [Fact]
        public async Task GetSurveys_WithLargePageSize_ShouldLimitPageSize()
        {
            // Arrange
            var pageSize = 200; // Exceeds maximum
            var expectedSurveys = _fixture.CreateMany<SurveyListDto>(100).ToList(); // Should be limited to 100

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetSurveysQuery>(), default))
                .ReturnsAsync(expectedSurveys);

            // Act
            var result = await _controller.GetSurveys(1, pageSize);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _mediatorMock.Verify(m => m.Send(It.Is<GetSurveysQuery>(q => q.PageSize == 100), default), Times.Once);
        }

        [Fact]
        public async Task GetSurvey_ShouldReturnOkResult_WhenSurveyExists()
        {
            // Arrange
            var surveyId = Guid.NewGuid();
            var expectedSurvey = _fixture.Create<SurveyDto>();

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetSurveyQuery>(), default))
                .ReturnsAsync(expectedSurvey);

            // Act
            var result = await _controller.GetSurvey(surveyId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().BeEquivalentTo(expectedSurvey);
        }

        [Fact]
        public async Task GetSurvey_ShouldReturnNotFound_WhenSurveyDoesNotExist()
        {
            // Arrange
            var surveyId = Guid.NewGuid();

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetSurveyQuery>(), default))
                .ReturnsAsync((SurveyDto)null);

            // Act
            var result = await _controller.GetSurvey(surveyId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Value.Should().Be("Survey not found");
        }

        [Fact]
        public async Task CreateSurvey_ShouldReturnCreatedResult_WhenRequestIsValid()
        {
            // Arrange
            var request = _fixture.Create<CreateSurveyRequest>();
            var expectedSurvey = _fixture.Create<SurveyDto>();

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateSurveyCommand>(), default))
                .ReturnsAsync(expectedSurvey);

            // Act
            var result = await _controller.CreateSurvey(request);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result as CreatedAtActionResult;
            createdResult.Value.Should().BeEquivalentTo(expectedSurvey);
            createdResult.ActionName.Should().Be(nameof(_controller.GetSurvey));
        }

        [Fact]
        public async Task CreateSurvey_ShouldReturnBadRequest_WhenArgumentExceptionIsThrown()
        {
            // Arrange
            var request = _fixture.Create<CreateSurveyRequest>();

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateSurveyCommand>(), default))
                .ThrowsAsync(new ArgumentException("Invalid survey data"));

            // Act
            var result = await _controller.CreateSurvey(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Value.Should().Be("Invalid survey data");
        }

        [Fact]
        public async Task CreateSurvey_ShouldReturnInternalServerError_WhenExceptionIsThrown()
        {
            // Arrange
            var request = _fixture.Create<CreateSurveyRequest>();

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateSurveyCommand>(), default))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.CreateSurvey(request);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task UpdateSurvey_ShouldReturnOkResult_WhenRequestIsValid()
        {
            // Arrange
            var surveyId = Guid.NewGuid();
            var request = _fixture.Create<UpdateSurveyRequest>();
            var expectedSurvey = _fixture.Create<SurveyDto>();

            _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateSurveyCommand>(), default))
                .ReturnsAsync(expectedSurvey);

            // Act
            var result = await _controller.UpdateSurvey(surveyId, request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().BeEquivalentTo(expectedSurvey);
        }

        [Fact]
        public async Task UpdateSurvey_ShouldReturnNotFound_WhenKeyNotFoundExceptionIsThrown()
        {
            // Arrange
            var surveyId = Guid.NewGuid();
            var request = _fixture.Create<UpdateSurveyRequest>();

            _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateSurveyCommand>(), default))
                .ThrowsAsync(new KeyNotFoundException("Survey not found"));

            // Act
            var result = await _controller.UpdateSurvey(surveyId, request);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Value.Should().Be("Survey not found");
        }

        [Fact]
        public async Task UpdateSurvey_ShouldReturnBadRequest_WhenArgumentExceptionIsThrown()
        {
            // Arrange
            var surveyId = Guid.NewGuid();
            var request = _fixture.Create<UpdateSurveyRequest>();

            _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateSurveyCommand>(), default))
                .ThrowsAsync(new ArgumentException("Invalid survey data"));

            // Act
            var result = await _controller.UpdateSurvey(surveyId, request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Value.Should().Be("Invalid survey data");
        }

        [Fact]
        public async Task DeleteSurvey_ShouldReturnNoContent_WhenDeletionIsSuccessful()
        {
            // Arrange
            var surveyId = Guid.NewGuid();

            _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteSurveyCommand>(), default))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteSurvey(surveyId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().Be("Survey deleted successfully");
            okResult.StatusCode.Should().Be(204);
        }

        [Fact]
        public async Task DeleteSurvey_ShouldReturnNotFound_WhenKeyNotFoundExceptionIsThrown()
        {
            // Arrange
            var surveyId = Guid.NewGuid();

            _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteSurveyCommand>(), default))
                .ThrowsAsync(new KeyNotFoundException("Survey not found"));

            // Act
            var result = await _controller.DeleteSurvey(surveyId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Value.Should().Be("Survey not found");
        }

        [Fact]
        public async Task GetSurveyResults_ShouldReturnOkResult_WhenSurveyExists()
        {
            // Arrange
            var surveyId = Guid.NewGuid();
            var expectedResults = _fixture.Create<SurveyResultsDto>();

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetSurveyResultsQuery>(), default))
                .ReturnsAsync(expectedResults);

            // Act
            var result = await _controller.GetSurveyResults(surveyId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().BeEquivalentTo(expectedResults);
        }

        [Fact]
        public async Task GetSurveyResults_ShouldReturnNotFound_WhenSurveyDoesNotExist()
        {
            // Arrange
            var surveyId = Guid.NewGuid();

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetSurveyResultsQuery>(), default))
                .ReturnsAsync((SurveyResultsDto)null);

            // Act
            var result = await _controller.GetSurveyResults(surveyId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Value.Should().Be("Survey not found");
        }

        [Fact]
        public async Task GetSurveyStatistics_ShouldReturnOkResult_WhenSurveyExists()
        {
            // Arrange
            var surveyId = Guid.NewGuid();
            var expectedStatistics = _fixture.Create<SurveyStatisticsDto>();

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetSurveyStatisticsQuery>(), default))
                .ReturnsAsync(expectedStatistics);

            // Act
            var result = await _controller.GetSurveyStatistics(surveyId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().BeEquivalentTo(expectedStatistics);
        }

        [Fact]
        public async Task GetSurveyStatistics_ShouldReturnNotFound_WhenSurveyDoesNotExist()
        {
            // Arrange
            var surveyId = Guid.NewGuid();

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetSurveyStatisticsQuery>(), default))
                .ReturnsAsync((SurveyStatisticsDto)null);

            // Act
            var result = await _controller.GetSurveyStatistics(surveyId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Value.Should().Be("Survey not found");
        }

        // Mock DTOs and Commands for testing
        public class CreateSurveyRequest
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public List<Guid> ActivityIds { get; set; } = new List<Guid>();
        }

        public class UpdateSurveyRequest
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public List<Guid> ActivityIds { get; set; } = new List<Guid>();
        }

        public class SurveyDto
        {
            public Guid Id { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public bool IsActive { get; set; }
            public bool IsCurrentlyActive { get; set; }
            public int MaxParticipants { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
        }

        public class SurveyListDto
        {
            public Guid Id { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public bool IsActive { get; set; }
            public bool IsCurrentlyActive { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public int TotalActivities { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        public class SurveyResultsDto
        {
            public Guid SurveyId { get; set; }
            public string Title { get; set; }
            public List<ActivityResultDto> ActivityResults { get; set; } = new List<ActivityResultDto>();
        }

        public class SurveyStatisticsDto
        {
            public Guid SurveyId { get; set; }
            public string Title { get; set; }
            public int TotalVotes { get; set; }
            public int TotalParticipants { get; set; }
            public List<ActivityStatisticsDto> ActivityStatistics { get; set; } = new List<ActivityStatisticsDto>();
        }

        public class ActivityResultDto
        {
            public Guid ActivityId { get; set; }
            public string ActivityName { get; set; }
            public double AverageRating { get; set; }
            public int VoteCount { get; set; }
            public List<VoteDetailDto> Votes { get; set; } = new List<VoteDetailDto>();
        }

        public class ActivityStatisticsDto
        {
            public Guid ActivityId { get; set; }
            public string ActivityName { get; set; }
            public double AverageRating { get; set; }
            public int VoteCount { get; set; }
            public int Rank { get; set; }
        }

        public class VoteDetailDto
        {
            public Guid VoteId { get; set; }
            public int VoteValue { get; set; }
            public string Comment { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        // Mock Commands and Queries
        public class CreateSurveyCommand : IRequest<SurveyDto>
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public List<Guid> ActivityIds { get; set; }
            public Guid CreatedByUserId { get; set; }
        }

        public class UpdateSurveyCommand : IRequest<SurveyDto>
        {
            public Guid Id { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public List<Guid> ActivityIds { get; set; }
            public Guid UpdatedByUserId { get; set; }
        }

        public class DeleteSurveyCommand : IRequest<bool>
        {
            public Guid Id { get; set; }
            public Guid RequestedByUserId { get; set; }
        }

        public class GetSurveyQuery : IRequest<SurveyDto>
        {
            public Guid Id { get; set; }
            public Guid? RequestedByUserId { get; set; }
        }

        public class GetSurveysQuery : IRequest<List<SurveyListDto>>
        {
            public Guid? UserId { get; set; }
            public bool? IsActive { get; set; }
            public bool? IsCurrentlyActive { get; set; }
            public string SearchTerm { get; set; }
            public int PageNumber { get; set; } = 1;
            public int PageSize { get; set; } = 10;
            public string SortBy { get; set; } = "CreatedAt";
            public bool SortDescending { get; set; } = true;
        }

        public class GetSurveyResultsQuery : IRequest<SurveyResultsDto>
        {
            public Guid SurveyId { get; set; }
            public Guid RequestedByUserId { get; set; }
        }

        public class GetSurveyStatisticsQuery : IRequest<SurveyStatisticsDto>
        {
            public Guid SurveyId { get; set; }
            public Guid RequestedByUserId { get; set; }
            public bool IncludeDailyStatistics { get; set; } = false;
            public bool IncludeDetailedActivityStats { get; set; } = true;
        }
    }
}