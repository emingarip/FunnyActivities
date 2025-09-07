namespace FunnyActivities.Application.DTOs.UserManagement
{
    public class LoginResponse
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public UserDto User { get; set; }
    }
}