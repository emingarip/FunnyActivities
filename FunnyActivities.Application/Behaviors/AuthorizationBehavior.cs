using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace FunnyActivities.Application.Behaviors
{
    /// <summary>
    /// MediatR pipeline behavior that performs authorization checks for commands and queries.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationBehavior{TRequest, TResponse}"/> class.
        /// </summary>
        /// <param name="authorizationService">The authorization service for policy evaluation.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor for accessing the current user context.</param>
        public AuthorizationBehavior(
            IAuthorizationService authorizationService,
            IHttpContextAccessor httpContextAccessor)
        {
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Handles the request by performing authorization checks before proceeding to the next behavior in the pipeline.
        /// </summary>
        /// <param name="request">The request to authorize and handle.</param>
        /// <param name="next">The next delegate in the pipeline.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The response from the next behavior in the pipeline.</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user is not authorized for the requested operation.</exception>
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User == null)
            {
                throw new UnauthorizedAccessException("User context is not available.");
            }

            // Determine the appropriate policy based on the request type
            string? policyName = GetPolicyForRequest(request);

            if (!string.IsNullOrEmpty(policyName))
            {
                var authorizationResult = await _authorizationService.AuthorizeAsync(httpContext.User, policyName);

                if (!authorizationResult.Succeeded)
                {
                    throw new UnauthorizedAccessException($"Access denied. Required policy: {policyName}");
                }
            }

            return await next();
        }

        /// <summary>
        /// Determines the authorization policy required for the given request type.
        /// </summary>
        /// <param name="request">The request to evaluate.</param>
        /// <returns>The name of the required policy, or null if no authorization is required.</returns>
        private static string? GetPolicyForRequest(TRequest request)
        {
            var requestType = request.GetType();

            // Commands - require specific permissions
            if (requestType.Name.Contains("UpdateMaterialCommand"))
            {
                return "CanUpdateMaterial";
            }

            if (requestType.Name.Contains("DeleteMaterialCommand"))
            {
                return "CanDeleteMaterial";
            }

            if (requestType.Name.Contains("UploadMaterialPhotosCommand") || requestType.Name.Contains("DeleteMaterialPhotoCommand"))
            {
                return "CanManagePhotos";
            }

            // Queries - require view permissions
            if (requestType.Name.Contains("GetMaterialQuery") ||
                requestType.Name.Contains("GetMaterialsQuery") ||
                requestType.Name.Contains("GetMaterialPhotosQuery") ||
                requestType.Name.Contains("DownloadMaterialPhotoQuery"))
            {
                return "CanViewMaterial";
            }

            // Default: no specific authorization required
            return null;
        }
    }
}