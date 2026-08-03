using System.Net;
using System.Text.Json;

namespace AppDrugsV2.Api.Middleware
{
    /// <summary>
    /// Middleware que atrapa cualquier excepción no controlada en el pipeline
    /// y devuelve un JSON estándar y seguro, sin exponer StackTraces al cliente.
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next   = next;
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
                _logger.LogError(ex, "Excepción no controlada: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var (statusCode, message) = exception switch
            {
                KeyNotFoundException  => (HttpStatusCode.NotFound,            "El recurso solicitado no fue encontrado."),
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized,  "No tiene permisos para realizar esta acción."),
                ArgumentException     => (HttpStatusCode.BadRequest,          exception.Message),
                _                     => (HttpStatusCode.InternalServerError, "Ha ocurrido un error interno. Por favor, intente más tarde.")
            };

            context.Response.StatusCode = (int)statusCode;

            var payload = JsonSerializer.Serialize(new
            {
                status  = (int)statusCode,
                error   = message,
                traceId = context.TraceIdentifier
            });

            await context.Response.WriteAsync(payload);
        }
    }
}
