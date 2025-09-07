using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Application.Interfaces;

namespace FunnyActivities.Infrastructure
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly ApplicationDbContext _context;

        public AuditLogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(AuditLog auditLog)
        {
            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId)
        {
            return await _context.AuditLogs
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task DeleteOldLogsAsync(DateTime beforeDate)
        {
            var oldLogs = _context.AuditLogs.Where(a => a.Timestamp < beforeDate);
            _context.AuditLogs.RemoveRange(oldLogs);
            await _context.SaveChangesAsync();
        }
    }
}