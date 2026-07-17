using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Rotativa.AspNetCore;
using System.Text;
using AppDrugsV2.Application;
using AppDrugsV2.Application.Common.Constants;
using AppDrugsV2.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ✅ CORS para desarrollo local (acepta React en http y https)
builder.Services.AddCors(options =>
{
    options.AddPolicy(AppConstants.Cors.PolicyName,
        policy =>
        {
            policy.WithOrigins(AppConstants.Cors.FrontendOrigin)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
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
    });

builder.Services.AddAuthorization();

var app = builder.Build();

var webRootPath = app.Environment.WebRootPath ?? app.Environment.ContentRootPath;
RotativaConfiguration.Setup(webRootPath, AppConstants.Rotativa.FolderName);

// ✅ CORS debe ir antes de Authentication/Authorization
app.UseCors(AppConstants.Cors.PolicyName);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();