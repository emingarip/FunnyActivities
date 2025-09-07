using System;
using System.Collections.Generic;

namespace FunnyActivities.Application.DTOs.Shared
{
    /// <summary>
    /// Represents a paged result with items and pagination metadata.
    /// Provides efficient navigation through large datasets by breaking them into manageable pages.
    /// </summary>
    /// <typeparam name="T">The type of items in the result.</typeparam>
    /// <example>
    /// {
    ///   "items": [
    ///     {
    ///       "id": "550e8400-e29b-41d4-a716-446655440000",
    ///       "name": "Steel Pipe 2-inch",
    ///       "category": "Plumbing",
    ///       "stockQuantity": 150.5,
    ///       "unit": "Meters"
    ///     }
    ///   ],
    ///   "page": 1,
    ///   "pageSize": 10,
    ///   "totalCount": 25,
    ///   "totalPages": 3,
    ///   "hasPreviousPage": false,
    ///   "hasNextPage": true
    /// }
    /// </example>
    public class PagedResult<T>
    {
        /// <summary>
        /// Gets or sets the items in the current page.
        /// Collection of items for the requested page.
        /// </summary>
        /// <example>Array of MaterialListDto objects</example>
        public IEnumerable<T> Items { get; set; }

        /// <summary>
        /// Gets or sets the current page number (1-based).
        /// The page number being displayed, starting from 1.
        /// </summary>
        /// <example>1</example>
        public int Page { get; set; }

        /// <summary>
        /// Gets or sets the number of items per page.
        /// Maximum number of items returned in a single page.
        /// </summary>
        /// <example>10</example>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the total number of items across all pages.
        /// Total count of items available in the complete dataset.
        /// </summary>
        /// <example>25</example>
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets the total number of pages.
        /// Calculated based on total count and page size.
        /// </summary>
        /// <example>3</example>
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        /// <summary>
        /// Gets a value indicating whether there is a previous page.
        /// True if current page is greater than 1.
        /// </summary>
        /// <example>false</example>
        public bool HasPreviousPage => Page > 1;

        /// <summary>
        /// Gets a value indicating whether there is a next page.
        /// True if current page is less than total pages.
        /// </summary>
        /// <example>true</example>
        public bool HasNextPage => Page < TotalPages;

        /// <summary>
        /// Initializes a new instance of the <see cref="PagedResult{T}"/> class.
        /// </summary>
        /// <param name="items">The items in the current page.</param>
        /// <param name="page">The current page number.</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="totalCount">The total number of items.</param>
        public PagedResult(IEnumerable<T> items, int page, int pageSize, int totalCount)
        {
            Items = items;
            Page = page;
            PageSize = pageSize;
            TotalCount = totalCount;
        }
    }
}