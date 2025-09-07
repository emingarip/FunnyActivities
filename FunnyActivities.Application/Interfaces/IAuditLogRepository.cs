using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Application.Interfaces
{
    public interface IAuditLogRepository
    {
        Task AddAsync(AuditLog auditLog);
        Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId);
        Task DeleteOldLogsAsync(DateTime beforeDate);
    }
}