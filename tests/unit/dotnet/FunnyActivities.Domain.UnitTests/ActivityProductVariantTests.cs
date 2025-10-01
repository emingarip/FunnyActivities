using System;
using FluentAssertions;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Domain.Events;
using Xunit;

namespace FunnyActivities.Domain.UnitTests
{
    public class ActivityProductVariantTests
    {
        [Fact]
        public void Create_ShouldCreateActivityProductVariantWithCorrectProperties()
        {
            // Arrange
            var activityId = Guid.NewGuid();
            var productVariantId = Guid.NewGuid();
            var quantity = 2.5m;
            var unitOfMeasureId = Guid.NewGuid();

            // Act
            var activityProductVariant = ActivityProductVariant.Create(activityId, productVariantId, quantity, unitOfMeasureId);

            // Assert
            activityProductVariant.ActivityId.Should().Be(activityId);
            activityProductVariant.ProductVariantId.Should().Be(productVariantId);
            activityProductVariant.Quantity.Should().Be(quantity);
            activityProductVariant.UnitOfMeasureId.Should().Be(unitOfMeasureId);
            activityProductVariant.DomainEvents.Should().ContainSingle(e => e is ActivityProductVariantCreatedEvent);
        }

        [Fact]
        public void UpdateDetails_ShouldUpdatePropertiesAndAddDomainEvent()
        {
            // Arrange
            var activityProductVariant = ActivityProductVariant.Create(Guid.NewGuid(), Guid.NewGuid(), 1.0m, Guid.NewGuid());
            activityProductVariant.ClearDomainEvents();
            var newQuantity = 3.0m;
            var newUnitOfMeasureId = Guid.NewGuid();

            // Act
            activityProductVariant.UpdateDetails(newQuantity, newUnitOfMeasureId);

            // Assert
            activityProductVariant.Quantity.Should().Be(newQuantity);
            activityProductVariant.UnitOfMeasureId.Should().Be(newUnitOfMeasureId);
            activityProductVariant.DomainEvents.Should().ContainSingle(e => e is ActivityProductVariantUpdatedEvent);
        }

        [Fact]
        public void ClearDomainEvents_ShouldClearAllDomainEvents()
        {
            // Arrange
            var activityProductVariant = ActivityProductVariant.Create(Guid.NewGuid(), Guid.NewGuid(), 1.0m, Guid.NewGuid());
            activityProductVariant.DomainEvents.Should().NotBeEmpty();

            // Act
            activityProductVariant.ClearDomainEvents();

            // Assert
            activityProductVariant.DomainEvents.Should().BeEmpty();
        }
    }
}