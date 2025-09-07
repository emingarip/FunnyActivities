using FluentAssertions;
using FunnyActivities.Domain.ValueObjects;
using Xunit;

namespace FunnyActivities.Domain.UnitTests
{
    public class EmailTests
    {
        [Fact]
        public void Constructor_ShouldCreateEmailWithValidValue()
        {
            // Arrange
            var validEmail = "test@example.com";

            // Act
            var email = new Email(validEmail);

            // Assert
            email.Value.Should().Be(validEmail);
        }

        [Fact]
        public void Constructor_ShouldThrowExceptionForEmptyEmail()
        {
            // Arrange
            var emptyEmail = "";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new Email(emptyEmail));
        }

        [Fact]
        public void Constructor_ShouldThrowExceptionForNullEmail()
        {
            // Arrange
            string nullEmail = null;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new Email(nullEmail));
        }

        [Fact]
        public void Constructor_ShouldThrowExceptionForWhitespaceEmail()
        {
            // Arrange
            var whitespaceEmail = "   ";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new Email(whitespaceEmail));
        }

        [Fact]
        public void Equals_ShouldReturnTrueForSameEmailValues()
        {
            // Arrange
            var email1 = new Email("test@example.com");
            var email2 = new Email("test@example.com");

            // Act & Assert
            email1.Equals(email2).Should().BeTrue();
            (email1 == email2).Should().BeTrue();
        }

        [Fact]
        public void Equals_ShouldReturnFalseForDifferentEmailValues()
        {
            // Arrange
            var email1 = new Email("test@example.com");
            var email2 = new Email("other@example.com");

            // Act & Assert
            email1.Equals(email2).Should().BeFalse();
            (email1 != email2).Should().BeTrue();
        }

        [Fact]
        public void Equals_ShouldReturnFalseForNullComparison()
        {
            // Arrange
            var email = new Email("test@example.com");

            // Act & Assert
            email.Equals(null).Should().BeFalse();
        }

        [Fact]
        public void GetHashCode_ShouldReturnSameHashForSameValues()
        {
            // Arrange
            var email1 = new Email("test@example.com");
            var email2 = new Email("test@example.com");

            // Act & Assert
            email1.GetHashCode().Should().Be(email2.GetHashCode());
        }
    }
}