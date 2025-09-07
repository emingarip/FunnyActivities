using FluentAssertions;
using FunnyActivities.Domain.Interfaces;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FunnyActivities.IntegrationTests
{
    public class UserRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserRepository _repository;

        public UserRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new UserRepository(_context);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User(userId, "test@example.com", "hashedpassword", "John", "Doe");
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(userId);
            result.Email.Should().Be("test@example.com");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _repository.GetByIdAsync(nonExistentId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AddAsync_ShouldAddUserToDatabase()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User(userId, "test@example.com", "hashedpassword", "John", "Doe");

            // Act
            await _repository.AddAsync(user);

            // Assert
            var addedUser = await _context.Users.FindAsync(userId);
            addedUser.Should().NotBeNull();
            addedUser.Email.Should().Be("test@example.com");
        }

        [Fact]
        public async Task ExistsByEmailAsync_ShouldReturnTrue_WhenEmailExists()
        {
            // Arrange
            var user = new User(Guid.NewGuid(), "test@example.com", "hashedpassword", "John", "Doe");
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.ExistsByEmailAsync("test@example.com");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByEmailAsync_ShouldReturnFalse_WhenEmailDoesNotExist()
        {
            // Act
            var result = await _repository.ExistsByEmailAsync("nonexistent@example.com");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetByEmailAsync_ShouldReturnUser_WhenEmailExists()
        {
            // Arrange
            var user = new User(Guid.NewGuid(), "test@example.com", "hashedpassword", "John", "Doe");
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByEmailAsync("test@example.com");

            // Assert
            result.Should().NotBeNull();
            result.Email.Should().Be("test@example.com");
        }

        [Fact]
        public async Task GetByEmailAsync_ShouldReturnNull_WhenEmailDoesNotExist()
        {
            // Act
            var result = await _repository.GetByEmailAsync("nonexistent@example.com");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateUserInDatabase()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User(userId, "test@example.com", "hashedpassword", "John", "Doe");
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            user.UpdateProfile("Jane", "Smith", "profile.jpg");

            // Act
            await _repository.UpdateAsync(user);

            // Assert
            var updatedUser = await _context.Users.FindAsync(userId);
            updatedUser.FirstName.Should().Be("Jane");
            updatedUser.LastName.Should().Be("Smith");
            updatedUser.ProfileImageUrl.Should().Be("profile.jpg");
        }

        [Fact]
        public async Task GetByResetTokenAsync_ShouldReturnUser_WhenValidTokenExists()
        {
            // Arrange
            var user = new User(Guid.NewGuid(), "test@example.com", "hashedpassword", "John", "Doe");
            var resetToken = "validtoken";
            var expiry = DateTime.UtcNow.AddHours(1);
            user.SetResetToken(resetToken, expiry);
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByResetTokenAsync(resetToken);

            // Assert
            result.Should().NotBeNull();
            result.Email.Should().Be("test@example.com");
        }

        [Fact]
        public async Task GetByResetTokenAsync_ShouldReturnNull_WhenTokenExpired()
        {
            // Arrange
            var user = new User(Guid.NewGuid(), "test@example.com", "hashedpassword", "John", "Doe");
            var resetToken = "expiredtoken";
            var expiry = DateTime.UtcNow.AddHours(-1); // Expired
            user.SetResetToken(resetToken, expiry);
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByResetTokenAsync(resetToken);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task SearchAsync_ShouldReturnPagedResults_WithCorrectTotalCount()
        {
            // Arrange
            var users = new List<User>
            {
                new User(Guid.NewGuid(), "john@example.com", "hash", "John", "Doe"),
                new User(Guid.NewGuid(), "jane@example.com", "hash", "Jane", "Smith"),
                new User(Guid.NewGuid(), "bob@example.com", "hash", "Bob", "Johnson")
            };
            await _context.Users.AddRangeAsync(users);
            await _context.SaveChangesAsync();

            // Act
            var (results, totalCount) = await _repository.SearchAsync(null, 1, 2, "createdAt", "desc");

            // Assert
            results.Should().HaveCount(2);
            totalCount.Should().Be(3);
        }

        [Fact]
        public async Task SearchAsync_ShouldFilterBySearchTerm()
        {
            // Arrange
            var users = new List<User>
            {
                new User(Guid.NewGuid(), "john@example.com", "hash", "John", "Doe"),
                new User(Guid.NewGuid(), "jane@example.com", "hash", "Jane", "Smith"),
                new User(Guid.NewGuid(), "bob@example.com", "hash", "Bob", "Johnson")
            };
            await _context.Users.AddRangeAsync(users);
            await _context.SaveChangesAsync();

            // Act
            var (results, totalCount) = await _repository.SearchAsync("John", 1, 10, "createdAt", "desc");

            // Assert
            results.Should().HaveCount(2); // John Doe and Bob Johnson
            totalCount.Should().Be(2);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}