using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace TractorEcommerce.Api.Extensions
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred during request execution.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var statusCode = StatusCodes.Status500InternalServerError;
            var code = "INTERNAL_SERVER_ERROR";
            var message = exception.Message;
            object? details = null;

            if (exception is InvalidOperationException)
            {
                statusCode = StatusCodes.Status400BadRequest;
                code = "BAD_REQUEST";
            }
            else if (exception is ArgumentException || exception is ArgumentNullException)
            {
                statusCode = StatusCodes.Status400BadRequest;
                code = "INVALID_ARGUMENT";
            }
            else if (exception is KeyNotFoundException)
            {
                statusCode = StatusCodes.Status404NotFound;
                code = "NOT_FOUND";
            }

            context.Response.StatusCode = statusCode;

            var errorResponse = new
            {
                code = code,
                message = message,
                details = details
            };

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, jsonOptions));
        }
    }
}
