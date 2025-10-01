using MediatR;
using Microsoft.Extensions.Logging;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Domain.Interfaces;
using FunnyActivities.Application.Queries.SurveyManagement;
using FunnyActivities.Application.DTOs.SurveyManagement;

namespace FunnyActivities.Application.Handlers.SurveyManagement
{
    /// <summary>
    /// Handler for getting a list of surveys.
    /// </summary>
    public class GetSurveysQueryHandler : IRequestHandler<GetSurveysQuery, SurveyListResult>
    {
        private readonly ISurveyRepository _surveyRepository;
        private readonly ILogger<GetSurveysQueryHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetSurveysQueryHandler"/> class.
        /// </summary>
        /// <param name="surveyRepository">The survey repository.</param>
        /// <param name="logger">The logger.</param>
        public GetSurveysQueryHandler(
            ISurveyRepository surveyRepository,
            ILogger<GetSurveysQueryHandler> logger)
        {
            _surveyRepository = surveyRepository;
            _logger = logger;
        }

        /// <summary>
        /// Handles the get surveys query.
        /// </summary>
        /// <param name="request">The get surveys query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A survey list result with pagination.</returns>
        public async Task<SurveyListResult> Handle(GetSurveysQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting surveys with filters - UserId: {UserId}, IsActive: {IsActive}, SearchTerm: {SearchTerm}, PageNumber: {PageNumber}, PageSize: {PageSize}",
                request.UserId, request.IsActive, request.SearchTerm, request.PageNumber, request.PageSize);

            var surveys = await _surveyRepository.GetAllAsync();
            _logger.LogInformation("Retrieved {Count} surveys from repository", surveys.Count());

            var filteredSurveys = surveys.AsEnumerable();

            // Apply filters
            if (request.UserId.HasValue)
            {
                filteredSurveys = filteredSurveys.Where(s => s.CreatedByUserId == request.UserId.Value);
            }

            if (request.IsActive.HasValue)
            {
                filteredSurveys = filteredSurveys.Where(s => s.IsActive == request.IsActive.Value);
            }

            if (request.IsCurrentlyActive.HasValue)
            {
                filteredSurveys = filteredSurveys.Where(s => s.IsCurrentlyActive() == request.IsCurrentlyActive.Value);
            }

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                filteredSurveys = filteredSurveys.Where(s =>
                    s.Title.ToLower().Contains(searchTerm) ||
                    (s.Description != null && s.Description.ToLower().Contains(searchTerm)));
            }

            _logger.LogInformation("After filtering: {Count} surveys", filteredSurveys.Count());

            // Apply sorting
            filteredSurveys = request.SortBy?.ToLower() switch
            {
                "title" => request.SortDescending
                    ? filteredSurveys.OrderByDescending(s => s.Title)
                    : filteredSurveys.OrderBy(s => s.Title),
                "createdat" => request.SortDescending
                    ? filteredSurveys.OrderByDescending(s => s.CreatedAt)
                    : filteredSurveys.OrderBy(s => s.CreatedAt),
                "startdate" => request.SortDescending
                    ? filteredSurveys.OrderByDescending(s => s.StartDate)
                    : filteredSurveys.OrderBy(s => s.StartDate),
                "participantcount" => request.SortDescending
                    ? filteredSurveys.OrderByDescending(s => s.GetParticipantCount())
                    : filteredSurveys.OrderBy(s => s.GetParticipantCount()),
                _ => request.SortDescending
                    ? filteredSurveys.OrderByDescending(s => s.CreatedAt)
                    : filteredSurveys.OrderBy(s => s.CreatedAt)
            };

            // Apply pagination
            var totalCount = filteredSurveys.Count();
            _logger.LogInformation("Total count after filtering: {TotalCount}", totalCount);

            var paginatedSurveys = filteredSurveys
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            _logger.LogInformation("Paginated surveys count: {Count}", paginatedSurveys.Count);

            var surveyDtos = paginatedSurveys.Select(MapToListDto).ToList();

            _logger.LogInformation("Returning {Count} survey DTOs", surveyDtos.Count);

            return new SurveyListResult
            {
                Surveys = surveyDtos,
                TotalCount = totalCount,
                Page = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        private SurveyListDto MapToListDto(Survey survey)
        {
            return new SurveyListDto
            {
                Id = survey.Id,
                Title = survey.Title,
                Description = survey.Description,
                CreatedByUserId = survey.CreatedByUserId,
                IsActive = survey.IsActive,
                StartDate = survey.StartDate,
                EndDate = survey.EndDate,
                MaxParticipants = survey.MaxParticipants,
                ParticipantCount = survey.GetParticipantCount(),
                CreatedAt = survey.CreatedAt,
                UpdatedAt = survey.UpdatedAt,
                ActivityCount = survey.SurveyActivities?.Count ?? 0,
                IsCurrentlyActive = survey.IsCurrentlyActive(),
                HasReachedMaxParticipants = survey.HasReachedMaxParticipants(),
                ShareToken = survey.ShareToken
            };
        }
    }
}