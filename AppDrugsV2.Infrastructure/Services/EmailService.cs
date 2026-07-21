using AppDrugsV2.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace AppDrugsV2.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string to, string subject, string body)
        {
            // Simulación de envío de correo electrónico en consola para entorno de desarrollo
            _logger.LogInformation("\n=== SIMULACIÓN DE CORREO ELECTRÓNICO ===");
            _logger.LogInformation("Para: {To}", to);
            _logger.LogInformation("Asunto: {Subject}", subject);
            _logger.LogInformation("Cuerpo: \n{Body}", body);
            _logger.LogInformation("========================================\n");
            
            return Task.CompletedTask;
        }
    }
}
