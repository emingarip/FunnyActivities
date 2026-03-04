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
            result.ShouldHaveValidationErrorFor(x => x.Name);
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
            result.ShouldHaveValidationErrorFor(x => x.Name);
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
            result.ShouldHaveValidationErrorFor(x => x.Name);
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
            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void Should_Have_Error_When_VideoUrl_Is_Invalid()
        {
            // Arrange
            var command = new CreateActivityCommand
            {
                Name = "Valid Name",
                VideoUrl = "ftp://invalid-video-source.example/video.mp4",
                ActivityCategoryId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.VideoUrl);
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
        public void Should_Have_Error_When_IntroVideoUrl_Is_Invalid()
        {
            // Arrange
            var command = new CreateActivityCommand
            {
                Name = "Valid Name",
                IntroVideoUrl = "mailto:intro@example.com",
                ActivityCategoryId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.IntroVideoUrl);
        }

        [Theory]
        [InlineData("https://cdn.example.com/intro.mp4")]
        [InlineData("videos/activity-123/intro/video.mp4")]
        public void Should_Not_Have_Error_When_IntroVideoUrl_Is_Valid(string introUrl)
        {
            // Arrange
            var command = new CreateActivityCommand
            {
                Name = "Valid Name",
                IntroVideoUrl = introUrl,
                ActivityCategoryId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.IntroVideoUrl);
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
            result.ShouldHaveValidationErrorFor(x => x.DurationHours);
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
            result.ShouldHaveValidationErrorFor(x => x.DurationHours);
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
            result.ShouldHaveValidationErrorFor(x => x.DurationMinutes);
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
            result.ShouldHaveValidationErrorFor(x => x.DurationMinutes);
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
            result.ShouldHaveValidationErrorFor(x => x.DurationSeconds);
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
            result.ShouldHaveValidationErrorFor(x => x.DurationSeconds);
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
            result.ShouldHaveValidationErrorFor(x => x.ActivityCategoryId);
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
            result.ShouldHaveValidationErrorFor(x => x.UserId);
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

