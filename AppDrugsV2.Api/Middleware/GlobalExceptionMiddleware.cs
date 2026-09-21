using System.Net;
using System.Text.Json;
using AppDrugsV2.Application.Common.Constants;

namespace AppDrugsV2.Api.Middleware
{
  
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
            context.Response.ContentType = AppConstants.Middleware.ContentTypeJson;

            var (statusCode, message) = exception switch
            {
                KeyNotFoundException        => (HttpStatusCode.NotFound,            AppConstants.Middleware.NotFoundError),
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized,        AppConstants.Middleware.UnauthorizedError),
                ArgumentException           => (HttpStatusCode.BadRequest,          exception.Message),
                _                           => (HttpStatusCode.InternalServerError, AppConstants.Middleware.InternalServerError)
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
