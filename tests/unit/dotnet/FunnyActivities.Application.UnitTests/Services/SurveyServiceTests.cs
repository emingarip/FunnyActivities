using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FunnyActivities.Application.UnitTests.Services
{
    public class SurveyServiceTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ILogger<SurveyService>> _loggerMock;
        private readonly SurveyService _service;

        public SurveyServiceTests()
        {
            _fixture = new Fixture();
            _mediatorMock = new Mock<IMediator>();
            _loggerMock = new Mock<ILogger<SurveyService>>();
            _service = new SurveyService(_mediatorMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task CreateSurveyAsync_ShouldReturnSurveyDto_WhenRequestIsValid()
        {
            // Arrange
            var request = new CreateSurveyRequest
            {
                Title = "Test Survey",
                Description = "Test Description",
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(7),
                ActivityIds = new List<Guid> { Guid.NewGuid() }
            };
            var userId = Guid.NewGuid();
            var expectedSurvey = new SurveyDto { Id = Guid.NewGuid(), Title = request.Title };

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateSurveyCommand>(), default))
                .ReturnsAsync(expectedSurvey);

            // Act
            var result = await _service.CreateSurveyAsync(request, userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedSurvey);
            _mediatorMock.Verify(m => m.Send(It.Is<CreateSurveyCommand>(c =>
                c.Title == request.Title &&
                c.Description == request.Description &&
                c.StartDate == request.StartDate &&
                c.EndDate == request.EndDate &&
                c.ActivityIds.SequenceEqual(request.ActivityIds) &&
                c.CreatedByUserId == userId
            ), default), Times.Once);
        }

        [Fact]
        public async Task UpdateSurveyAsync_ShouldReturnSurveyDto_WhenRequestIsValid()
        {
            // Arrange
            var surveyId = Guid.NewGuid();
            var request = new UpdateSurveyRequest
            {
                Title = "Updated Survey",
                Description = "Updated Description",
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(7),
                ActivityIds = new List<Guid> { Guid.NewGuid() }
            };
            var userId = Guid.NewGuid();
            var expectedSurvey = new SurveyDto { Id = surveyId, Title = request.Title };

            _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateSurveyCommand>(), default))
                .ReturnsAsync(expectedSurvey);

            // Act
            var result = await _service.UpdateSurveyAsync(surveyId, request, userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedSurvey);
            _mediatorMock.Verify(m => m.Send(It.Is<UpdateSurveyCommand>(c =>
                c.Id == surveyId &&
                c.Title == request.Title &&
                c.Description == request.Description &&
                c.StartDate == request.StartDate &&
                c.EndDate == request.EndDate &&
                c.ActivityIds.SequenceEqual(request.ActivityIds) &&
                c.UpdatedByUserId == userId
            ), default), Times.Once);
        }

        [Fact]
        public async Task DeleteSurveyAsync_ShouldReturnTrue_WhenDeletionIsSuccessful()
        {
            // Arrange
            var surveyId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteSurveyCommand>(), default))
                .ReturnsAsync(true);

            // Act
            var result = await _service.DeleteSurveyAsync(surveyId, userId);

            // Assert
            result.Should().BeTrue();
            _mediatorMock.Verify(m => m.Send(It.Is<DeleteSurveyCommand>(c =>
                c.Id == surveyId &&
                c.RequestedByUserId == userId
            ), default), Times.Once);
        }

        [Fact]
        public async Task DeleteSurveyAsync_ShouldReturnFalse_WhenDeletionFails()
        {
            // Arrange
            var surveyId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteSurveyCommand>(), default))
                .ReturnsAsync(false);

            // Act
            var result = await _service.DeleteSurveyAsync(surveyId, userId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetSurveyAsync_ShouldReturnSurveyDto_WhenSurveyExists()
        {
            // Arrange
            var surveyId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var expectedSurvey = new SurveyDto { Id = surveyId, Title = "Test Survey" };

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetSurveyQuery>(), default))
                .ReturnsAsync(expectedSurvey);

            // Act
            var result = await _service.GetSurveyAsync(surveyId, userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedSurvey);
            _mediatorMock.Verify(m => m.Send(It.Is<GetSurveyQuery>(q =>
                q.Id == surveyId &&
                q.RequestedByUserId == userId
            ), default), Times.Once);
        }

        [Fact]
        public async Task GetSurveysAsync_ShouldReturnSurveyList_WhenCalledWithDefaultParameters()
        {
            // Arrange
            var expectedSurveys = new List<SurveyListDto>
            {
                new SurveyListDto { Id = Guid.NewGuid(), Title = "Survey 1" },
                new SurveyListDto { Id = Guid.NewGuid(), Title = "Survey 2" }
            };

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetSurveysQuery>(), default))
                .ReturnsAsync(expectedSurveys);

            // Act
            var result = await _service.GetSurveysAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(expectedSurveys);
        }

        [Fact]
        public async Task GetSurveyResultsAsync_ShouldReturnSurveyResults_WhenSurveyExists()
        {
            // Arrange
            var surveyId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var expectedResults = new SurveyResultsDto { SurveyId = surveyId };

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetSurveyResultsQuery>(), default))
                .ReturnsAsync(expectedResults);

            // Act
            var result = await _service.GetSurveyResultsAsync(surveyId, userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedResults);
            _mediatorMock.Verify(m => m.Send(It.Is<GetSurveyResultsQuery>(q =>
                q.SurveyId == surveyId &&
                q.RequestedByUserId == userId
            ), default), Times.Once);
        }

        [Fact]
        public async Task GetSurveyStatisticsAsync_ShouldReturnSurveyStatistics_WhenSurveyExists()
        {
            // Arrange
            var surveyId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var expectedStatistics = new SurveyStatisticsDto { SurveyId = surveyId };

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetSurveyStatisticsQuery>(), default))
                .ReturnsAsync(expectedStatistics);

            // Act
            var result = await _service.GetSurveyStatisticsAsync(surveyId, userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedStatistics);
            _mediatorMock.Verify(m => m.Send(It.Is<GetSurveyStatisticsQuery>(q =>
                q.SurveyId == surveyId &&
                q.RequestedByUserId == userId
            ), default), Times.Once);
        }

        [Fact]
        public async Task SurveyTitleExistsAsync_ShouldReturnTrue_WhenTitleExists()
        {
            // Arrange
            var title = "Test Survey Title";
            var surveys = new List<SurveyListDto>
            {
                new SurveyListDto { Id = Guid.NewGuid(), Title = title }
            };

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetSurveysQuery>(), default))
                .ReturnsAsync(surveys);

            // Act
            var result = await _service.SurveyTitleExistsAsync(title);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task SurveyTitleExistsAsync_ShouldReturnFalse_WhenTitleDoesNotExist()
        {
            // Arrange
            var title = "Non-existent Title";
            var surveys = new List<SurveyListDto>
            {
                new SurveyListDto { Id = Guid.NewGuid(), Title = "Different Title" }
            };

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetSurveysQuery>(), default))
                .ReturnsAsync(surveys);

            // Act
            var result = await _service.SurveyTitleExistsAsync(title);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task SurveyTitleExistsAsync_ShouldExcludeSpecifiedSurvey_WhenExcludeSurveyIdIsProvided()
        {
            // Arrange
            var title = "Test Survey Title";
            var excludeSurveyId = Guid.NewGuid();
            var surveys = new List<SurveyListDto>
            {
                new SurveyListDto { Id = excludeSurveyId, Title = title }
            };

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetSurveysQuery>(), default))
                .ReturnsAsync(surveys);

            // Act
            var result = await _service.SurveyTitleExistsAsync(title, excludeSurveyId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task SurveyTitleExistsAsync_ShouldBeCaseInsensitive()
        {
            // Arrange
            var title = "Test Survey Title";
            var differentCaseTitle = "TEST SURVEY TITLE";
            var surveys = new List<SurveyListDto>
            {
                new SurveyListDto { Id = Guid.NewGuid(), Title = title }
            };

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetSurveysQuery>(), default))
                .ReturnsAsync(surveys);

            // Act
            var result = await _service.SurveyTitleExistsAsync(differentCaseTitle);

            // Assert
            result.Should().BeTrue();
        }

        // Mock DTOs for testing
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

    // Mock SurveyService class for testing
    public class SurveyService
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SurveyService> _logger;

        public SurveyService(IMediator mediator, ILogger<SurveyService> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<SurveyServiceTests.SurveyDto> CreateSurveyAsync(SurveyServiceTests.CreateSurveyRequest request, Guid createdByUserId)
        {
            var command = new SurveyServiceTests.CreateSurveyCommand
            {
                Title = request.Title,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                ActivityIds = request.ActivityIds,
                CreatedByUserId = createdByUserId
            };

            var result = await _mediator.Send(command);
            return result;
        }

        public async Task<SurveyServiceTests.SurveyDto> UpdateSurveyAsync(Guid id, SurveyServiceTests.UpdateSurveyRequest request, Guid updatedByUserId)
        {
            var command = new SurveyServiceTests.UpdateSurveyCommand
            {
                Id = id,
                Title = request.Title,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                ActivityIds = request.ActivityIds,
                UpdatedByUserId = updatedByUserId
            };

            var result = await _mediator.Send(command);
            return result;
        }

        public async Task<bool> DeleteSurveyAsync(Guid id, Guid requestedByUserId)
        {
            var command = new SurveyServiceTests.DeleteSurveyCommand
            {
                Id = id,
                RequestedByUserId = requestedByUserId
            };

            var result = await _mediator.Send(command);
            return result;
        }

        public async Task<SurveyServiceTests.SurveyDto> GetSurveyAsync(Guid id, Guid? requestedByUserId = null)
        {
            var query = new SurveyServiceTests.GetSurveyQuery
            {
                Id = id,
                RequestedByUserId = requestedByUserId
            };

            var result = await _mediator.Send(query);
            return result;
        }

        public async Task<List<SurveyServiceTests.SurveyListDto>> GetSurveysAsync(
            Guid? userId = null,
            bool? isActive = null,
            bool? isCurrentlyActive = null,
            string searchTerm = null,
            int pageNumber = 1,
            int pageSize = 10,
            string sortBy = "CreatedAt",
            bool sortDescending = true)
        {
            var query = new SurveyServiceTests.GetSurveysQuery
            {
                UserId = userId,
                IsActive = isActive,
                IsCurrentlyActive = isCurrentlyActive,
                SearchTerm = searchTerm,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                SortDescending = sortDescending
            };

            var result = await _mediator.Send(query);
            return result;
        }

        public async Task<SurveyServiceTests.SurveyResultsDto> GetSurveyResultsAsync(Guid surveyId, Guid requestedByUserId)
        {
            var query = new SurveyServiceTests.GetSurveyResultsQuery
            {
                SurveyId = surveyId,
                RequestedByUserId = requestedByUserId
            };

            var result = await _mediator.Send(query);
            return result;
        }

        public async Task<SurveyServiceTests.SurveyStatisticsDto> GetSurveyStatisticsAsync(
            Guid surveyId,
            Guid requestedByUserId,
            bool includeDailyStatistics = false,
            bool includeDetailedActivityStats = true)
        {
            var query = new SurveyServiceTests.GetSurveyStatisticsQuery
            {
                SurveyId = surveyId,
                RequestedByUserId = requestedByUserId,
                IncludeDailyStatistics = includeDailyStatistics,
                IncludeDetailedActivityStats = includeDetailedActivityStats
            };

            var result = await _mediator.Send(query);
            return result;
        }

        public async Task<bool> SurveyTitleExistsAsync(string title, Guid? excludeSurveyId = null)
        {
            var query = new SurveyServiceTests.GetSurveysQuery { SearchTerm = title };
            var surveys = await _mediator.Send(query);

            return surveys.Any(s => string.Equals(s.Title, title, StringComparison.OrdinalIgnoreCase) &&
                                  (!excludeSurveyId.HasValue || s.Id != excludeSurveyId.Value));
        }
    }
}