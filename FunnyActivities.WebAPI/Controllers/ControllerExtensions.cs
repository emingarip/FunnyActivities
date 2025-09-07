using Microsoft.AspNetCore.Mvc;
using FunnyActivities.Application.DTOs.Shared;

namespace FunnyActivities.WebAPI.Controllers
{
    /// <summary>
    /// Extension methods for ControllerBase to provide standardized API responses.
    /// </summary>
    public static class ControllerExtensions
    {
        /// <summary>
        /// Returns a standardized success response with data.
        /// </summary>
        /// <typeparam name="T">The type of data to return.</typeparam>
        /// <param name="controller">The controller instance.</param>
        /// <param name="data">The data to include in the response.</param>
        /// <param name="message">Optional success message.</param>
        /// <param name="statusCode">HTTP status code (default: 200 OK).</param>
        /// <returns>An IActionResult with standardized success response.</returns>
        public static IActionResult ApiSuccess<T>(this ControllerBase controller, T data, string message = "", int statusCode = 200)
        {
            var response = ApiResponse<T>.CreateSuccess(data, message);
            return controller.StatusCode(statusCode, response);
        }

        /// <summary>
        /// Returns a standardized success response without data (useful for DELETE operations).
        /// </summary>
        /// <typeparam name="T">The type of data (can be object for no data).</typeparam>
        /// <param name="controller">The controller instance.</param>
        /// <param name="message">Optional success message.</param>
        /// <param name="statusCode">HTTP status code (default: 200 OK).</param>
        /// <returns>An IActionResult with standardized success response.</returns>
        public static IActionResult ApiSuccess<T>(this ControllerBase controller, string message = "", int statusCode = 200)
        {
            var response = ApiResponse<T>.CreateSuccess(message);
            return controller.StatusCode(statusCode, response);
        }

        /// <summary>
        /// Returns a standardized error response.
        /// </summary>
        /// <param name="controller">The controller instance.</param>
        /// <param name="message">The error message.</param>
        /// <param name="error">The error type or category.</param>
        /// <param name="statusCode">HTTP status code (default: 400 Bad Request).</param>
        /// <returns>An IActionResult with standardized error response.</returns>
        public static IActionResult ApiError(this ControllerBase controller, string message, string error = "Error", int statusCode = 400)
        {
            var response = ApiErrorResponse.CreateError(message, error);
            return controller.StatusCode(statusCode, response);
        }

        /// <summary>
        /// Returns a standardized success response for Created operations.
        /// </summary>
        /// <typeparam name="T">The type of data to return.</typeparam>
        /// <param name="controller">The controller instance.</param>
        /// <param name="actionName">The name of the action to generate the location URL.</param>
        /// <param name="routeValues">The route values for the location URL.</param>
        /// <param name="data">The data to include in the response.</param>
        /// <param name="message">Optional success message.</param>
        /// <returns>An IActionResult with standardized success response and Created status.</returns>
        public static IActionResult ApiCreated<T>(this ControllerBase controller, string actionName, object routeValues, T data, string message = "")
        {
            var response = ApiResponse<T>.CreateSuccess(data, message);
            return controller.CreatedAtAction(actionName, routeValues, response);
        }

        /// <summary>
        /// Returns a standardized success response for Created operations without data.
        /// </summary>
        /// <typeparam name="T">The type of data (can be object for no data).</typeparam>
        /// <param name="controller">The controller instance.</param>
        /// <param name="actionName">The name of the action to generate the location URL.</param>
        /// <param name="routeValues">The route values for the location URL.</param>
        /// <param name="message">Optional success message.</param>
        /// <returns>An IActionResult with standardized success response and Created status.</returns>
        public static IActionResult ApiCreated<T>(this ControllerBase controller, string actionName, object routeValues, string message = "")
        {
            var response = ApiResponse<T>.CreateSuccess(message);
            return controller.CreatedAtAction(actionName, routeValues, response);
        }
    }
}