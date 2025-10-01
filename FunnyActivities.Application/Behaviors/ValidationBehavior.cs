using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using CustomValidationException = FunnyActivities.CrossCuttingConcerns.ErrorHandling.ValidationException;

namespace FunnyActivities.Application.Behaviors
{
    /// <summary>
    /// MediatR pipeline behavior that validates requests using FluentValidation validators.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly IValidator<TRequest>? _validator;
        private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationBehavior{TRequest, TResponse}"/> class.
        /// </summary>
        /// <param name="validator">The validator for the request type, if available.</param>
        /// <param name="logger">The logger instance.</param>
        public ValidationBehavior(IValidator<TRequest>? validator = null, ILogger<ValidationBehavior<TRequest, TResponse>>? logger = null)
        {
            _validator = validator;
            _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<ValidationBehavior<TRequest, TResponse>>.Instance;
        }

        /// <summary>
        /// Handles the request by validating it before proceeding to the next behavior in the pipeline.
        /// </summary>
        /// <param name="request">The request to validate and handle.</param>
        /// <param name="next">The next delegate in the pipeline.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The response from the next behavior in the pipeline.</returns>
        /// <exception cref="ValidationException">Thrown when validation fails.</exception>
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // Log validation start
            _logger.LogInformation("[ValidationBehavior] Starting validation for request type: {RequestType}", typeof(TRequest).Name);
            _logger.LogInformation("[ValidationBehavior] Validator available: {ValidatorAvailable}", _validator != null);

            if (_validator != null)
            {
                _logger.LogInformation("[ValidationBehavior] Executing validator: {ValidatorType}", _validator.GetType().Name);

                var validationResult = await _validator.ValidateAsync(request, cancellationToken);

                _logger.LogInformation("[ValidationBehavior] Validation completed. IsValid: {IsValid}, Errors count: {ErrorCount}",
                    validationResult.IsValid, validationResult.Errors.Count);

                foreach (var error in validationResult.Errors)
                {
                    _logger.LogWarning("[ValidationBehavior] Validation Error - Property: {PropertyName}, Message: {ErrorMessage}, AttemptedValue: {AttemptedValue}",
                        error.PropertyName, error.ErrorMessage, error.AttemptedValue);
                }

                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => e.ErrorMessage);
                    _logger.LogError("[ValidationBehavior] Validation failed with {ErrorCount} errors: {Errors}",
                        errors.Count(), string.Join("; ", errors));
                    throw new CustomValidationException(errors);
                }

                _logger.LogInformation("[ValidationBehavior] Validation passed successfully");
            }
            else
            {
                _logger.LogWarning("[ValidationBehavior] No validator found for request type: {RequestType}", typeof(TRequest).Name);
            }

            _logger.LogInformation("[ValidationBehavior] Proceeding to next behavior in pipeline");
            return await next();
        }
    }
}