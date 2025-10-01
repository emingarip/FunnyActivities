using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Domain.Interfaces;

namespace FunnyActivities.Infrastructure
{
    /// <summary>
    /// Repository implementation for SurveyVote entity operations.
    /// </summary>
    public class VoteRepository : IVoteRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="VoteRepository"/> class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public VoteRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets a vote by its ID.
        /// </summary>
        /// <param name="id">The vote ID.</param>
        /// <returns>The vote if found; otherwise, null.</returns>
        public async Task<SurveyVote> GetByIdAsync(Guid id)
        {
            return await _context.SurveyVotes
                .Include(v => v.SurveyParticipant)
                .Include(v => v.SurveyActivity)
                    .ThenInclude(sa => sa.Activity)
                .Include(v => v.Survey)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        /// <summary>
        /// Gets all votes for a survey.
        /// </summary>
        /// <param name="surveyId">The survey ID.</param>
        /// <returns>A collection of votes for the survey.</returns>
        public async Task<IEnumerable<SurveyVote>> GetBySurveyIdAsync(Guid surveyId)
        {
            return await _context.SurveyVotes
                .Include(v => v.SurveyParticipant)
                .Include(v => v.SurveyActivity)
                    .ThenInclude(sa => sa.Activity)
                .Where(v => v.SurveyId == surveyId)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Gets all votes for a survey activity.
        /// </summary>
        /// <param name="surveyActivityId">The survey activity ID.</param>
        /// <returns>A collection of votes for the survey activity.</returns>
        public async Task<IEnumerable<SurveyVote>> GetBySurveyActivityIdAsync(Guid surveyActivityId)
        {
            return await _context.SurveyVotes
                .Include(v => v.SurveyParticipant)
                .Include(v => v.SurveyActivity)
                    .ThenInclude(sa => sa.Activity)
                .Where(v => v.SurveyActivityId == surveyActivityId)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Gets all votes by a survey participant.
        /// </summary>
        /// <param name="surveyParticipantId">The survey participant ID.</param>
        /// <returns>A collection of votes by the survey participant.</returns>
        public async Task<IEnumerable<SurveyVote>> GetByParticipantIdAsync(Guid surveyParticipantId)
        {
            return await _context.SurveyVotes
                .Include(v => v.SurveyActivity)
                    .ThenInclude(sa => sa.Activity)
                .Include(v => v.Survey)
                .Where(v => v.SurveyParticipantId == surveyParticipantId)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Gets a specific vote by participant and survey activity.
        /// </summary>
        /// <param name="participantId">The participant ID.</param>
        /// <param name="surveyActivityId">The survey activity ID.</param>
        /// <returns>The vote if found; otherwise, null.</returns>
        public async Task<SurveyVote> GetByParticipantAndActivityAsync(Guid participantId, Guid surveyActivityId)
        {
            return await _context.SurveyVotes
                .Include(v => v.SurveyActivity)
                    .ThenInclude(sa => sa.Activity)
                .Include(v => v.Survey)
                .Include(v => v.SurveyParticipant)
                .FirstOrDefaultAsync(v => v.SurveyParticipantId == participantId && v.SurveyActivityId == surveyActivityId);
        }

        /// <summary>
        /// Gets all votes by survey participant for a specific survey.
        /// </summary>
        /// <param name="surveyParticipantId">The survey participant ID.</param>
        /// <param name="surveyId">The survey ID.</param>
        /// <returns>A collection of votes by the survey participant for the survey.</returns>
        public async Task<IEnumerable<SurveyVote>> GetByParticipantAndSurveyAsync(Guid surveyParticipantId, Guid surveyId)
        {
            return await _context.SurveyVotes
                .Include(v => v.SurveyActivity)
                    .ThenInclude(sa => sa.Activity)
                .Where(v => v.SurveyParticipantId == surveyParticipantId && v.SurveyId == surveyId)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Adds a new vote.
        /// </summary>
        /// <param name="vote">The vote to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AddAsync(SurveyVote vote)
        {
            await _context.SurveyVotes.AddAsync(vote);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Updates an existing vote.
        /// </summary>
        /// <param name="vote">The vote to update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task UpdateAsync(SurveyVote vote)
        {
            _context.SurveyVotes.Update(vote);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes a vote.
        /// </summary>
        /// <param name="vote">The vote to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DeleteAsync(SurveyVote vote)
        {
            _context.SurveyVotes.Remove(vote);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Checks if a vote exists by its ID.
        /// </summary>
        /// <param name="id">The vote ID.</param>
        /// <returns>True if the vote exists; otherwise, false.</returns>
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.SurveyVotes.AnyAsync(v => v.Id == id);
        }

        /// <summary>
        /// Checks if a survey participant has already voted for a survey activity.
        /// </summary>
        /// <param name="surveyParticipantId">The survey participant ID.</param>
        /// <param name="surveyActivityId">The survey activity ID.</param>
        /// <returns>True if the survey participant has voted; otherwise, false.</returns>
        public async Task<bool> HasParticipantVotedAsync(Guid surveyParticipantId, Guid surveyActivityId)
        {
            return await _context.SurveyVotes.AnyAsync(v => v.SurveyParticipantId == surveyParticipantId && v.SurveyActivityId == surveyActivityId);
        }

        /// <summary>
        /// Gets the average vote value for a survey activity.
        /// </summary>
        /// <param name="surveyActivityId">The survey activity ID.</param>
        /// <returns>The average vote value.</returns>
        public async Task<double> GetAverageVoteForActivityAsync(Guid surveyActivityId)
        {
            var votes = await _context.SurveyVotes
                .Where(v => v.SurveyActivityId == surveyActivityId)
                .Select(v => v.VoteValue)
                .ToListAsync();

            return votes.Any() ? votes.Average() : 0.0;
        }

        /// <summary>
        /// Gets the total vote count for a survey activity.
        /// </summary>
        /// <param name="surveyActivityId">The survey activity ID.</param>
        /// <returns>The total vote count.</returns>
        public async Task<int> GetVoteCountForActivityAsync(Guid surveyActivityId)
        {
            return await _context.SurveyVotes.CountAsync(v => v.SurveyActivityId == surveyActivityId);
        }

        /// <summary>
        /// Gets vote statistics for a survey.
        /// </summary>
        /// <param name="surveyId">The survey ID.</param>
        /// <returns>A dictionary with activity IDs as keys and vote statistics as values.</returns>
        public async Task<Dictionary<Guid, (double Average, int Count)>> GetVoteStatisticsForSurveyAsync(Guid surveyId)
        {
            var statistics = await _context.SurveyVotes
                .Where(v => v.SurveyId == surveyId)
                .GroupBy(v => v.SurveyActivityId)
                .Select(g => new
                {
                    ActivityId = g.Key,
                    Average = g.Average(v => v.VoteValue),
                    Count = g.Count()
                })
                .ToListAsync();

            return statistics.ToDictionary(
                s => s.ActivityId,
                s => (s.Average, s.Count)
            );
        }
    }
}