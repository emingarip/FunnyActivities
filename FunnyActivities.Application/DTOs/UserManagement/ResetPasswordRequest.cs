namespace FunnyActivities.Application.DTOs.UserManagement
{
    public class ResetPasswordRequest
    {
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }
}