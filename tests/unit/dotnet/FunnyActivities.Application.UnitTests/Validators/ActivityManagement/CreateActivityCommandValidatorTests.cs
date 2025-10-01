using System;
using FluentAssertions;
using FluentValidation.TestHelper;
using FunnyActivities.Application.Commands.ActivityManagement;
using FunnyActivities.Application.Validators.ActivityManagement;
using Xunit;

namespace FunnyActivities.Application.UnitTests.Validators.ActivityManagement
{
    public class CreateActivityCommandValidatorTests
    {
        private readonly CreateActivityCommandValidator _validator;

        public CreateActivityCommandValidatorTests()
        {
            _validator = new CreateActivityCommandValidator();
        }

        [Fact]
        public void Should_Have_Error_When_Name_Is_Empty()
        {
            // Arrange
            var command = new CreateActivityCommand
            {
                Name = "",
                ActivityCategoryId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name)
                .WithErrorMessage("Activity name is required. Please provide a descriptive name for your activity.");
        }

        [Fact]
        public void Should_Have_Error_When_Name_Is_Null()
        {
            // Arrange
            var command = new CreateActivityCommand
            {
                Name = null,
                ActivityCategoryId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name)
                .WithErrorMessage("Activity name is required. Please provide a descriptive name for your activity.");
        }

        [Fact]
        public void Should_Have_Error_When_Name_Exceeds_MaxLength()
        {
            // Arrange
            var command = new CreateActivityCommand
            {
                Name = new string('A', 201), // 201 characters
                ActivityCategoryId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name)
                .WithErrorMessage("Activity name must be between 1 and 200 characters. Choose a clear, concise name that describes what participants will do.");
        }

        [Fact]
        public void Should_Have_Error_When_Description_Exceeds_MaxLength()
        {
            // Arrange
            var command = new CreateActivityCommand
            {
                Name = "Valid Name",
                Description = new string('A', 1001), // 1001 characters
                ActivityCategoryId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Description)
                .WithErrorMessage("Activity description cannot exceed 1000 characters. Please summarize the activity's purpose, materials needed, and expected outcomes.");
        }

        [Fact]
        public void Should_Have_Error_When_VideoUrl_Is_Invalid()
        {
            // Arrange
            var command = new CreateActivityCommand
            {
                Name = "Valid Name",
                VideoUrl = "invalid-url",
                ActivityCategoryId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.VideoUrl)
                .WithErrorMessage("Invalid video URL format. Please provide a valid video URL (e.g., https://youtube.com/watch?v=...) or leave empty if no video is needed.");
        }

        [Theory]
        [InlineData("https://youtube.com/watch?v=abc123")]
        [InlineData("https://vimeo.com/123456")]
        [InlineData("data:video/mp4;base64,abc123")]
        [InlineData("blob:http://example.com/abc123")]
        [InlineData("file:///path/to/video.mp4")]
        [InlineData("rtmp://stream.example.com/live")]
        [InlineData("rtsp://stream.example.com/live")]
        [InlineData("mms://stream.example.com/live")]
        public void Should_Not_Have_Error_When_VideoUrl_Is_Valid(string validUrl)
        {
            // Arrange
            var command = new CreateActivityCommand
            {
                Name = "Valid Name",
                VideoUrl = validUrl,
                ActivityCategoryId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.VideoUrl);
        }

        [Fact]
        public void Should_Not_Have_Error_When_VideoUrl_Is_Valid_MinIO_ObjectKey()
        {
            // Arrange
            var command = new CreateActivityCommand
            {
                Name = "Valid Name",
                VideoUrl = "videos/activity-123/video.mp4", // MinIO object key
                ActivityCategoryId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.VideoUrl);
        }

        [Fact]
        public void Should_Have_Error_When_DurationHours_Is_Less_Than_Zero()
        {
            // Arrange
            var command = new CreateActivityCommand
            {
                Name = "Valid Name",
                DurationHours = -1,
                ActivityCategoryId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.DurationHours)
                .WithErrorMessage("Hours must be between 0 and 23. For longer activities, consider breaking them into multiple sessions.");
        }

