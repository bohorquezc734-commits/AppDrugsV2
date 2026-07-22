using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Infrastructure.Hubs;
using AppDrugsV2.Infrastructure.Persistence;
using AppDrugsV2.Infrastructure.Services;
using AppDrugsV2.Infrastructure.Services.Auth;
using AppDrugsV2.Infrastructure.Services.Reports;

namespace AppDrugsV2.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Configurar DbContext con SQL Server
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            // Registrar servicios
            services.AddScoped<IApplicationDbContext>(provider =>
                provider.GetRequiredService<ApplicationDbContext>());

            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            // Registrar HttpContextAccessor
            services.AddHttpContextAccessor();

            // Registrar servicios de reportes
            services.AddScoped<IExcelExportService, ExcelExportService>();
            services.AddScoped<IEmailService, EmailService>();

            // ── QR y notificaciones en tiempo real ────────────────────────────
            // QrCodeService: sin estado → Singleton es suficiente.
            services.AddSingleton<IQrCodeService, QrCodeService>();

            // NotificationHubService: depende de IHubContext que es Singleton.
            services.AddSingleton<INotificationHubService, NotificationHubService>();

            // Registrar SignalR en el contenedor (el Hub se mapea en Program.cs).
            services.AddSignalR();

            return services;
        }
    }
}