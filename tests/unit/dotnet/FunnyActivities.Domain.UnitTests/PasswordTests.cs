using FluentAssertions;
using FunnyActivities.Domain.ValueObjects;
using Xunit;

namespace FunnyActivities.Domain.UnitTests
{
    public class PasswordTests
    {
        [Fact]
        public void Constructor_ShouldCreatePasswordWithValidValue()
        {
            // Arrange
            var validPassword = "validpassword123";

            // Act
            var password = new Password(validPassword);

            // Assert
            password.Value.Should().Be(validPassword);
        }

        [Fact]
        public void Constructor_ShouldThrowExceptionForShortPassword()
        {
            // Arrange
            var shortPassword = "12345";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new Password(shortPassword));
        }

        [Fact]
        public void Constructor_ShouldThrowExceptionForEmptyPassword()
        {
            // Arrange
            var emptyPassword = "";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new Password(emptyPassword));
        }

        [Fact]
        public void Constructor_ShouldThrowExceptionForNullPassword()
        {
            // Arrange
            string nullPassword = null;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new Password(nullPassword));
        }

        [Fact]
        public void Constructor_ShouldThrowExceptionForWhitespacePassword()
        {
            // Arrange
            var whitespacePassword = "   ";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new Password(whitespacePassword));
        }

        [Fact]
        public void Equals_ShouldReturnTrueForSamePasswordValues()
        {
            // Arrange
            var password1 = new Password("password123");
            var password2 = new Password("password123");

            // Act & Assert
            password1.Equals(password2).Should().BeTrue();
            (password1 == password2).Should().BeTrue();
        }

        [Fact]
        public void Equals_ShouldReturnFalseForDifferentPasswordValues()
        {
            // Arrange
            var password1 = new Password("password123");
            var password2 = new Password("different456");

            // Act & Assert
            password1.Equals(password2).Should().BeFalse();
            (password1 != password2).Should().BeTrue();
        }

        [Fact]
        public void Equals_ShouldReturnFalseForNullComparison()
        {
            // Arrange
            var password = new Password("password123");

            // Act & Assert
            password.Equals(null).Should().BeFalse();
        }

        [Fact]
        public void GetHashCode_ShouldReturnSameHashForSameValues()
        {
            // Arrange
            var password1 = new Password("password123");
            var password2 = new Password("password123");

            // Act & Assert
            password1.GetHashCode().Should().Be(password2.GetHashCode());
        }
    }
}