        [Fact]
        public void Should_Have_Error_When_DurationHours_Exceeds_Max()
        {
            // Arrange
            var command = new CreateActivityCommand
            {
                Name = "Valid Name",
                DurationHours = 24,
                ActivityCategoryId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.DurationHours)
                .WithErrorMessage("Hours must be between 0 and 23. For longer activities, consider breaking them into multiple sessions.");
        }

        [Fact]
        public void Should_Have_Error_When_DurationMinutes_Is_Less_Than_Zero()
        {
            // Arrange
            var command = new CreateActivityCommand
            {
                Name = "Valid Name",
                DurationMinutes = -1,
                ActivityCategoryId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.DurationMinutes)
                .WithErrorMessage("Minutes must be between 0 and 59. Use this to specify additional time beyond the hours.");
        }

        [Fact]
        public void Should_Have_Error_When_DurationMinutes_Exceeds_Max()
        {
            // Arrange
            var command = new CreateActivityCommand
            {
                Name = "Valid Name",
                DurationMinutes = 60,
                ActivityCategoryId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.DurationMinutes)
                .WithErrorMessage("Minutes must be between 0 and 59. Use this to specify additional time beyond the hours.");
        }

        [Fact]
        public void Should_Have_Error_When_DurationSeconds_Is_Less_Than_Zero()
        {
            // Arrange
            var command = new CreateActivityCommand
            {
                Name = "Valid Name",
                DurationSeconds = -1,
                ActivityCategoryId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.DurationSeconds)
                .WithErrorMessage("Seconds must be between 0 and 59. This field is typically used for very short activities or precise timing requirements.");
        }

        [Fact]
        public void Should_Have_Error_When_DurationSeconds_Exceeds_Max()
        {
            // Arrange
            var command = new CreateActivityCommand
            {
                Name = "Valid Name",
                DurationSeconds = 60,
                ActivityCategoryId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.DurationSeconds)
                .WithErrorMessage("Seconds must be between 0 and 59. This field is typically used for very short activities or precise timing requirements.");
        }

        [Fact]
        public void Should_Have_Error_When_ActivityCategoryId_Is_Empty()
        {
            // Arrange
            var command = new CreateActivityCommand
            {
                Name = "Valid Name",
                ActivityCategoryId = Guid.Empty,
                UserId = Guid.NewGuid()
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ActivityCategoryId)
                .WithErrorMessage("Activity category is required. Please select an appropriate category that best describes your activity type.");
        }

        [Fact]
        public void Should_Have_Error_When_UserId_Is_Empty()
        {
            // Arrange
            var command = new CreateActivityCommand
            {
                Name = "Valid Name",
                ActivityCategoryId = Guid.NewGuid(),
                UserId = Guid.Empty
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.UserId)
                .WithErrorMessage("User authentication is required. Please ensure you are logged in before creating activities.");
        }

        [Fact]
        public void Should_Not_Have_Error_When_Command_Is_Valid()
        {
            // Arrange
            var command = new CreateActivityCommand
            {
                Name = "Valid Activity Name",
                Description = "Valid description",
                VideoUrl = "https://youtube.com/watch?v=abc123",
                DurationHours = 1,
                DurationMinutes = 30,
                DurationSeconds = 45,
                ActivityCategoryId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Not_Have_Error_When_Description_Is_Null()
        {
            // Arrange
            var command = new CreateActivityCommand
            {
                Name = "Valid Name",
                Description = null,
                ActivityCategoryId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Not_Have_Error_When_VideoUrl_Is_Null()
        {
            // Arrange
            var command = new CreateActivityCommand
            {
                Name = "Valid Name",
                VideoUrl = null,
                ActivityCategoryId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Not_Have_Error_When_DurationFields_Are_Null()
        {
            // Arrange
            var command = new CreateActivityCommand
            {
                Name = "Valid Name",
                DurationHours = null,
                DurationMinutes = null,
                DurationSeconds = null,
                ActivityCategoryId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}