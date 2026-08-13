using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Rotativa.AspNetCore;
using Serilog;
using System.Text;
using System.Threading.RateLimiting;
using AppDrugsV2.Application;
using AppDrugsV2.Application.Common.Constants;
using AppDrugsV2.Infrastructure;
using AppDrugsV2.Infrastructure.Hubs;
using AppDrugsV2.Api.Middleware;

// ── Bootstrap Logger (captura errores antes de que DI esté listo) ──────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Iniciando AppDrugsV2 API...");

    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog ─────────────────────────────────────────────────────────────
    builder.Host.UseSerilog((ctx, services, config) =>
        config.ReadFrom.Configuration(ctx.Configuration)
              .ReadFrom.Services(services)
              .Enrich.FromLogContext());

    // ── CORS ─────────────────────────────────────────────────────────────────
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(AppConstants.Cors.PolicyName,
            policy =>
            {
                policy.WithOrigins(
                          AppConstants.Cors.FrontendOrigin,
                          AppConstants.Cors.FrontendOriginLan,
                          "http://localhost:5173")
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
    });

    // ── Rate Limiting (built-in .NET 8) ──────────────────────────────────────
    builder.Services.AddRateLimiter(options =>
    {
        // Política para Login: 5 intentos por minuto por IP
        options.AddFixedWindowLimiter("auth-login", limiterOptions =>
        {
            limiterOptions.PermitLimit       = builder.Configuration.GetValue<int>("RateLimiting:AuthLoginPermitLimit", 5);
            limiterOptions.Window            = TimeSpan.FromSeconds(builder.Configuration.GetValue<int>("RateLimiting:AuthLoginWindowSeconds", 60));
            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiterOptions.QueueLimit        = 0;
        });

        // Política para Forgot-Password: 3 intentos cada 5 minutos por IP
        options.AddFixedWindowLimiter("auth-forgot", limiterOptions =>
        {
            limiterOptions.PermitLimit       = builder.Configuration.GetValue<int>("RateLimiting:AuthForgotPasswordPermitLimit", 3);
            limiterOptions.Window            = TimeSpan.FromSeconds(builder.Configuration.GetValue<int>("RateLimiting:AuthForgotPasswordWindowSeconds", 300));
            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiterOptions.QueueLimit        = 0;
        });

        // Respuesta al superar el límite
        options.OnRejected = async (ctx, token) =>
        {
            ctx.HttpContext.Response.StatusCode  = StatusCodes.Status429TooManyRequests;
            ctx.HttpContext.Response.ContentType = "application/json";
            await ctx.HttpContext.Response.WriteAsync(
                "{\"error\":\"Demasiadas solicitudes. Por favor espera antes de intentarlo nuevamente.\"}", token);
        };
    });

    builder.Services.AddControllersWithViews();
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc(AppConstants.Swagger.DocVersion,
            new OpenApiInfo { Title = AppConstants.Swagger.DocTitle, Version = AppConstants.Swagger.DocVersion });

        c.AddSecurityDefinition(AppConstants.Jwt.BearerScheme, new OpenApiSecurityScheme
        {
            Description = AppConstants.Jwt.BearerDescription,
            Name        = AppConstants.Jwt.AuthorizationHeader,
            In          = ParameterLocation.Header,
            Type        = SecuritySchemeType.ApiKey,
            Scheme      = AppConstants.Jwt.BearerScheme
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id   = AppConstants.Jwt.BearerScheme
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    var jwtSettings = builder.Configuration.GetSection(AppConstants.Jwt.SectionName);
    var secretKey   = jwtSettings[AppConstants.Jwt.SecretKey]
        ?? throw new InvalidOperationException(AppConstants.Jwt.ErrorJwtNotConfigured);

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer           = true,
                ValidateAudience         = true,
                ValidateLifetime         = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer              = jwtSettings[AppConstants.Jwt.IssuerKey]  ?? AppConstants.Jwt.DefaultIssuer,
                ValidAudience            = jwtSettings[AppConstants.Jwt.AudienceKey] ?? AppConstants.Jwt.DefaultAudience,
                IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
            };
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    if (context.Request.Cookies.ContainsKey("X-Access-Token"))
                    {
                        context.Token = context.Request.Cookies["X-Access-Token"];
                    }
                    return Task.CompletedTask;
                }
            };
        });

    builder.Services.AddAuthorization();

    var app = builder.Build();

    var webRootPath = app.Environment.WebRootPath ?? app.Environment.ContentRootPath;
    RotativaConfiguration.Setup(webRootPath, AppConstants.Rotativa.FolderName);

    // ── Pipeline ─────────────────────────────────────────────────────────────
    app.UseCors(AppConstants.Cors.PolicyName);

    // Logging de requests HTTP (Serilog)
    app.UseSerilogRequestLogging(opts =>
    {
        opts.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    });

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // ⚠️ HTTPS redirect deshabilitado para permitir acceso HTTP desde celulares en red LAN.
    // Reactivar en producción con HTTPS real: app.UseHttpsRedirection();
    app.UseMiddleware<GlobalExceptionMiddleware>();
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    // ── SignalR Hub ───────────────────────────────────────────────────────────
    app.MapHub<NotificationHub>("/hubs/notifications");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación falló durante el arranque.");
}
finally
{
    Log.CloseAndFlush();
}