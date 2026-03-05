using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FunnyActivities.Application.Commands.UserManagement;
using FunnyActivities.Application.Queries.UserManagement;
using FunnyActivities.Application.DTOs.UserManagement;
using FunnyActivities.Application.DTOs.Shared;
using Microsoft.AspNetCore.Authorization;
using FunnyActivities.WebAPI.Controllers.Base;
using System.Security.Claims;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Configuration;

namespace FunnyActivities.WebAPI.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly IStringLocalizer<UsersController> _localizer;
        private readonly IConfiguration _configuration;

        public UsersController(IMediator mediator, ILogger<UsersController> logger, IStringLocalizer<UsersController> localizer, IConfiguration configuration)
            : base(logger)
        {
            _mediator = mediator;
            _localizer = localizer;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
        {
            var command = new RegisterUserCommand
            {
                Email = request.Email,
                Password = request.Password,
                FirstName = request.FirstName,
                LastName = request.LastName
            };

            await _mediator.Send(command);
            return this.ApiSuccess<object>(_localizer["UserRegistered"]);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var command = new LoginUserCommand
            {
                Email = request.Email,
                Password = request.Password
            };

            var response = await _mediator.Send(command);
            return this.ApiSuccess(response, _localizer["LoginSuccessful"]);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(Guid id)
        {
            var query = new GetUserQuery { UserId = id };
            var user = await _mediator.Send(query);

            if (user == null)
            {
                return this.ApiError(_localizer["UserNotFound"], "NotFound", 404);
            }

            return this.ApiSuccess(user, _localizer["UserRetrieved"]);
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            // User ID is automatically validated and available through BaseController
            var query = new GetProfileQuery { UserId = CurrentUserId };
            var profile = await _mediator.Send(query);

            if (profile == null)
            {
                return this.ApiError(_localizer["ProfileNotFound"], "NotFound", 404);
            }

            return this.ApiSuccess(profile, _localizer["ProfileRetrieved"]);
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileWithImageRequest request)
        {
            // User ID is automatically validated and available through BaseController
            string profileImageUrl = null;

            // Eğer yeni resim yüklenmişse MinIO'ya yükle
            if (request.ProfileImage != null && request.ProfileImage.Length > 0)
            {
                try
                {
                    using var memoryStream = new MemoryStream();
                    await request.ProfileImage.CopyToAsync(memoryStream);
                    var imageData = memoryStream.ToArray();

                    var uploadCommand = new UploadProfileImageCommand
                    {
                        UserId = CurrentUserId,
                        ImageData = imageData,
                        FileName = request.ProfileImage.FileName,
                        ContentType = request.ProfileImage.ContentType
                    };

                    var uploadResponse = await _mediator.Send(uploadCommand);
                    profileImageUrl = uploadResponse.ProfileImageUrl; // MinIO URL'sini kullan
                }
                catch (ArgumentException ex)
                {
                    return this.ApiError(string.Format(_localizer["ProfileImageValidationError"], ex.Message), "ValidationError", 400);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading profile image for user {UserId}", CurrentUserId);
                    return this.ApiError(_localizer["ProfileImageUploadUnexpected"], "InternalError", 500);
                }
            }

            var command = new UpdateProfileCommand
            {
                UserId = CurrentUserId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                ProfileImageUrl = profileImageUrl
            };

            try
            {
                var updatedProfile = await _mediator.Send(command);
                return this.ApiSuccess(updatedProfile, _localizer["ProfileUpdated"]);
            }
            catch (ArgumentException ex)
            {
                return this.ApiError(string.Format(_localizer["ProfileUpdateValidationError"], ex.Message), "ValidationError", 400);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for user {UserId}", CurrentUserId);
                return this.ApiError(_localizer["ProfileUpdateUnexpected"], "InternalError", 500);
            }
        }

        [HttpPost("request-password-reset")]
        [AllowAnonymous]
        public async Task<IActionResult> RequestPasswordReset([FromBody] RequestPasswordResetRequest request)
        {
            var command = new RequestPasswordResetCommand
            {
                Email = request.Email,
                FrontendUrl = _configuration["FrontendUrl"] ?? $"{Request.Scheme}://{Request.Host.Value}"
            };

            await _mediator.Send(command);
            return this.ApiSuccess<object>(_localizer["PasswordResetRequested"]);
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var command = new ResetPasswordCommand
            {
                Token = request.Token,
                NewPassword = request.NewPassword
            };

            await _mediator.Send(command);
            return this.ApiSuccess<object>(_localizer["PasswordResetSuccess"]);
        }

        [HttpGet("search")]
        [Authorize(Policy = "CanManageUsers")]
        public async Task<IActionResult> SearchUsers([FromQuery] string searchTerm, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string sortBy = "CreatedAt", [FromQuery] string sortOrder = "desc")
        {
            var query = new SearchUsersQuery
            {
                SearchTerm = searchTerm,
                Page = page,
                PageSize = pageSize,
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            var response = await _mediator.Send(query);
            return this.ApiSuccess(response, _localizer["UsersSearched"]);
        }

        [HttpGet("admin/count")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> GetUserCount()
        {
            try
            {
                var count = await _mediator.Send(new GetUserCountQuery());
                return this.ApiSuccess(new { totalUsers = count }, _localizer["UserCountRetrieved"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user count");
                return this.ApiError(_localizer["UserCountError"], "InternalError", 500);
            }
        }

        [HttpGet("admin/online-count")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> GetOnlineUsersCount([FromQuery] int thresholdMinutes = 30)
        {
            try
            {
                var query = new GetOnlineUsersCountQuery
                {
                    OnlineThreshold = TimeSpan.FromMinutes(thresholdMinutes)
                };
                var count = await _mediator.Send(query);
                return this.ApiSuccess(new { onlineUsers = count }, _localizer["OnlineUserCountRetrieved"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving online users count");
                return this.ApiError(_localizer["OnlineUserCountError"], "InternalError", 500);
            }
        }

        [HttpGet("admin/growth")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> GetUserGrowth([FromQuery] string period = "weekly", [FromQuery] int days = 30)
        {
            try
            {
                var query = new GetUserGrowthQuery
                {
                    Period = period,
                    Days = days
                };
                var data = await _mediator.Send(query);
                return this.ApiSuccess(new { data }, _localizer["UserGrowthRetrieved"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user growth data");
                return this.ApiError(_localizer["UserGrowthError"], "InternalError", 500);
            }
        }

        [HttpPost("upload-profile-image")]
        [Authorize]
        public async Task<IActionResult> UploadProfileImage([FromForm] UploadProfileImageRequest request)
        {
            // User ID is automatically validated and available through BaseController
            if (request.ImageFile == null || request.ImageFile.Length == 0)
            {
                return this.ApiError(_localizer["ProfileImageRequired"], "ValidationError", 400);
            }

            try
            {
                using var memoryStream = new MemoryStream();
                await request.ImageFile.CopyToAsync(memoryStream);
                var imageData = memoryStream.ToArray();

                var command = new UploadProfileImageCommand
                {
                    UserId = CurrentUserId,
                    ImageData = imageData,
                    FileName = request.ImageFile.FileName,
                    ContentType = request.ImageFile.ContentType
                };

                var response = await _mediator.Send(command);
                return this.ApiSuccess(response, _localizer["ProfileImageUploaded"]);
            }
            catch (ArgumentException ex)
            {
                return this.ApiError(string.Format(_localizer["ProfileImageValidationError"], ex.Message), "ValidationError", 400);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while uploading profile image for user {UserId}", CurrentUserId);
                return this.ApiError(_localizer["ProfileImageUploadUnexpected"], "InternalError", 500);
            }
        }
    }
}
