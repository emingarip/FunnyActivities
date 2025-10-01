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
    public class VoteRepositoryTests
    {
        private readonly Fixture _fixture;
        private readonly ApplicationDbContext _context;
        private readonly VoteRepository _repository;

        public VoteRepositoryTests()
        {
            _fixture = new Fixture();
            _fixture.Customize<SurveyVote>(v => v
                .With(v => v.Id, Guid.NewGuid)
                .With(v => v.CreatedAt, DateTime.UtcNow)
                .With(v => v.UpdatedAt, DateTime.UtcNow)
                .With(v => v.VoteValue, _fixture.Create<int>() % 5 + 1) // 1-5 range
                .With(v => v.SurveyParticipantId, Guid.NewGuid));

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new VoteRepository(_context);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnVote_WhenVoteExists()
        {
            // Arrange
            var vote = _fixture.Create<SurveyVote>();
            await _context.SurveyVotes.AddAsync(vote);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(vote.Id);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(vote);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenVoteDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _repository.GetByIdAsync(nonExistentId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetBySurveyIdAsync_ShouldReturnAllVotesForSurvey()
        {
            // Arrange
            var surveyId = Guid.NewGuid();
            var surveyVotes = _fixture.Build<SurveyVote>()
                .With(v => v.SurveyId, surveyId)
                .CreateMany(3)
                .ToList();

            var otherVotes = _fixture.Build<SurveyVote>()
                .With(v => v.SurveyId, Guid.NewGuid())
                .CreateMany(2)
                .ToList();

            await _context.SurveyVotes.AddRangeAsync(surveyVotes.Concat(otherVotes));
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetBySurveyIdAsync(surveyId);

            // Assert
            result.Should().HaveCount(3);
            result.Should().BeEquivalentTo(surveyVotes);
        }

        [Fact]
        public async Task GetBySurveyActivityIdAsync_ShouldReturnAllVotesForActivity()
        {
            // Arrange
            var surveyActivityId = Guid.NewGuid();
            var activityVotes = _fixture.Build<SurveyVote>()
                .With(v => v.SurveyActivityId, surveyActivityId)
                .CreateMany(3)
                .ToList();

            var otherVotes = _fixture.Build<SurveyVote>()
                .With(v => v.SurveyActivityId, Guid.NewGuid())
                .CreateMany(2)
                .ToList();

            await _context.SurveyVotes.AddRangeAsync(activityVotes.Concat(otherVotes));
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetBySurveyActivityIdAsync(surveyActivityId);

            // Assert
            result.Should().HaveCount(3);
            result.Should().BeEquivalentTo(activityVotes);
        }

        [Fact]
        public async Task GetByParticipantIdAsync_ShouldReturnAllParticipantVotes()
        {
            // Arrange
            var surveyParticipantId = Guid.NewGuid();
            var participantVotes = _fixture.Build<SurveyVote>()
                .With(v => v.SurveyParticipantId, surveyParticipantId)
                .CreateMany(3)
                .ToList();

            var otherVotes = _fixture.Build<SurveyVote>()
                .With(v => v.SurveyParticipantId, Guid.NewGuid())
                .CreateMany(2)
                .ToList();

            await _context.SurveyVotes.AddRangeAsync(participantVotes.Concat(otherVotes));
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByParticipantIdAsync(surveyParticipantId);

            // Assert
            result.Should().HaveCount(3);
            result.Should().BeEquivalentTo(participantVotes);
        }

        [Fact]
        public async Task GetByUserAndActivityAsync_ShouldReturnVote_WhenUserVotedForActivity()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var surveyActivityId = Guid.NewGuid();
            var vote = _fixture.Build<SurveyVote>()
                .With(v => v.UserId, userId)
                .With(v => v.SurveyActivityId, surveyActivityId)
                .Create();

            await _context.SurveyVotes.AddAsync(vote);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByUserAndActivityAsync(userId, surveyActivityId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(vote);
        }

        [Fact]
        public async Task GetByUserAndActivityAsync_ShouldReturnNull_WhenUserDidNotVoteForActivity()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var surveyActivityId = Guid.NewGuid();

            // Act
            var result = await _repository.GetByUserAndActivityAsync(userId, surveyActivityId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByParticipantAndSurveyAsync_ShouldReturnParticipantVotesForSurvey()
        {
            // Arrange
            var surveyParticipantId = Guid.NewGuid();
            var surveyId = Guid.NewGuid();
            var participantSurveyVotes = _fixture.Build<SurveyVote>()
                .With(v => v.SurveyParticipantId, surveyParticipantId)
                .With(v => v.SurveyId, surveyId)
                .CreateMany(2)
                .ToList();

            var otherVotes = _fixture.Build<SurveyVote>()
                .With(v => v.SurveyParticipantId, surveyParticipantId)
                .With(v => v.SurveyId, Guid.NewGuid())
                .CreateMany(3)
                .ToList();

            await _context.SurveyVotes.AddRangeAsync(participantSurveyVotes.Concat(otherVotes));
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByParticipantAndSurveyAsync(surveyParticipantId, surveyId);

            // Assert
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(participantSurveyVotes);
        }

        [Fact]
        public async Task AddAsync_ShouldAddVoteToDatabase()
        {
            // Arrange
            var vote = _fixture.Create<SurveyVote>();

            // Act
            await _repository.AddAsync(vote);
            await _context.SaveChangesAsync();

            // Assert
            var result = await _context.SurveyVotes.FindAsync(vote.Id);
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(vote);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateVoteInDatabase()
        {
            // Arrange
            var vote = _fixture.Create<SurveyVote>();
            await _context.SurveyVotes.AddAsync(vote);
            await _context.SaveChangesAsync();

            var updatedVoteValue = 5;
            vote.VoteValue = updatedVoteValue;

            // Act
            await _repository.UpdateAsync(vote);
            await _context.SaveChangesAsync();

            // Assert
            var result = await _context.SurveyVotes.FindAsync(vote.Id);
            result.Should().NotBeNull();
            result.VoteValue.Should().Be(updatedVoteValue);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveVoteFromDatabase()
        {
            // Arrange
            var vote = _fixture.Create<SurveyVote>();
            await _context.SurveyVotes.AddAsync(vote);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(vote);
            await _context.SaveChangesAsync();

            // Assert
            var result = await _context.SurveyVotes.FindAsync(vote.Id);
            result.Should().BeNull();
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnTrue_WhenVoteExists()
        {
            // Arrange
            var vote = _fixture.Create<SurveyVote>();
            await _context.SurveyVotes.AddAsync(vote);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.ExistsAsync(vote.Id);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnFalse_WhenVoteDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _repository.ExistsAsync(nonExistentId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasParticipantVotedAsync_ShouldReturnTrue_WhenParticipantVotedForActivity()
        {
            // Arrange
            var surveyParticipantId = Guid.NewGuid();
            var surveyActivityId = Guid.NewGuid();
            var vote = _fixture.Build<SurveyVote>()
                .With(v => v.SurveyParticipantId, surveyParticipantId)
                .With(v => v.SurveyActivityId, surveyActivityId)
                .Create();

            await _context.SurveyVotes.AddAsync(vote);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.HasParticipantVotedAsync(surveyParticipantId, surveyActivityId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasParticipantVotedAsync_ShouldReturnFalse_WhenParticipantDidNotVoteForActivity()
        {
            // Arrange
            var surveyParticipantId = Guid.NewGuid();
            var surveyActivityId = Guid.NewGuid();

            // Act
            var result = await _repository.HasParticipantVotedAsync(surveyParticipantId, surveyActivityId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetAverageVoteForActivityAsync_ShouldReturnCorrectAverage()
        {
            // Arrange
            var surveyActivityId = Guid.NewGuid();
            var votes = new List<SurveyVote>
            {
                _fixture.Build<SurveyVote>()
                    .With(v => v.SurveyActivityId, surveyActivityId)
                    .With(v => v.VoteValue, 3)
                    .Create(),
                _fixture.Build<SurveyVote>()
                    .With(v => v.SurveyActivityId, surveyActivityId)
                    .With(v => v.VoteValue, 5)
                    .Create(),
                _fixture.Build<SurveyVote>()
                    .With(v => v.SurveyActivityId, surveyActivityId)
                    .With(v => v.VoteValue, 4)
                    .Create()
            };

            await _context.SurveyVotes.AddRangeAsync(votes);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAverageVoteForActivityAsync(surveyActivityId);

            // Assert
            result.Should().Be(4.0); // (3 + 5 + 4) / 3 = 4.0
        }

        [Fact]
        public async Task GetAverageVoteForActivityAsync_ShouldReturnZero_WhenNoVotes()
        {
            // Arrange
            var surveyActivityId = Guid.NewGuid();

            // Act
            var result = await _repository.GetAverageVoteForActivityAsync(surveyActivityId);

            // Assert
            result.Should().Be(0.0);
        }

        [Fact]
        public async Task GetVoteCountForActivityAsync_ShouldReturnCorrectCount()
        {
            // Arrange
            var surveyActivityId = Guid.NewGuid();
            var votes = _fixture.Build<SurveyVote>()
                .With(v => v.SurveyActivityId, surveyActivityId)
                .CreateMany(5)
                .ToList();

            await _context.SurveyVotes.AddRangeAsync(votes);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetVoteCountForActivityAsync(surveyActivityId);

            // Assert
            result.Should().Be(5);
        }

        [Fact]
        public async Task GetVoteCountForActivityAsync_ShouldReturnZero_WhenNoVotes()
        {
            // Arrange
            var surveyActivityId = Guid.NewGuid();

            // Act
            var result = await _repository.GetVoteCountForActivityAsync(surveyActivityId);

            // Assert
            result.Should().Be(0);
        }

        [Fact]
        public async Task GetVoteStatisticsForSurveyAsync_ShouldReturnCorrectStatistics()
        {
            // Arrange
            var surveyId = Guid.NewGuid();
            var activity1Id = Guid.NewGuid();
            var activity2Id = Guid.NewGuid();

            var votes = new List<SurveyVote>
            {
                _fixture.Build<SurveyVote>()
                    .With(v => v.SurveyId, surveyId)
                    .With(v => v.SurveyActivityId, activity1Id)
                    .With(v => v.VoteValue, 3)
                    .Create(),
                _fixture.Build<SurveyVote>()
                    .With(v => v.SurveyId, surveyId)
                    .With(v => v.SurveyActivityId, activity1Id)
                    .With(v => v.VoteValue, 5)
                    .Create(),
                _fixture.Build<SurveyVote>()
                    .With(v => v.SurveyId, surveyId)
                    .With(v => v.SurveyActivityId, activity2Id)
                    .With(v => v.VoteValue, 4)
                    .Create(),
                _fixture.Build<SurveyVote>()
                    .With(v => v.SurveyId, surveyId)
                    .With(v => v.SurveyActivityId, activity2Id)
                    .With(v => v.VoteValue, 4)
                    .Create(),
                _fixture.Build<SurveyVote>()
                    .With(v => v.SurveyId, surveyId)
                    .With(v => v.SurveyActivityId, activity2Id)
                    .With(v => v.VoteValue, 2)
                    .Create()
            };

            await _context.SurveyVotes.AddRangeAsync(votes);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetVoteStatisticsForSurveyAsync(surveyId);

            // Assert
            result.Should().HaveCount(2);
            result[activity1Id].Average.Should().Be(4.0); // (3 + 5) / 2 = 4.0
            result[activity1Id].Count.Should().Be(2);
            result[activity2Id].Average.Should().Be(3.333); // (4 + 4 + 2) / 3 ≈ 3.333
            result[activity2Id].Count.Should().Be(3);
        }

        [Fact]
        public async Task GetVoteStatisticsForSurveyAsync_ShouldReturnEmptyDictionary_WhenNoVotes()
        {
            // Arrange
            var surveyId = Guid.NewGuid();

            // Act
            var result = await _repository.GetVoteStatisticsForSurveyAsync(surveyId);

            // Assert
            result.Should().BeEmpty();
        }
    }
}