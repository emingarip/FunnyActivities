using System;
using System.Collections.Generic;
using System.Linq;
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
    public class PublicSurveyControllerTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ILogger<PublicSurveyController>> _loggerMock;
        private readonly PublicSurveyController _controller;
        private readonly Mock<HttpContext> _httpContextMock;
        private readonly Mock<ClaimsPrincipal> _userMock;

        public PublicSurveyControllerTests()
        {
            _fixture = new Fixture();
            _fixture.Customize<SurveyDto>(s => s
                .With(s => s.Id, Guid.NewGuid)
                .With(s => s.CreatedAt, DateTime.UtcNow)
                .With(s => s.UpdatedAt, DateTime.UtcNow));

            _fixture.Customize<VoteRequest>(r => r
                .With(r => r.VoteValue, _fixture.Create<int>() % 5 + 1)); // 1-5 range

            _mediatorMock = new Mock<IMediator>();
            _loggerMock = new Mock<ILogger<PublicSurveyController>>();
            _controller = new PublicSurveyController(_mediatorMock.Object, _loggerMock.Object);

            // Setup HttpContext and User (for authenticated scenarios)
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
        public async Task GetPublicSurvey_ShouldReturnOkResult_WhenSurveyExists()
        {
            // Arrange
            var surveyId = Guid.NewGuid();
            var expectedSurvey = _fixture.Create<SurveyDto>();

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetPublicSurveyQuery>(), default))
                .ReturnsAsync(expectedSurvey);

            // Act
            var result = await _controller.GetPublicSurvey(surveyId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().BeEquivalentTo(expectedSurvey);
        }

        [Fact]
        public async Task GetPublicSurvey_ShouldReturnNotFound_WhenSurveyDoesNotExist()
        {
            // Arrange
            var surveyId = Guid.NewGuid();

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetPublicSurveyQuery>(), default))
                .ReturnsAsync((SurveyDto)null);

            // Act
            var result = await _controller.GetPublicSurvey(surveyId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Value.Should().Be("Survey not found or not accessible");
        }

        [Fact]
        public async Task Vote_ShouldReturnOkResult_WhenVoteIsSuccessful()
        {
            // Arrange
            var surveyId = Guid.NewGuid();
            var request = _fixture.Create<VoteRequest>();
            var expectedVote = _fixture.Create<VoteDto>();

            _mediatorMock.Setup(m => m.Send(It.IsAny<VoteCommand>(), default))
                .ReturnsAsync(expectedVote);

            // Act
            var result = await _controller.Vote(surveyId, request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().BeEquivalentTo(expectedVote);
        }

        [Fact]
        public async Task Vote_ShouldReturnBadRequest_WhenArgumentExceptionIsThrown()
        {
            // Arrange
            var surveyId = Guid.NewGuid();
            var request = _fixture.Create<VoteRequest>();

            _mediatorMock.Setup(m => m.Send(It.IsAny<VoteCommand>(), default))
                .ThrowsAsync(new ArgumentException("Invalid vote data"));

            // Act
            var result = await _controller.Vote(surveyId, request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Value.Should().Be("Invalid vote data");
        }

        [Fact]
        public async Task Vote_ShouldReturnNotFound_WhenKeyNotFoundExceptionIsThrown()
        {
            // Arrange
            var surveyId = Guid.NewGuid();
            var request = _fixture.Create<VoteRequest>();

            _mediatorMock.Setup(m => m.Send(It.IsAny<VoteCommand>(), default))
                .ThrowsAsync(new KeyNotFoundException("Survey activity not found"));

            // Act
            var result = await _controller.Vote(surveyId, request);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Value.Should().Be("Survey activity not found");
        }

        [Fact]
        public async Task Vote_ShouldReturnInternalServerError_WhenExceptionIsThrown()
        {
            // Arrange
            var surveyId = Guid.NewGuid();
            var request = _fixture.Create<VoteRequest>();

            _mediatorMock.Setup(m => m.Send(It.IsAny<VoteCommand>(), default))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.Vote(surveyId, request);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task GetSurveyActivities_ShouldReturnOkResult_WhenActivitiesExist()
        {
            // Arrange
            var surveyId = Guid.NewGuid();
            var expectedActivities = _fixture.CreateMany<SurveyActivityDto>(3).ToList();

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetSurveyActivitiesQuery>(), default))
                .ReturnsAsync(expectedActivities);

            // Act
            var result = await _controller.GetSurveyActivities(surveyId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().BeEquivalentTo(expectedActivities);
        }

        [Fact]
        public async Task GetSurveyActivities_ShouldReturnNotFound_WhenNoActivitiesFound()
        {
            // Arrange
            var surveyId = Guid.NewGuid();
            var emptyActivities = new List<SurveyActivityDto>();

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetSurveyActivitiesQuery>(), default))
                .ReturnsAsync(emptyActivities);

            // Act
            var result = await _controller.GetSurveyActivities(surveyId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Value.Should().Be("No activities found for this survey");
        }

        [Fact]
        public async Task GetSurveyStatus_ShouldReturnOkResult_WhenSurveyExists()
        {
            // Arrange
            var surveyId = Guid.NewGuid();
            var survey = _fixture.Create<SurveyDto>();

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetPublicSurveyQuery>(), default))
                .ReturnsAsync(survey);

            // Act
            var result = await _controller.GetSurveyStatus(surveyId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var status = okResult.Value as dynamic;
            status.SurveyId.Should().Be(surveyId);
            status.Title.Should().Be(survey.Title);
            status.IsActive.Should().Be(survey.IsActive);
        }

        [Fact]
        public async Task GetSurveyStatus_ShouldReturnNotFound_WhenSurveyDoesNotExist()
        {
            // Arrange
            var surveyId = Guid.NewGuid();

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetPublicSurveyQuery>(), default))
                .ReturnsAsync((SurveyDto)null);

            // Act
            var result = await _controller.GetSurveyStatus(surveyId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Value.Should().Be("Survey not found");
        }

        [Fact]
        public async Task GetSurveyStatus_ShouldCalculateCanVoteCorrectly()
        {
            // Arrange
            var surveyId = Guid.NewGuid();
            var survey = _fixture.Build<SurveyDto>()
                .With(s => s.IsActive, true)
                .With(s => s.StartDate, DateTime.UtcNow.AddDays(-1)) // Started yesterday
                .With(s => s.EndDate, (DateTime?)DateTime.UtcNow.AddDays(1)) // Ends tomorrow
                .Create();

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetPublicSurveyQuery>(), default))
                .ReturnsAsync(survey);

            // Act
            var result = await _controller.GetSurveyStatus(surveyId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var status = okResult.Value as dynamic;
            status.CanVote.Should().BeTrue();
        }

        [Fact]
        public async Task GetSurveyStatus_ShouldReturnFalseForCanVote_WhenSurveyIsNotActive()
        {
            // Arrange
            var surveyId = Guid.NewGuid();
            var survey = _fixture.Build<SurveyDto>()
                .With(s => s.IsActive, false)
                .With(s => s.StartDate, DateTime.UtcNow.AddDays(-1))
                .With(s => s.EndDate, (DateTime?)DateTime.UtcNow.AddDays(1))
                .Create();

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetPublicSurveyQuery>(), default))
                .ReturnsAsync(survey);

            // Act
            var result = await _controller.GetSurveyStatus(surveyId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var status = okResult.Value as dynamic;
            status.CanVote.Should().BeFalse();
        }

        [Fact]
        public async Task GetSurveyStatus_ShouldReturnFalseForCanVote_WhenSurveyHasNotStarted()
        {
            // Arrange
            var surveyId = Guid.NewGuid();
            var survey = _fixture.Build<SurveyDto>()
                .With(s => s.IsActive, true)
                .With(s => s.StartDate, DateTime.UtcNow.AddDays(1)) // Starts tomorrow
                .With(s => s.EndDate, (DateTime?)DateTime.UtcNow.AddDays(2))
                .Create();

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetPublicSurveyQuery>(), default))
                .ReturnsAsync(survey);

            // Act
            var result = await _controller.GetSurveyStatus(surveyId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var status = okResult.Value as dynamic;
            status.CanVote.Should().BeFalse();
        }

        [Fact]
        public async Task GetSurveyStatus_ShouldReturnFalseForCanVote_WhenSurveyHasEnded()
        {
            // Arrange
            var surveyId = Guid.NewGuid();
            var survey = _fixture.Build<SurveyDto>()
                .With(s => s.IsActive, true)
                .With(s => s.StartDate, DateTime.UtcNow.AddDays(-2)) // Started 2 days ago
                .With(s => s.EndDate, (DateTime?)DateTime.UtcNow.AddDays(-1)) // Ended yesterday
                .Create();

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetPublicSurveyQuery>(), default))
                .ReturnsAsync(survey);

            // Act
            var result = await _controller.GetSurveyStatus(surveyId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var status = okResult.Value as dynamic;
            status.CanVote.Should().BeFalse();
        }

        // Mock DTOs and Commands for testing
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
            public bool HasReachedMaxParticipants { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
        }

        public class VoteRequest
        {
            public Guid SurveyActivityId { get; set; }
            public int VoteValue { get; set; }
            public string Comment { get; set; }
        }

        public class VoteDto
        {
            public Guid Id { get; set; }
            public Guid SurveyId { get; set; }
            public Guid SurveyActivityId { get; set; }
            public Guid UserId { get; set; }
            public int VoteValue { get; set; }
            public string Comment { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
        }

        public class SurveyActivityDto
        {
            public Guid Id { get; set; }
            public Guid SurveyId { get; set; }
            public Guid ActivityId { get; set; }
            public string ActivityName { get; set; }
            public string ActivityDescription { get; set; }
            public int Order { get; set; }
        }

        // Mock Commands and Queries
        public class GetPublicSurveyQuery : IRequest<SurveyDto>
        {
            public Guid SurveyId { get; set; }
        }

        public class VoteCommand : IRequest<VoteDto>
        {
            public Guid SurveyActivityId { get; set; }
            public int VoteValue { get; set; }
            public string Comment { get; set; }
            public Guid? UserId { get; set; }
        }

        public class GetSurveyActivitiesQuery : IRequest<List<SurveyActivityDto>>
        {
            public Guid SurveyId { get; set; }
        }
    }
}