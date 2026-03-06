using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Infrastructure;

namespace FunnyActivities.Infrastructure.UnitTests.Repositories.ActivityManagement
{
    public class ActivityRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly ActivityRepository _repository;

        public ActivityRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new ActivityRepository(_context);
        }

        [Fact]
        public async Task GetFilteredAsync_ShouldReflectUpdatedPublicStatusImmediately()
        {
            // Arrange
            var category = new ActivityCategory("Test Category", "Description");
            await _context.ActivityCategories.AddAsync(category);

            var activity = Activity.Create("Activity 1", "Description 1", null, null, category.Id, false);
            await _context.Activities.AddAsync(activity);
            await _context.SaveChangesAsync();

            var initialResult = await _repository.GetFilteredAsync(null, null, true, "name", "asc", 1, 12);
            initialResult.Activities.Should().BeEmpty();
            initialResult.TotalCount.Should().Be(0);

            activity.UpdatePublicStatus(true);
            await _context.SaveChangesAsync();

            var updatedResult = await _repository.GetFilteredAsync(null, null, true, "name", "asc", 1, 12);
            updatedResult.Activities.Should().ContainSingle(a => a.Id == activity.Id);
            updatedResult.TotalCount.Should().Be(1);
        }

        [Fact]
        public async Task GetFilteredAsync_ShouldNotCache_WhenSearchTermProvided()
        {
            // Arrange
            var category = new ActivityCategory("Test Category", "Description");
            await _context.ActivityCategories.AddAsync(category);

            var activity = Activity.Create("Activity 1", "Description with search term", null, null, category.Id, true);

            await _context.Activities.AddAsync(activity);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetFilteredAsync("search", null, true, "name", "asc", 1, 10);

            // Assert
            result.Activities.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
        }

        [Fact]
        public async Task GetFilteredAsync_ShouldNotCache_WhenCategoryIdProvided()
        {
            // Arrange
            var category = new ActivityCategory("Test Category", "Description");
            await _context.ActivityCategories.AddAsync(category);

            var activity = Activity.Create("Activity 1", "Description", null, null, category.Id, true);

            await _context.Activities.AddAsync(activity);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetFilteredAsync(null, category.Id, true, "name", "asc", 1, 10);

            // Assert
            result.Activities.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
        }

        [Fact]
        public async Task GetFilteredAsync_ShouldNotCache_WhenNotPublic()
        {
            // Arrange
            var category = new ActivityCategory("Test Category", "Description");
            await _context.ActivityCategories.AddAsync(category);

            var activity = Activity.Create("Activity 1", "Description", null, null, category.Id, false);

            await _context.Activities.AddAsync(activity);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetFilteredAsync(null, null, false, "name", "asc", 1, 10);

            // Assert
            result.Activities.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
        }

        [Fact]
        public async Task GetFilteredAsync_ShouldApplySearchFilter()
        {
            // Arrange
            var category = new ActivityCategory("Test Category", "Description");
            await _context.ActivityCategories.AddAsync(category);

            var activity1 = Activity.Create("Yoga Session", "Relaxing yoga", null, null, category.Id, true);
            var activity2 = Activity.Create("Cooking Class", "Learn to cook", null, null, category.Id, true);

            await _context.Activities.AddRangeAsync(activity1, activity2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetFilteredAsync("yoga", null, false, "name", "asc", 1, 10);

            // Assert
            result.Activities.Should().HaveCount(1);
            result.Activities.First().Name.Should().Be("Yoga Session");
            result.TotalCount.Should().Be(1);
        }

        [Fact]
        public async Task GetFilteredAsync_ShouldApplyCategoryFilter()
        {
            // Arrange
            var category1 = new ActivityCategory("Category 1", "Description");
            var category2 = new ActivityCategory("Category 2", "Description");
            await _context.ActivityCategories.AddRangeAsync(category1, category2);

            var activity1 = Activity.Create("Activity 1", "Description", null, null, category1.Id);
            var activity2 = Activity.Create("Activity 2", "Description", null, null, category2.Id);

            await _context.Activities.AddRangeAsync(activity1, activity2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetFilteredAsync(null, category1.Id, false, "name", "asc", 1, 10);

            // Assert
            result.Activities.Should().HaveCount(1);
            result.Activities.First().ActivityCategoryId.Should().Be(category1.Id);
            result.TotalCount.Should().Be(1);
        }

        [Fact]
        public async Task GetFilteredAsync_ShouldApplyPublicFilter()
        {
            // Arrange
            var category = new ActivityCategory("Test Category", "Description");
            await _context.ActivityCategories.AddAsync(category);

            var activity1 = Activity.Create("Public Activity", "Description", null, null, category.Id, true);
            var activity2 = Activity.Create("Private Activity", "Description", null, null, category.Id, false);

            await _context.Activities.AddRangeAsync(activity1, activity2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetFilteredAsync(null, null, true, "name", "asc", 1, 10);

            // Assert
            result.Activities.Should().HaveCount(1);
            result.Activities.First().Name.Should().Be("Public Activity");
            result.TotalCount.Should().Be(1);
        }

        [Fact]
        public async Task GetFilteredAsync_ShouldApplySorting()
        {
            // Arrange
            var category = new ActivityCategory("Test Category", "Description");
            await _context.ActivityCategories.AddAsync(category);

            var activity1 = Activity.Create("Activity B", "Description", null, null, category.Id);
            var activity2 = Activity.Create("Activity A", "Description", null, null, category.Id);

            await _context.Activities.AddRangeAsync(activity1, activity2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetFilteredAsync(null, null, false, "name", "asc", 1, 10);

            // Assert
            result.Activities.Should().HaveCount(2);
            result.Activities.First().Name.Should().Be("Activity A");
            result.Activities.Last().Name.Should().Be("Activity B");
        }

        [Fact]
        public async Task GetFilteredAsync_ShouldApplyPagination()
        {
            // Arrange
            var category = new ActivityCategory("Test Category", "Description");
            await _context.ActivityCategories.AddAsync(category);

            for (int i = 1; i <= 5; i++)
            {
                var activity = Activity.Create($"Activity {i}", "Description", null, null, category.Id);
                await _context.Activities.AddAsync(activity);
            }
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetFilteredAsync(null, null, false, "name", "asc", 2, 2);

            // Assert
            result.Activities.Should().HaveCount(2);
            result.Activities.First().Name.Should().Be("Activity 3");
            result.TotalCount.Should().Be(5);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
