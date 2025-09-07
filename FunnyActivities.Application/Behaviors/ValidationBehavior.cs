using FluentValidation;
using MediatR;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationBehavior{TRequest, TResponse}"/> class.
        /// </summary>
        /// <param name="validator">The validator for the request type, if available.</param>
        public ValidationBehavior(IValidator<TRequest>? validator = null)
        {
            _validator = validator;
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
            if (_validator != null)
            {
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);

                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => e.ErrorMessage);
                    throw new CustomValidationException(errors);
                }
            }

            return await next();
        }
    }
}