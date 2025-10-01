using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Domain.Interfaces;

namespace FunnyActivities.Infrastructure
{
    /// <summary>
    /// Repository implementation for Survey entity operations.
    /// </summary>
    public class SurveyRepository : ISurveyRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SurveyRepository> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SurveyRepository"/> class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        /// <param name="logger">The logger.</param>
        public SurveyRepository(ApplicationDbContext context, ILogger<SurveyRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Gets a survey by its ID.
        /// </summary>
        /// <param name="id">The survey ID.</param>
        /// <returns>The survey if found; otherwise, null.</returns>
        public async Task<Survey> GetByIdAsync(Guid id)
        {
            return await _context.Surveys
                .Include(s => s.CreatedByUser)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        /// <summary>
        /// Gets all surveys.
        /// </summary>
        /// <returns>A collection of all surveys.</returns>
        public async Task<IEnumerable<Survey>> GetAllAsync()
        {
            return await _context.Surveys
                .Include(s => s.CreatedByUser)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Gets surveys by user ID.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>A collection of surveys created by the user.</returns>
        public async Task<IEnumerable<Survey>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Surveys
                .Include(s => s.CreatedByUser)
                .Where(s => s.CreatedByUserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Gets active surveys.
        /// </summary>
        /// <returns>A collection of active surveys.</returns>
        public async Task<IEnumerable<Survey>> GetActiveAsync()
        {
            return await _context.Surveys
                .Include(s => s.CreatedByUser)
                .Where(s => s.IsActive)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Gets surveys by title.
        /// </summary>
        /// <param name="title">The survey title.</param>
        /// <returns>A collection of surveys with the specified title.</returns>
        public async Task<IEnumerable<Survey>> GetByTitleAsync(string title)
        {
            return await _context.Surveys
                .Include(s => s.CreatedByUser)
                .Where(s => s.Title.ToLower().Contains(title.ToLower()))
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Adds a new survey.
        /// </summary>
        /// <param name="survey">The survey to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AddAsync(Survey survey)
        {
            _logger.LogInformation("AddAsync: Adding survey with ID {SurveyId}, Title {Title}", survey.Id, survey.Title);
            _logger.LogInformation("AddAsync: Survey entity state before AddAsync: {State}", _context.Entry(survey).State);

            await _context.Surveys.AddAsync(survey);

            _logger.LogInformation("AddAsync: Survey entity state after AddAsync: {State}", _context.Entry(survey).State);

            await _context.SaveChangesAsync();

            _logger.LogInformation("AddAsync: Survey entity state after SaveChangesAsync: {State}", _context.Entry(survey).State);
            _logger.LogInformation("AddAsync: SurveyActivities count: {Count}", survey.SurveyActivities.Count);
        }

        /// <summary>
        /// Updates an existing survey.
        /// </summary>
        /// <param name="survey">The survey to update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task UpdateAsync(Survey survey)
        {
            _logger.LogInformation("UpdateAsync: Updating survey with ID {SurveyId}, Title {Title}", survey.Id, survey.Title);
            _logger.LogInformation("UpdateAsync: Survey entity state: {State}", _context.Entry(survey).State);
            _logger.LogInformation("UpdateAsync: SurveyActivities count: {Count}", survey.SurveyActivities.Count);

            foreach (var activity in survey.SurveyActivities)
            {
                _logger.LogInformation("UpdateAsync: SurveyActivity ID {Id}, State {State}", activity.Id, _context.Entry(activity).State);
            }

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("UpdateAsync: SaveChangesAsync completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateAsync: SaveChangesAsync failed with exception: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Deletes a survey.
        /// </summary>
        /// <param name="survey">The survey to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DeleteAsync(Survey survey)
        {
            _context.Surveys.Remove(survey);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Checks if a survey exists by its ID.
        /// </summary>
        /// <param name="id">The survey ID.</param>
        /// <returns>True if the survey exists; otherwise, false.</returns>
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Surveys.AnyAsync(s => s.Id == id);
        }

        /// <summary>
        /// Checks if a survey exists by title.
        /// </summary>
        /// <param name="title">The survey title.</param>
        /// <returns>True if the survey exists; otherwise, false.</returns>
        public async Task<bool> ExistsByTitleAsync(string title)
        {
            return await _context.Surveys.AnyAsync(s => s.Title.ToLower() == title.ToLower());
        }

        /// <summary>
        /// Gets surveys with their activities included.
        /// </summary>
        /// <param name="surveyId">The survey ID (optional).</param>
        /// <returns>A collection of surveys with activities.</returns>
        public async Task<IEnumerable<Survey>> GetWithActivitiesAsync(Guid? surveyId = null)
        {
            var query = _context.Surveys
                .Include(s => s.CreatedByUser)
                .Include(s => s.SurveyActivities)
                    .ThenInclude(sa => sa.Activity)
                .AsQueryable();

            if (surveyId.HasValue)
            {
                query = query.Where(s => s.Id == surveyId.Value);
            }

            return await query.OrderByDescending(s => s.CreatedAt).ToListAsync();
        }

        /// <summary>
        /// Gets surveys with their participants included.
        /// </summary>
        /// <param name="surveyId">The survey ID (optional).</param>
        /// <returns>A collection of surveys with participants.</returns>
        public async Task<IEnumerable<Survey>> GetWithParticipantsAsync(Guid? surveyId = null)
        {
            var query = _context.Surveys
                .Include(s => s.CreatedByUser)
                .Include(s => s.SurveyParticipants)
                .AsQueryable();

            if (surveyId.HasValue)
            {
                query = query.Where(s => s.Id == surveyId.Value);
            }

            return await query.OrderByDescending(s => s.CreatedAt).ToListAsync();
        }

        /// <summary>
        /// Gets surveys with their votes included.
        /// </summary>
        /// <param name="surveyId">The survey ID (optional).</param>
        /// <returns>A collection of surveys with votes.</returns>
        public async Task<IEnumerable<Survey>> GetWithVotesAsync(Guid? surveyId = null)
        {
            var query = _context.Surveys
                .Include(s => s.CreatedByUser)
                .Include(s => s.SurveyActivities)
                    .ThenInclude(sa => sa.Activity)
                .Include(s => s.SurveyActivities)
                    .ThenInclude(sa => sa.SurveyVotes)
                        .ThenInclude(sv => sv.SurveyParticipant)
                .Include(s => s.SurveyVotes)
                    .ThenInclude(sv => sv.SurveyParticipant)
                .AsQueryable();

            if (surveyId.HasValue)
            {
                query = query.Where(s => s.Id == surveyId.Value);
            }

            return await query.OrderByDescending(s => s.CreatedAt).ToListAsync();
        }

        /// <summary>
        /// Gets a survey by its share token.
        /// </summary>
        /// <param name="shareToken">The share token.</param>
        /// <returns>The survey if found; otherwise, null.</returns>
        public async Task<Survey> GetByShareTokenAsync(string shareToken)
        {
            return await _context.Surveys
                .Include(s => s.CreatedByUser)
                .FirstOrDefaultAsync(s => s.ShareToken == shareToken);
        }

        /// <summary>
        /// Checks if a survey activity exists by its ID.
        /// </summary>
        /// <param name="activityId">The survey activity ID.</param>
        /// <returns>True if the survey activity exists; otherwise, false.</returns>
        public async Task<bool> SurveyActivityExistsAsync(Guid activityId)
        {
            return await _context.SurveyActivities.AnyAsync(sa => sa.Id == activityId);
        }

        /// <summary>
        /// Gets a survey activity by its ID.
        /// </summary>
        /// <param name="activityId">The survey activity ID.</param>
        /// <returns>The survey activity if found; otherwise, null.</returns>
        public async Task<SurveyActivity> GetSurveyActivityByIdAsync(Guid activityId)
        {
            return await _context.SurveyActivities
                .Include(sa => sa.Survey)
                .Include(sa => sa.Activity)
                .FirstOrDefaultAsync(sa => sa.Id == activityId);
        }

        /// <summary>
        /// Adds a new survey participant.
        /// </summary>
        /// <param name="participant">The participant to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AddParticipantAsync(SurveyParticipant participant)
        {
            _logger.LogInformation("AddParticipantAsync: Adding participant with ID {ParticipantId}, Name {FirstName} {LastName}",
                participant.Id, participant.FirstName, participant.LastName);

            await _context.SurveyParticipants.AddAsync(participant);
            await _context.SaveChangesAsync();

            _logger.LogInformation("AddParticipantAsync: Participant added successfully");
        }
    }
}