using System;

namespace FunnyActivities.Domain.Entities
{
    public class UnitConversion
    {
        public Guid Id { get; set; }
        public Guid FromUnitId { get; set; }
        public Guid ToUnitId { get; set; }
        public decimal ConversionFactor { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public UnitOfMeasure FromUnit { get; set; } = null!;
        public UnitOfMeasure ToUnit { get; set; } = null!;
    }
}