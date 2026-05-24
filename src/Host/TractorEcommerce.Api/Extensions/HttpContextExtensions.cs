using System.Security.Claims;

namespace TractorEcommerce.Api.Extensions
{
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Extrae de forma segura el identificador único del usuario autenticado desde los Claims del JWT.
        /// </summary>
        public static string GetUserId(this HttpContext httpContext)
        {
            // El claim 'sub' del JWT se traduce automáticamente a ClaimTypes.NameIdentifier en .NET
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                // Lanzamos una excepción controlada si el token fue manipulado o no posee la estructura correcta
                throw new System.UnauthorizedAccessException("El token JWT no contiene un identificador de usuario válido.");
            }

            return userId;
        }
    }
}
