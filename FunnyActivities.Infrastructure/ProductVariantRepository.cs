using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Application.Interfaces;

namespace FunnyActivities.Infrastructure
{
    /// <summary>
    /// Repository for ProductVariant entity operations.
    /// </summary>
    public class ProductVariantRepository : IProductVariantRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductVariantRepository"/> class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public ProductVariantRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets a product variant by its ID.
        /// </summary>
        /// <param name="id">The product variant ID.</param>
        /// <returns>The product variant if found; otherwise, null.</returns>
        public async Task<ProductVariant> GetByIdAsync(Guid id)
        {
            return await _context.ProductVariants
                .Include(pv => pv.BaseProduct)
                .Include(pv => pv.UnitOfMeasure)
                .FirstOrDefaultAsync(pv => pv.Id == id);
        }

        /// <summary>
        /// Gets all product variants.
        /// </summary>
        /// <returns>A collection of all product variants.</returns>
        public async Task<IEnumerable<ProductVariant>> GetAllAsync()
        {
            return await _context.ProductVariants
                .Include(pv => pv.BaseProduct)
                .Include(pv => pv.UnitOfMeasure)
                .ToListAsync();
        }

        /// <summary>
        /// Gets product variants by base product ID.
        /// </summary>
        /// <param name="baseProductId">The base product ID.</param>
        /// <returns>A collection of product variants for the specified base product.</returns>
        public async Task<IEnumerable<ProductVariant>> GetByBaseProductIdAsync(Guid baseProductId)
        {
            return await _context.ProductVariants
                .Include(pv => pv.BaseProduct)
                .Include(pv => pv.UnitOfMeasure)
                .Where(pv => pv.BaseProductId == baseProductId)
                .ToListAsync();
        }

        /// <summary>
        /// Adds a new product variant.
        /// </summary>
        /// <param name="productVariant">The product variant to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AddAsync(ProductVariant productVariant)
        {
            await _context.ProductVariants.AddAsync(productVariant);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Updates an existing product variant.
        /// </summary>
        /// <param name="productVariant">The product variant to update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task UpdateAsync(ProductVariant productVariant)
        {
            _context.ProductVariants.Update(productVariant);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes a product variant.
        /// </summary>
        /// <param name="productVariant">The product variant to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DeleteAsync(ProductVariant productVariant)
        {
            _context.ProductVariants.Remove(productVariant);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Checks if a product variant exists by its ID.
        /// </summary>
        /// <param name="id">The product variant ID.</param>
        /// <returns>True if the product variant exists; otherwise, false.</returns>
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.ProductVariants.AnyAsync(pv => pv.Id == id);
        }

        /// <summary>
        /// Gets a product variant by name.
        /// </summary>
        /// <param name="name">The product variant name.</param>
        /// <returns>The product variant if found; otherwise, null.</returns>
        public async Task<ProductVariant> GetByNameAsync(string name)
        {
            return await _context.ProductVariants
                .Include(pv => pv.BaseProduct)
                .Include(pv => pv.UnitOfMeasure)
                .FirstOrDefaultAsync(pv => pv.Name.ToLower() == name.ToLower());
        }

        /// <summary>
        /// Gets a product variant by name and base product ID.
        /// </summary>
        /// <param name="name">The product variant name.</param>
        /// <param name="baseProductId">The base product ID.</param>
        /// <returns>The product variant if found; otherwise, null.</returns>
        public async Task<ProductVariant> GetByNameAndBaseProductAsync(string name, Guid baseProductId)
        {
            return await _context.ProductVariants
                .Include(pv => pv.BaseProduct)
                .Include(pv => pv.UnitOfMeasure)
                .FirstOrDefaultAsync(pv => pv.Name.ToLower() == name.ToLower() && pv.BaseProductId == baseProductId);
        }
    }
}