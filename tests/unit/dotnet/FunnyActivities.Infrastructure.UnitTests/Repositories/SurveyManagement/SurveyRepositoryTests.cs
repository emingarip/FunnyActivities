using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Domain.Interfaces;
using FunnyActivities.Infrastructure;
using Xunit;

namespace FunnyActivities.Infrastructure.UnitTests.Repositories.SurveyManagement
{
    public class SurveyRepositoryTests
    {
        private readonly Fixture _fixture;
        private readonly ApplicationDbContext _context;
        private readonly SurveyRepository _repository;

        public SurveyRepositoryTests()
        {
            _fixture = new Fixture();
            _fixture.Customize<Survey>(s => s
                .With(s => s.Id, Guid.NewGuid)
                .With(s => s.CreatedAt, DateTime.UtcNow)
                .With(s => s.UpdatedAt, DateTime.UtcNow));

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new SurveyRepository(_context);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnSurvey_WhenSurveyExists()
        {
            // Arrange
            var survey = _fixture.Create<Survey>();
            await _context.Surveys.AddAsync(survey);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(survey.Id);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(survey);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenSurveyDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _repository.GetByIdAsync(nonExistentId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllSurveys()
        {
            // Arrange
            var surveys = _fixture.CreateMany<Survey>(3).ToList();
            await _context.Surveys.AddRangeAsync(surveys);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.Should().HaveCount(3);
            result.Should().BeEquivalentTo(surveys);
        }

        [Fact]
        public async Task GetByUserIdAsync_ShouldReturnUserSurveys()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userSurveys = _fixture.Build<Survey>()
                .With(s => s.CreatedByUserId, userId)
                .CreateMany(2)
                .ToList();

            var otherSurveys = _fixture.Build<Survey>()
                .With(s => s.CreatedByUserId, Guid.NewGuid())
                .CreateMany(3)
                .ToList();

            await _context.Surveys.AddRangeAsync(userSurveys.Concat(otherSurveys));
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByUserIdAsync(userId);

            // Assert
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(userSurveys);
        }

        [Fact]
        public async Task GetActiveAsync_ShouldReturnOnlyActiveSurveys()
        {
            // Arrange
            var activeSurveys = _fixture.Build<Survey>()
                .With(s => s.IsActive, true)
                .CreateMany(2)
                .ToList();

            var inactiveSurveys = _fixture.Build<Survey>()
                .With(s => s.IsActive, false)
                .CreateMany(3)
                .ToList();

            await _context.Surveys.AddRangeAsync(activeSurveys.Concat(inactiveSurveys));
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetActiveAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(activeSurveys);
        }

        [Fact]
        public async Task GetByTitleAsync_ShouldReturnMatchingSurveys()
        {
            // Arrange
            var searchTitle = "Test Survey";
            var matchingSurveys = _fixture.Build<Survey>()
                .With(s => s.Title, searchTitle)
                .CreateMany(2)
                .ToList();

            var nonMatchingSurveys = _fixture.Build<Survey>()
                .With(s => s.Title, "Different Survey")
                .CreateMany(3)
                .ToList();

            await _context.Surveys.AddRangeAsync(matchingSurveys.Concat(nonMatchingSurveys));
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByTitleAsync(searchTitle);

            // Assert
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(matchingSurveys);
        }

        [Fact]
        public async Task AddAsync_ShouldAddSurveyToDatabase()
        {
            // Arrange
            var survey = _fixture.Create<Survey>();

            // Act
            await _repository.AddAsync(survey);
            await _context.SaveChangesAsync();

            // Assert
            var result = await _context.Surveys.FindAsync(survey.Id);
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(survey);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateSurveyInDatabase()
        {
            // Arrange
            var survey = _fixture.Create<Survey>();
            await _context.Surveys.AddAsync(survey);
            await _context.SaveChangesAsync();

            var updatedTitle = "Updated Survey Title";
            survey.Title = updatedTitle;

            // Act
            await _repository.UpdateAsync(survey);
            await _context.SaveChangesAsync();

            // Assert
            var result = await _context.Surveys.FindAsync(survey.Id);
            result.Should().NotBeNull();
            result.Title.Should().Be(updatedTitle);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveSurveyFromDatabase()
        {
            // Arrange
            var survey = _fixture.Create<Survey>();
            await _context.Surveys.AddAsync(survey);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(survey);
            await _context.SaveChangesAsync();

            // Assert
            var result = await _context.Surveys.FindAsync(survey.Id);
            result.Should().BeNull();
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnTrue_WhenSurveyExists()
        {
            // Arrange
            var survey = _fixture.Create<Survey>();
            await _context.Surveys.AddAsync(survey);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.ExistsAsync(survey.Id);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnFalse_WhenSurveyDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _repository.ExistsAsync(nonExistentId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ExistsByTitleAsync_ShouldReturnTrue_WhenTitleExists()
        {
            // Arrange
            var title = "Test Survey Title";
            var survey = _fixture.Build<Survey>()
                .With(s => s.Title, title)
                .Create();

            await _context.Surveys.AddAsync(survey);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.ExistsByTitleAsync(title);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByTitleAsync_ShouldReturnFalse_WhenTitleDoesNotExist()
        {
            // Arrange
            var nonExistentTitle = "Non-existent Title";

            // Act
            var result = await _repository.ExistsByTitleAsync(nonExistentTitle);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetWithActivitiesAsync_ShouldIncludeActivities()
        {
            // Arrange
            var survey = _fixture.Create<Survey>();
            var activities = _fixture.CreateMany<Activity>(2).ToList();
            var surveyActivities = activities.Select(a => new SurveyActivity
            {
                SurveyId = survey.Id,
                ActivityId = a.Id,
                Activity = a
            }).ToList();

            survey.SurveyActivities = surveyActivities;
            await _context.Surveys.AddAsync(survey);
            await _context.Activities.AddRangeAsync(activities);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetWithActivitiesAsync(survey.Id);

            // Assert
            result.Should().HaveCount(1);
            var resultSurvey = result.First();
            resultSurvey.SurveyActivities.Should().HaveCount(2);
            resultSurvey.SurveyActivities.All(sa => sa.Activity != null).Should().BeTrue();
        }

        [Fact]
        public async Task GetWithParticipantsAsync_ShouldIncludeParticipants()
        {
            // Arrange
            var survey = _fixture.Create<Survey>();
            var user = _fixture.Create<User>();
            var surveyParticipant = new SurveyParticipant
            {
                SurveyId = survey.Id,
                UserId = user.Id,
                User = user
            };

            survey.SurveyParticipants = new List<SurveyParticipant> { surveyParticipant };
            await _context.Surveys.AddAsync(survey);
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetWithParticipantsAsync(survey.Id);

            // Assert
            result.Should().HaveCount(1);
            var resultSurvey = result.First();
            resultSurvey.SurveyParticipants.Should().HaveCount(1);
            resultSurvey.SurveyParticipants.First().User.Should().NotBeNull();
        }

        [Fact]
        public async Task GetWithVotesAsync_ShouldIncludeVotes()
        {
            // Arrange
            var survey = _fixture.Create<Survey>();
            var activity = _fixture.Create<Activity>();
            var user = _fixture.Create<User>();
            var surveyActivity = new SurveyActivity
            {
                SurveyId = survey.Id,
                ActivityId = activity.Id,
                Activity = activity
            };
            var surveyVote = new SurveyVote
            {
                SurveyId = survey.Id,
                SurveyActivityId = surveyActivity.Id,
                UserId = user.Id,
                VoteValue = 4,
                User = user,
                SurveyActivity = surveyActivity
            };

            survey.SurveyActivities = new List<SurveyActivity> { surveyActivity };
            survey.SurveyVotes = new List<SurveyVote> { surveyVote };

            await _context.Surveys.AddAsync(survey);
            await _context.Activities.AddAsync(activity);
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetWithVotesAsync(survey.Id);

            // Assert
            result.Should().HaveCount(1);
            var resultSurvey = result.First();
            resultSurvey.SurveyVotes.Should().HaveCount(1);
            resultSurvey.SurveyActivities.First().SurveyVotes.Should().HaveCount(1);
        }
    }
}