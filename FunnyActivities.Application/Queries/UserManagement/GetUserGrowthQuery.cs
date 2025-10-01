using MediatR;
using System.Collections.Generic;
using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Application.Queries.UserManagement;

public class GetUserGrowthQuery : IRequest<List<UserGrowthDataPoint>>
{
    public string Period { get; set; } = "weekly"; // weekly, monthly, quarterly
    public int Days { get; set; } = 30; // fallback for custom periods
}