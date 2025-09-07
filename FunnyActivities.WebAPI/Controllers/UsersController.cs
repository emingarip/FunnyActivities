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

namespace FunnyActivities.WebAPI.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : BaseController
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator, ILogger<UsersController> logger)
            : base(logger)
        {
            _mediator = mediator;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
        {
            var command = new RegisterUserCommand
            {
                Email = request.Email,
                Password = request.Password,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Role = request.Role
            };

            await _mediator.Send(command);
            return Ok();
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
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(Guid id)
        {
            var query = new GetUserQuery { UserId = id };
            var user = await _mediator.Send(query);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
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
                return NotFound();
            }

            return Ok(profile);
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
                    return BadRequest(new { message = ex.Message });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "An error occurred while uploading the image.", details = ex.Message });
                }
            }

            var command = new UpdateProfileCommand
            {
                UserId = CurrentUserId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                ProfileImageUrl = profileImageUrl
            };

            var updatedProfile = await _mediator.Send(command);
            return Ok(updatedProfile);
        }

        [HttpPost("request-password-reset")]
        [AllowAnonymous]
        public async Task<IActionResult> RequestPasswordReset([FromBody] RequestPasswordResetRequest request)
        {
            var command = new RequestPasswordResetCommand
            {
                Email = request.Email
            };

            await _mediator.Send(command);
            return Ok(new { message = "If the email exists, a reset link has been sent." });
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
            return Ok(new { message = "Password has been reset successfully." });
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
            return Ok(response);
        }

        [HttpPost("upload-profile-image")]
        [Authorize]
        public async Task<IActionResult> UploadProfileImage([FromForm] UploadProfileImageRequest request)
        {
            // User ID is automatically validated and available through BaseController
            if (request.ImageFile == null || request.ImageFile.Length == 0)
            {
                return BadRequest(new { message = "No image file provided." });
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
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while uploading the image.", details = ex.Message });
            }
        }
    }
}