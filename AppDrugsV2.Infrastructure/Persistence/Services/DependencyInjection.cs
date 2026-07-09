using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Infrastructure.Persistence;
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

            return services;
        }
    }
}