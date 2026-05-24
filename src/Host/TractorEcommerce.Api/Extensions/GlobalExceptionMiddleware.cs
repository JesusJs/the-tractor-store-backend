using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TractorEcommerce.Api.Modules.Shared.TractorEcommerce.Modules.Shared.Application.Exceptions;

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

            // Valores por defecto (Error Interno 500)
            var statusCode = StatusCodes.Status500InternalServerError;
            var code = "INTERNAL_SERVER_ERROR";
            var message = "An unexpected error occurred on the server."; // Mensaje genérico seguro para producción
            object? details = null;

            // Evaluamos el tipo de excepción utilizando Pattern Matching moderno de C#
            switch (exception)
            {
                case DomainValidationException valEx:
                    statusCode = StatusCodes.Status400BadRequest;
                    code = "VALIDATION_ERROR";
                    message = valEx.Message;
                    details = valEx.Details; // Aquí capturamos los detalles para Angular
                    break;

                case ArgumentException _ or ArgumentNullException _ or InvalidOperationException _:
                    statusCode = StatusCodes.Status400BadRequest;
                    code = "BAD_REQUEST";
                    message = exception.Message;
                    break;

                case KeyNotFoundException _ or DomainNotFoundException _:
                    statusCode = StatusCodes.Status404NotFound;
                    code = "NOT_FOUND";
                    message = exception.Message;
                    break;

                case DomainConflictException conflictEx:
                    statusCode = StatusCodes.Status409Conflict; // Cumplimos con el HTTP 409 requerido
                    code = "CONFLICT";
                    message = conflictEx.Message;
                    break;

                default:
                    // Si estás en entorno de Desarrollo (Local), puedes adjuntar el StackTrace en los detalles
                    // En producción, details se mantendrá null para no filtrar secretos del código
#if DEBUG
                    details = new { stackTrace = exception.StackTrace };
                    message = exception.Message;
#endif
                    break;
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
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                // Ignora propiedades nulas si quieres ahorrar ancho de banda, 
                // o déjalo comentando si deseas enviar explicitamente "details: null"
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, jsonOptions));
        }
    }
}