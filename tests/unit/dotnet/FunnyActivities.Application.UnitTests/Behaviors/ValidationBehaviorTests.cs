using FluentAssertions;
using FluentValidation;
using FunnyActivities.Application.Behaviors;
using FunnyActivities.CrossCuttingConcerns.ErrorHandling;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FunnyActivities.Application.UnitTests.Behaviors
{
    /// <summary>
    /// Unit tests for ValidationBehavior.
    /// Tests validation pipeline behavior, exception throwing, and pipeline continuation.
    /// </summary>
    public class ValidationBehaviorTests
    {
        private readonly Mock<ILogger<ValidationBehavior<ValidationTestRequest, ValidationTestResponse>>> _loggerMock;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationBehaviorTests"/> class.
        /// </summary>
        public ValidationBehaviorTests()
        {
            _loggerMock = new Mock<ILogger<ValidationBehavior<ValidationTestRequest, ValidationTestResponse>>>();
        }

        #region Valid Requests

        [Fact]
        public async Task Handle_ValidRequestWithValidator_ShouldContinuePipeline()
        {
            // Arrange
            var validatorMock = new Mock<IValidator<ValidationTestRequest>>();
            var behavior = new ValidationBehavior<ValidationTestRequest, ValidationTestResponse>(validatorMock.Object);
            var request = new ValidationTestRequest();
            var response = new ValidationTestResponse();
            var nextCalled = false;

            validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            RequestHandlerDelegate<ValidationTestResponse> next = (RequestHandlerDelegate<ValidationTestResponse>)(Delegate)(() => {
                nextCalled = true;
                return Task.FromResult(response);
            });

            // Act
            var result = await behavior.Handle(request, next, CancellationToken.None);

            // Assert
            result.Should().Be(response);
            nextCalled.Should().BeTrue();
            validatorMock.Verify(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidRequestWithoutValidator_ShouldContinuePipeline()
        {
            // Arrange
            var behavior = new ValidationBehavior<ValidationTestRequest, ValidationTestResponse>(null);
            var request = new ValidationTestRequest();
            var response = new ValidationTestResponse();
            var nextCalled = false;

            Task<ValidationTestResponse> Next() => Task.FromResult(response);
            RequestHandlerDelegate<ValidationTestResponse> next = (RequestHandlerDelegate<ValidationTestResponse>)(Delegate)(() => Task.FromResult(response));

            // Act
            var result = await behavior.Handle(request, next, CancellationToken.None);

            // Assert
            result.Should().Be(response);
            nextCalled.Should().BeTrue();
        }

        #endregion

        #region Invalid Requests

        [Fact]
        public async Task Handle_InvalidRequest_ShouldThrowValidationException()
        {
            // Arrange
            var validatorMock = new Mock<IValidator<ValidationTestRequest>>();
            var behavior = new ValidationBehavior<ValidationTestRequest, ValidationTestResponse>(validatorMock.Object);
            var request = new ValidationTestRequest();

            var validationResult = new FluentValidation.Results.ValidationResult();
            validationResult.Errors.Add(new FluentValidation.Results.ValidationFailure("Property", "Error message"));

            validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            RequestHandlerDelegate<ValidationTestResponse> next = (RequestHandlerDelegate<ValidationTestResponse>)(Delegate)(() => Task.FromResult(new ValidationTestResponse()));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<FunnyActivities.CrossCuttingConcerns.ErrorHandling.ValidationException>(() =>
                behavior.Handle(request, next, CancellationToken.None));

            exception.Errors.Should().Contain("Error message");
            validatorMock.Verify(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_InvalidRequestWithMultipleErrors_ShouldThrowValidationExceptionWithAllErrors()
        {
            // Arrange
            var validatorMock = new Mock<IValidator<ValidationTestRequest>>();
            var behavior = new ValidationBehavior<ValidationTestRequest, ValidationTestResponse>(validatorMock.Object);
            var request = new ValidationTestRequest();

            var validationResult = new FluentValidation.Results.ValidationResult();
            validationResult.Errors.Add(new FluentValidation.Results.ValidationFailure("Property1", "Error 1"));
            validationResult.Errors.Add(new FluentValidation.Results.ValidationFailure("Property2", "Error 2"));
            validationResult.Errors.Add(new FluentValidation.Results.ValidationFailure("Property3", "Error 3"));

            validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            RequestHandlerDelegate<ValidationTestResponse> next = (RequestHandlerDelegate<ValidationTestResponse>)(Delegate)(() => Task.FromResult(new ValidationTestResponse()));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<FunnyActivities.CrossCuttingConcerns.ErrorHandling.ValidationException>(() =>
                behavior.Handle(request, next, CancellationToken.None));

            exception.Errors.Should().HaveCount(3);
            exception.Errors.Should().Contain("Error 1");
            exception.Errors.Should().Contain("Error 2");
            exception.Errors.Should().Contain("Error 3");
        }

        [Fact]
        public async Task Handle_InvalidRequest_ShouldNotContinuePipeline()
        {
            // Arrange
            var validatorMock = new Mock<IValidator<ValidationTestRequest>>();
            var behavior = new ValidationBehavior<ValidationTestRequest, ValidationTestResponse>(validatorMock.Object);
            var request = new ValidationTestRequest();
            var nextCalled = false;

            var validationResult = new FluentValidation.Results.ValidationResult();
            validationResult.Errors.Add(new FluentValidation.Results.ValidationFailure("Property", "Error message"));

            validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            RequestHandlerDelegate<ValidationTestResponse> next = (RequestHandlerDelegate<ValidationTestResponse>)(Delegate)(() => Task.FromResult(new ValidationTestResponse()));

            // Act & Assert
            await Assert.ThrowsAsync<FunnyActivities.CrossCuttingConcerns.ErrorHandling.ValidationException>(() =>
                behavior.Handle(request, next, CancellationToken.None));

            nextCalled.Should().BeFalse();
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task Handle_ValidationThrowsException_ShouldPropagateException()
        {
            // Arrange
            var validatorMock = new Mock<IValidator<ValidationTestRequest>>();
            var behavior = new ValidationBehavior<ValidationTestRequest, ValidationTestResponse>(validatorMock.Object);
            var request = new ValidationTestRequest();

            validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Validation failed"));

            RequestHandlerDelegate<ValidationTestResponse> next = (RequestHandlerDelegate<ValidationTestResponse>)(Delegate)(() => Task.FromResult(new ValidationTestResponse()));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                behavior.Handle(request, next, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_CancellationRequested_ShouldPassCancellationToken()
        {
            // Arrange
            var validatorMock = new Mock<IValidator<ValidationTestRequest>>();
            var behavior = new ValidationBehavior<ValidationTestRequest, ValidationTestResponse>(validatorMock.Object);
            var request = new ValidationTestRequest();
            var cts = new CancellationTokenSource();

            validatorMock.Setup(x => x.ValidateAsync(request, cts.Token))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            RequestHandlerDelegate<ValidationTestResponse> next = (RequestHandlerDelegate<ValidationTestResponse>)(Delegate)(() => Task.FromResult(new ValidationTestResponse()));

            // Act
            cts.Cancel();
            await behavior.Handle(request, next, cts.Token);

            // Assert
            validatorMock.Verify(x => x.ValidateAsync(request, cts.Token), Times.Once);
        }

        #endregion
    }

    // Test classes
    public class ValidationTestRequest : IRequest<ValidationTestResponse> { }
    public class ValidationTestResponse { }
}