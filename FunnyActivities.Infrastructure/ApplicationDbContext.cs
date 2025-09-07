using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Domain.ValueObjects;
using System.Text.Json;
using System;
using System.Linq;
using System.Collections.Generic;

namespace FunnyActivities.Infrastructure
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<BaseProduct> BaseProducts { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<UnitOfMeasure> UnitOfMeasures { get; set; }
        public DbSet<UnitConversion> UnitConversions { get; set; }
        public DbSet<ShoppingCartItem> ShoppingCartItems { get; set; }
        public DbSet<ActivityCategory> ActivityCategories { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<Step> Steps { get; set; }
        public DbSet<ActivityProductVariant> ActivityProductVariants { get; set; }
        // public DbSet<UnitType> UnitTypes { get; set; } // Commented out - UnitType entity not found
        // public DbSet<Unit> Units { get; set; } // Commented out - Unit entity not found

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Value converters for value objects
            var videoUrlConverter = new ValueConverter<VideoUrl?, string>(
                v => v == null ? null : v.Value,
                v => string.IsNullOrEmpty(v) ? null : VideoUrl.Create(v));
            var durationConverter = new ValueConverter<Duration?, TimeSpan>(
                v => v == null ? TimeSpan.Zero : v.Value,
                v => v == TimeSpan.Zero ? null : Duration.Create(v));

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(256);
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.Property(u => u.Role).HasConversion<int>(); // Store enum as int
                // Add unique index on Email
                entity.HasIndex(u => u.Email).IsUnique();
            });

            // Configure AuditLog entity
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Action).IsRequired().HasMaxLength(100);
                entity.Property(a => a.IpAddress).HasMaxLength(45); // IPv6 max length
                entity.Property(a => a.UserAgent).HasMaxLength(500);
                entity.Property(a => a.Details).HasMaxLength(1000);
                // Index on UserId for faster queries
                entity.HasIndex(a => a.UserId);
                // Index on Timestamp for retention policies
                entity.HasIndex(a => a.Timestamp);
            });

            // Configure Image entity
            modelBuilder.Entity<Image>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.FileName).IsRequired().HasMaxLength(255);
                entity.Property(i => i.OriginalFileName).IsRequired().HasMaxLength(255);
                entity.Property(i => i.ContentType).IsRequired().HasMaxLength(100);
                entity.Property(i => i.BucketName).IsRequired().HasMaxLength(100);
                entity.Property(i => i.ObjectKey).IsRequired().HasMaxLength(500);
                entity.Property(i => i.PreSignedUrl).IsRequired().HasMaxLength(1000);
                entity.Property(i => i.ImageType).IsRequired().HasMaxLength(50);
                // Index on UserId for faster queries
                entity.HasIndex(i => i.UserId);
                // Index on ImageType for filtering
                entity.HasIndex(i => i.ImageType);
            });

            // Configure Role entity
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Name).IsRequired().HasMaxLength(50);
                entity.Property(r => r.Description).HasMaxLength(500);
                // Add unique index on Name
                entity.HasIndex(r => r.Name).IsUnique();
            });

            // Configure Category entity
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
                entity.Property(c => c.Description).HasMaxLength(500);
                // Add unique index on Name
                entity.HasIndex(c => c.Name).IsUnique();
            });

            // Configure UnitType entity - Commented out as UnitType entity not found
            // modelBuilder.Entity<UnitType>(entity =>
            // {
            //     entity.HasKey(ut => ut.Id);
            //     entity.Property(ut => ut.Name).IsRequired().HasMaxLength(50);
            //     entity.Property(ut => ut.Description).HasMaxLength(500);
            //     // Add unique index on Name
            //     entity.HasIndex(ut => ut.Name).IsUnique();
            // });

            // Configure Unit entity - Commented out as Unit entity not found
            // modelBuilder.Entity<Unit>(entity =>
            // {
            //     entity.HasKey(u => u.Id);
            //     entity.Property(u => u.Name).IsRequired().HasMaxLength(50);
            //     entity.Property(u => u.Description).HasMaxLength(500);
            //     // Foreign key to UnitType
            //     entity.HasOne(u => u.UnitType)
            //           .WithMany(ut => ut.Units)
            //           .HasForeignKey(u => u.UnitTypeId)
            //           .OnDelete(DeleteBehavior.Restrict);
            //     // Add unique index on Name within UnitType
            //     entity.HasIndex(u => new { u.Name, u.UnitTypeId }).IsUnique();
            // });



            // Configure BaseProduct entity
            modelBuilder.Entity<BaseProduct>(entity =>
            {
                entity.HasKey(bp => bp.Id);
                entity.Property(bp => bp.Name).IsRequired().HasMaxLength(100);
                entity.Property(bp => bp.Description).HasMaxLength(500);
                // Foreign key to Category
                entity.HasOne(bp => bp.Category)
                      .WithMany(c => c.BaseProducts)
                      .HasForeignKey(bp => bp.CategoryId)
                      .OnDelete(DeleteBehavior.SetNull);
                // Add indexes for performance
                entity.HasIndex(bp => bp.Name);
                entity.HasIndex(bp => bp.CategoryId);
                entity.HasIndex(bp => bp.CreatedAt);
            });

            // Configure ProductVariant entity
            modelBuilder.Entity<ProductVariant>(entity =>
            {
                entity.HasKey(pv => pv.Id);
                entity.Property(pv => pv.Name).IsRequired().HasMaxLength(100);
                entity.Property(pv => pv.StockQuantity).HasColumnType("decimal(18,2)");
                entity.Property(pv => pv.UnitValue).HasColumnType("decimal(18,2)");
                entity.Property(pv => pv.UsageNotes).HasMaxLength(300);
                // Foreign key to BaseProduct
                entity.HasOne(pv => pv.BaseProduct)
                      .WithMany(bp => bp.Variants)
                      .HasForeignKey(pv => pv.BaseProductId)
                      .OnDelete(DeleteBehavior.Cascade);
                // Foreign key to UnitOfMeasure
                entity.HasOne(pv => pv.UnitOfMeasure)
                      .WithMany()
                      .HasForeignKey(pv => pv.UnitOfMeasureId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Configure Photos property with proper JSON conversion
                var photosConverter = new ValueConverter<List<string>, string>(
                    v => JsonSerializer.Serialize(v ?? new List<string>(), new JsonSerializerOptions()),
                    v => string.IsNullOrEmpty(v) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(v, new JsonSerializerOptions()) ?? new List<string>());
                entity.Property(pv => pv.Photos).HasConversion(photosConverter);

                // Configure DynamicProperties with proper JSON conversion
                var dynamicPropertiesConverter = new ValueConverter<Dictionary<string, object>, string>(
                    v => JsonSerializer.Serialize(v ?? new Dictionary<string, object>(), new JsonSerializerOptions()),
                    v => string.IsNullOrEmpty(v) ? new Dictionary<string, object>() : JsonSerializer.Deserialize<Dictionary<string, object>>(v, new JsonSerializerOptions()) ?? new Dictionary<string, object>());
                entity.Property(pv => pv.DynamicProperties).HasConversion(dynamicPropertiesConverter);

                // Add indexes for performance
                entity.HasIndex(pv => pv.Name);
                entity.HasIndex(pv => pv.BaseProductId);
                entity.HasIndex(pv => pv.UnitOfMeasureId);
                entity.HasIndex(pv => pv.CreatedAt);
            });

            // Configure UnitOfMeasure entity
            modelBuilder.Entity<UnitOfMeasure>(entity =>
            {
                entity.HasKey(uom => uom.Id);
                entity.Property(uom => uom.Name).IsRequired().HasMaxLength(50);
                entity.Property(uom => uom.Symbol).IsRequired().HasMaxLength(10);
                entity.Property(uom => uom.Type).IsRequired().HasMaxLength(50);
                // Add unique index on Name
                entity.HasIndex(uom => uom.Name).IsUnique();
            });

            // Configure UnitConversion entity
            modelBuilder.Entity<UnitConversion>(entity =>
            {
                entity.HasKey(uc => uc.Id);
                entity.Property(uc => uc.ConversionFactor).HasColumnType("decimal(18,6)");
                // Foreign key to FromUnit
                entity.HasOne(uc => uc.FromUnit)
                      .WithMany()
                      .HasForeignKey(uc => uc.FromUnitId)
                      .OnDelete(DeleteBehavior.Restrict);
                // Foreign key to ToUnit
                entity.HasOne(uc => uc.ToUnit)
                      .WithMany()
                      .HasForeignKey(uc => uc.ToUnitId)
                      .OnDelete(DeleteBehavior.Restrict);
                // Add indexes for performance
                entity.HasIndex(uc => uc.FromUnitId);
                entity.HasIndex(uc => uc.ToUnitId);
            });

            // Configure ShoppingCartItem entity
            modelBuilder.Entity<ShoppingCartItem>(entity =>
            {
                entity.HasKey(sci => sci.Id);
                entity.Property(sci => sci.Quantity).HasColumnType("decimal(18,2)");
                // Foreign key to ProductVariant
                entity.HasOne(sci => sci.ProductVariant)
                      .WithMany()
                      .HasForeignKey(sci => sci.ProductVariantId)
                      .OnDelete(DeleteBehavior.Cascade);
                // Foreign key to User
                entity.HasOne(sci => sci.User)
                      .WithMany()
                      .HasForeignKey(sci => sci.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                // Add indexes for performance
                entity.HasIndex(sci => sci.UserId);
                entity.HasIndex(sci => sci.ProductVariantId);
                entity.HasIndex(sci => sci.AddedAt);
            });

            // Configure ActivityCategory entity
            modelBuilder.Entity<ActivityCategory>(entity =>
            {
                entity.HasKey(ac => ac.Id);
                entity.Property(ac => ac.Name).IsRequired().HasMaxLength(100);
                entity.Property(ac => ac.Description).HasMaxLength(500);
                // Ignore domain events
                entity.Ignore(ac => ac.DomainEvents);
                // Foreign key to Activities
                entity.HasMany(ac => ac.Activities)
                      .WithOne(a => a.ActivityCategory)
                      .HasForeignKey(a => a.ActivityCategoryId)
                      .OnDelete(DeleteBehavior.Cascade);
                // Add unique index on Name
                entity.HasIndex(ac => ac.Name).IsUnique();
                // Index on CreatedAt
                entity.HasIndex(ac => ac.CreatedAt);
            });

            // Configure Activity entity
            modelBuilder.Entity<Activity>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Name).IsRequired().HasMaxLength(100);
                entity.Property(a => a.Description).HasMaxLength(500);
                // Ignore domain events
                entity.Ignore(a => a.DomainEvents);
                // Value converters
                entity.Property(a => a.VideoUrl).HasConversion(videoUrlConverter);
                entity.Property(a => a.Duration).HasConversion(durationConverter);
                // Foreign key to ActivityCategory
                entity.HasOne(a => a.ActivityCategory)
                      .WithMany(ac => ac.Activities)
                      .HasForeignKey(a => a.ActivityCategoryId)
                      .OnDelete(DeleteBehavior.Cascade);
                // Foreign keys to Steps and ActivityProductVariants
                entity.HasMany(a => a.Steps)
                      .WithOne(s => s.Activity)
                      .HasForeignKey(s => s.ActivityId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(a => a.ActivityProductVariants)
                      .WithOne(apv => apv.Activity)
                      .HasForeignKey(apv => apv.ActivityId)
                      .OnDelete(DeleteBehavior.Cascade);
                // Add indexes for performance
                entity.HasIndex(a => a.Name);
                entity.HasIndex(a => a.ActivityCategoryId);
                entity.HasIndex(a => a.CreatedAt);
            });

            // Configure Step entity
            modelBuilder.Entity<Step>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Order);
                entity.Property(s => s.Description).IsRequired().HasMaxLength(500);
                // Ignore domain events
                entity.Ignore(s => s.DomainEvents);
                // Foreign key to Activity
                entity.HasOne(s => s.Activity)
                      .WithMany(a => a.Steps)
                      .HasForeignKey(s => s.ActivityId)
                      .OnDelete(DeleteBehavior.Cascade);
                // Add indexes for performance
                entity.HasIndex(s => s.ActivityId);
                entity.HasIndex(s => new { s.ActivityId, s.Order });
            });

            // Configure ActivityProductVariant entity
            modelBuilder.Entity<ActivityProductVariant>(entity =>
            {
                entity.HasKey(apv => apv.Id);
                entity.Property(apv => apv.Quantity).HasColumnType("decimal(18,2)");
                // Ignore domain events
                entity.Ignore(apv => apv.DomainEvents);
                // Foreign key to Activity
                entity.HasOne(apv => apv.Activity)
                      .WithMany(a => a.ActivityProductVariants)
                      .HasForeignKey(apv => apv.ActivityId)
                      .OnDelete(DeleteBehavior.Cascade);
                // Foreign key to ProductVariant
                entity.HasOne(apv => apv.ProductVariant)
                      .WithMany()
                      .HasForeignKey(apv => apv.ProductVariantId)
                      .OnDelete(DeleteBehavior.Cascade);
                // Foreign key to UnitOfMeasure
                entity.HasOne(apv => apv.UnitOfMeasure)
                      .WithMany()
                      .HasForeignKey(apv => apv.UnitOfMeasureId)
                      .OnDelete(DeleteBehavior.Restrict);
                // Add indexes for performance
                entity.HasIndex(apv => apv.ActivityId);
                entity.HasIndex(apv => apv.ProductVariantId);
                entity.HasIndex(apv => apv.UnitOfMeasureId);
            });
        }
    }
}