using FluentAssertions;
using FluentValidation;
using FunnyActivities.Application.Behaviors;
using CustomValidationException = FunnyActivities.CrossCuttingConcerns.ErrorHandling.ValidationException;
using MediatR;
using Moq;
using Xunit;

namespace FunnyActivities.Application.UnitTests.Behaviors
{
    public class ValidationBehaviorTests
    {
        [Fact]
        public async Task Handle_ValidRequestWithValidator_ShouldContinuePipeline()
        {
            var validatorMock = new Mock<IValidator<ValidationTestRequest>>();
            validatorMock
                .Setup(x => x.ValidateAsync(It.IsAny<ValidationTestRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            var behavior = new ValidationBehavior<ValidationTestRequest, ValidationTestResponse>(validatorMock.Object);
            var nextCalled = false;
            RequestHandlerDelegate<ValidationTestResponse> next = _ =>
            {
                nextCalled = true;
                return Task.FromResult(new ValidationTestResponse());
            };

            var result = await behavior.Handle(new ValidationTestRequest(), next, CancellationToken.None);

            result.Should().NotBeNull();
            nextCalled.Should().BeTrue();
            validatorMock.Verify(x => x.ValidateAsync(It.IsAny<ValidationTestRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidRequestWithoutValidator_ShouldContinuePipeline()
        {
            var behavior = new ValidationBehavior<ValidationTestRequest, ValidationTestResponse>(null);
            var nextCalled = false;
            RequestHandlerDelegate<ValidationTestResponse> next = _ =>
            {
                nextCalled = true;
                return Task.FromResult(new ValidationTestResponse());
            };

            var result = await behavior.Handle(new ValidationTestRequest(), next, CancellationToken.None);

            result.Should().NotBeNull();
            nextCalled.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_InvalidRequest_ShouldThrowValidationException()
        {
            var validatorMock = new Mock<IValidator<ValidationTestRequest>>();
            var validationResult = new FluentValidation.Results.ValidationResult(new[]
            {
                new FluentValidation.Results.ValidationFailure("Field1", "Error 1"),
                new FluentValidation.Results.ValidationFailure("Field2", "Error 2")
            });

            validatorMock
                .Setup(x => x.ValidateAsync(It.IsAny<ValidationTestRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            var behavior = new ValidationBehavior<ValidationTestRequest, ValidationTestResponse>(validatorMock.Object);
            var nextCalled = false;
            RequestHandlerDelegate<ValidationTestResponse> next = _ =>
            {
                nextCalled = true;
                return Task.FromResult(new ValidationTestResponse());
            };

            var exception = await Assert.ThrowsAsync<CustomValidationException>(() =>
                behavior.Handle(new ValidationTestRequest(), next, CancellationToken.None));

            exception.Errors.Should().Contain("Error 1");
            exception.Errors.Should().Contain("Error 2");
            nextCalled.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_ValidationThrowsException_ShouldPropagateException()
        {
            var validatorMock = new Mock<IValidator<ValidationTestRequest>>();
            validatorMock
                .Setup(x => x.ValidateAsync(It.IsAny<ValidationTestRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Validation failed"));

            var behavior = new ValidationBehavior<ValidationTestRequest, ValidationTestResponse>(validatorMock.Object);
            RequestHandlerDelegate<ValidationTestResponse> next = _ => Task.FromResult(new ValidationTestResponse());

            await Assert.ThrowsAsync<Exception>(() =>
                behavior.Handle(new ValidationTestRequest(), next, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_CancellationRequested_ShouldPassCancellationToken()
        {
            var validatorMock = new Mock<IValidator<ValidationTestRequest>>();
            var cts = new CancellationTokenSource();
            cts.Cancel();

            validatorMock
                .Setup(x => x.ValidateAsync(It.IsAny<ValidationTestRequest>(), cts.Token))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            var behavior = new ValidationBehavior<ValidationTestRequest, ValidationTestResponse>(validatorMock.Object);
            RequestHandlerDelegate<ValidationTestResponse> next = _ => Task.FromResult(new ValidationTestResponse());

            await behavior.Handle(new ValidationTestRequest(), next, cts.Token);

            validatorMock.Verify(x => x.ValidateAsync(It.IsAny<ValidationTestRequest>(), cts.Token), Times.Once);
        }
    }

    public class ValidationTestRequest : IRequest<ValidationTestResponse> { }
    public class ValidationTestResponse { }
}
