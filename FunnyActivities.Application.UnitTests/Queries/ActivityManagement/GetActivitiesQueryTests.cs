using System;
using FluentAssertions;
using FunnyActivities.Application.Queries.ActivityManagement;
using Xunit;

namespace FunnyActivities.Application.UnitTests.Queries.ActivityManagement
{
    public class GetActivitiesQueryTests
    {
        [Fact]
        public void GetActivitiesQuery_ShouldHaveDefaultValues()
        {
            // Act
            var query = new GetActivitiesQuery();

            // Assert
            query.PageNumber.Should().Be(1);
            query.PageSize.Should().Be(10);
            query.SearchTerm.Should().BeNull();
            query.ActivityCategoryId.Should().BeNull();
            query.SortBy.Should().Be("name");
            query.SortOrder.Should().Be("asc");
        }

        [Fact]
        public void GetActivitiesQuery_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var pageNumber = 2;
            var pageSize = 20;
            var searchTerm = "test";
            var activityCategoryId = Guid.NewGuid();
            var sortBy = "createdAt";
            var sortOrder = "desc";

            // Act
            var query = new GetActivitiesQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                ActivityCategoryId = activityCategoryId,
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            // Assert
            query.PageNumber.Should().Be(pageNumber);
            query.PageSize.Should().Be(pageSize);
            query.SearchTerm.Should().Be(searchTerm);
            query.ActivityCategoryId.Should().Be(activityCategoryId);
            query.SortBy.Should().Be(sortBy);
            query.SortOrder.Should().Be(sortOrder);
        }
    }
}