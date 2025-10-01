using FluentAssertions;
using FunnyActivities.Domain.Entities;
using Xunit;

namespace FunnyActivities.Domain.UnitTests
{
    public class BaseProductTests
    {
        [Fact]
        public void Create_ShouldCreateBaseProductWithCorrectProperties()
        {
            // Arrange
            var name = "Test Base Product";
            var description = "Test Description";
            var categoryId = Guid.NewGuid();

            // Act
            var baseProduct = BaseProduct.Create(name, description, categoryId);

            // Assert
            baseProduct.Should().NotBeNull();
            baseProduct.Id.Should().NotBeEmpty();
            baseProduct.Name.Should().Be(name);
            baseProduct.Description.Should().Be(description);
            baseProduct.CategoryId.Should().Be(categoryId);
            baseProduct.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            baseProduct.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            baseProduct.Variants.Should().NotBeNull();
            baseProduct.Variants.Should().BeEmpty();
        }

        [Fact]
        public void Create_ShouldHandleNullDescriptionAndCategoryId()
        {
            // Arrange
            var name = "Test Base Product";

            // Act
            var baseProduct = BaseProduct.Create(name, null, null);

            // Assert
            baseProduct.Should().NotBeNull();
            baseProduct.Name.Should().Be(name);
            baseProduct.Description.Should().BeNull();
            baseProduct.CategoryId.Should().BeNull();
        }

        [Fact]
        public void UpdateDetails_ShouldUpdateAllPropertiesAndUpdatedAt()
        {
            // Arrange
            var baseProduct = BaseProduct.Create("Original", "Original Desc", Guid.NewGuid());
            var initialUpdatedAt = baseProduct.UpdatedAt;

            var newName = "Updated Name";
            var newDescription = "Updated Description";
            var newCategoryId = Guid.NewGuid();

            // Act
            baseProduct.UpdateDetails(newName, newDescription, newCategoryId);

            // Assert
            baseProduct.Name.Should().Be(newName);
            baseProduct.Description.Should().Be(newDescription);
            baseProduct.CategoryId.Should().Be(newCategoryId);
            baseProduct.UpdatedAt.Should().BeAfter(initialUpdatedAt);
        }

        [Fact]
        public void UpdateDetails_ShouldAllowNullValues()
        {
            // Arrange
            var baseProduct = BaseProduct.Create("Original", "Original Desc", Guid.NewGuid());

            // Act
            baseProduct.UpdateDetails("Updated", null, null);

            // Assert
            baseProduct.Name.Should().Be("Updated");
            baseProduct.Description.Should().BeNull();
            baseProduct.CategoryId.Should().BeNull();
        }

        [Fact]
        public void Constructor_ShouldInitializeVariantsCollection()
        {
            // Arrange
            var id = Guid.NewGuid();
            var name = "Test Product";

            // Act
            var baseProduct = new BaseProduct(id, name, null, null);

            // Assert
            baseProduct.Variants.Should().NotBeNull();
            baseProduct.Variants.Should().BeEmpty();
        }
    }
}