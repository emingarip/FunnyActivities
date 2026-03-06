using System.ComponentModel.DataAnnotations;

namespace FunnyActivities.Application.DTOs.UserManagement
{
    public class GoogleLoginRequest
    {
        [Required]
        public string IdToken { get; set; } = string.Empty;
    }
}
