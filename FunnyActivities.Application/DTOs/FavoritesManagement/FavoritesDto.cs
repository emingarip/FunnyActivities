using System;

namespace FunnyActivities.Application.DTOs.FavoritesManagement
{
    public class FavoritesDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ActivityId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}