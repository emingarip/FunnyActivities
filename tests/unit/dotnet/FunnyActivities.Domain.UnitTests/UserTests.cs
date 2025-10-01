using FluentAssertions;
using FunnyActivities.Domain.Entities;
using Xunit;

namespace FunnyActivities.Domain.UnitTests
{
    public class UserTests
    {
        [Fact]
        public void Constructor_ShouldCreateUserWithValidData()
        {
            // Arrange
            var id = Guid.NewGuid();
            var email = "test@example.com";
            var passwordHash = "hashedpassword";
            var firstName = "John";
            var lastName = "Doe";

            // Act
            var user = new User(id, email, passwordHash, firstName, lastName);

            // Assert
            user.Id.Should().Be(id);
            user.Email.Should().Be(email);
            user.PasswordHash.Should().Be(passwordHash);
            user.FirstName.Should().Be(firstName);
            user.LastName.Should().Be(lastName);
            user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void UpdateProfile_ShouldUpdateProfileFieldsAndUpdatedAt()
        {
            // Arrange
            var user = new User(Guid.NewGuid(), "test@example.com", "hash", "John", "Doe");
            var initialUpdatedAt = user.UpdatedAt;
            Thread.Sleep(10); // Ensure time difference

            // Act
            user.UpdateProfile("Jane", "Smith", "profile.jpg");

            // Assert
            user.FirstName.Should().Be("Jane");
            user.LastName.Should().Be("Smith");
            user.ProfileImageUrl.Should().Be("profile.jpg");
            user.UpdatedAt.Should().BeAfter(initialUpdatedAt);
        }

        [Fact]
        public void SetPasswordHash_ShouldUpdatePasswordHashAndUpdatedAt()
        {
            // Arrange
            var user = new User(Guid.NewGuid(), "test@example.com", "oldhash", "John", "Doe");
            var initialUpdatedAt = user.UpdatedAt;
            Thread.Sleep(10);

            // Act
            user.SetPasswordHash("newhash");

            // Assert
            user.PasswordHash.Should().Be("newhash");
            user.UpdatedAt.Should().BeAfter(initialUpdatedAt);
        }

        [Fact]
        public void SetResetToken_ShouldSetTokenAndExpiryAndUpdateUpdatedAt()
        {
            // Arrange
            var user = new User(Guid.NewGuid(), "test@example.com", "hash", "John", "Doe");
            var initialUpdatedAt = user.UpdatedAt;
            var expiry = DateTime.UtcNow.AddHours(1);
            Thread.Sleep(10);

            // Act
            user.SetResetToken("resettoken", expiry);

            // Assert
            user.ResetToken.Should().Be("resettoken");
            user.ResetTokenExpiry.Should().Be(expiry);
            user.UpdatedAt.Should().BeAfter(initialUpdatedAt);
        }

        [Fact]
        public void ClearResetToken_ShouldClearTokenAndExpiryAndUpdateUpdatedAt()
        {
            // Arrange
            var user = new User(Guid.NewGuid(), "test@example.com", "hash", "John", "Doe");
            user.SetResetToken("resettoken", DateTime.UtcNow.AddHours(1));
            var initialUpdatedAt = user.UpdatedAt;
            Thread.Sleep(10);

            // Act
            user.ClearResetToken();

            // Assert
            user.ResetToken.Should().BeNull();
            user.ResetTokenExpiry.Should().BeNull();
            user.UpdatedAt.Should().BeAfter(initialUpdatedAt);
        }
    }
}