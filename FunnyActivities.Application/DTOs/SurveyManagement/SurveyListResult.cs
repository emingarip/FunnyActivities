using System.Text.Json.Serialization;

namespace FunnyActivities.Application.DTOs.SurveyManagement
{
    /// <summary>
    /// Result for survey list queries with pagination information.
    /// </summary>
    public class SurveyListResult
    {
        /// <summary>
        /// The list of surveys.
        /// </summary>
        [JsonPropertyName("surveys")]
        public List<SurveyListDto> Surveys { get; set; } = new();

        /// <summary>
        /// The total number of surveys across all pages.
        /// </summary>
        [JsonPropertyName("totalCount")]
        public int TotalCount { get; set; }

        /// <summary>
        /// The current page number (1-based).
        /// </summary>
        [JsonPropertyName("page")]
        public int Page { get; set; }

        /// <summary>
        /// The number of items per page.
        /// </summary>
        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; }

        /// <summary>
        /// The total number of pages.
        /// </summary>
        [JsonPropertyName("totalPages")]
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}