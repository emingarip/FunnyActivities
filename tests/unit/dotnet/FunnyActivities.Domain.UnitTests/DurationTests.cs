using System;
using FluentAssertions;
using FunnyActivities.Domain.ValueObjects;
using Xunit;

namespace FunnyActivities.Domain.UnitTests
{
    public class DurationTests
    {
        [Fact]
        public void Create_WithHoursMinutesSeconds_ShouldCreateDurationWithCorrectValue()
        {
            // Arrange
            var hours = 1;
            var minutes = 30;
            var seconds = 45;

            // Act
            var duration = Duration.Create(hours, minutes, seconds);

            // Assert
            duration.Value.Should().Be(new TimeSpan(hours, minutes, seconds));
        }

        [Fact]
        public void Create_WithTimeSpan_ShouldCreateDurationWithCorrectValue()
        {
            // Arrange
            var timeSpan = new TimeSpan(2, 15, 30);

            // Act
            var duration = Duration.Create(timeSpan);

            // Assert
            duration.Value.Should().Be(timeSpan);
        }

        [Theory]
        [InlineData(-1, 0, 0)]
        [InlineData(0, -1, 0)]
        [InlineData(0, 0, -1)]
        public void Create_WithHoursMinutesSeconds_ShouldThrowArgumentException_WhenComponentsAreNegative(int hours, int minutes, int seconds)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => Duration.Create(hours, minutes, seconds));
        }

        [Theory]
        [InlineData(0, 60, 0)]
        [InlineData(0, 0, 60)]
        [InlineData(0, 61, 0)]
        public void Create_WithHoursMinutesSeconds_ShouldThrowArgumentException_WhenMinutesOrSecondsExceed59(int hours, int minutes, int seconds)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => Duration.Create(hours, minutes, seconds));
        }

        [Fact]
        public void Create_WithTimeSpan_ShouldThrowArgumentException_WhenTimeSpanIsZeroOrNegative()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => Duration.Create(TimeSpan.Zero));
            Assert.Throws<ArgumentException>(() => Duration.Create(TimeSpan.FromSeconds(-1)));
        }

        [Fact]
        public void ToString_ShouldReturnFormattedString()
        {
            // Arrange
            var duration = Duration.Create(1, 30, 45);

            // Act
            var result = duration.ToString();

            // Assert
            result.Should().Be("01:30:45");
        }

        [Fact]
        public void Equals_ShouldReturnTrue_WhenValuesAreEqual()
        {
            // Arrange
            var duration1 = Duration.Create(1, 30, 0);
            var duration2 = Duration.Create(new TimeSpan(1, 30, 0));

            // Act & Assert
            duration1.Equals(duration2).Should().BeTrue();
        }

        [Fact]
        public void Equals_ShouldReturnFalse_WhenValuesAreDifferent()
        {
            // Arrange
            var duration1 = Duration.Create(1, 30, 0);
            var duration2 = Duration.Create(2, 0, 0);

            // Act & Assert
            duration1.Equals(duration2).Should().BeFalse();
        }

        [Fact]
        public void GetHashCode_ShouldReturnSameHashCode_WhenValuesAreEqual()
        {
            // Arrange
            var timeSpan = new TimeSpan(1, 30, 0);
            var duration1 = Duration.Create(timeSpan);
            var duration2 = Duration.Create(timeSpan);

            // Act & Assert
            duration1.GetHashCode().Should().Be(duration2.GetHashCode());
        }
    }
}