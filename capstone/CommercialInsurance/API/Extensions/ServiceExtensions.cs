// Extension methods that register all application services, repositories, JWT authentication, and Swagger/OpenAPI configuration into the DI container.
using System.Text;
using Application.Helpers;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Services;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace API.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Repositories
            // Registering the Generic Repository avoids redefining basic CRUD operations multiple times
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IPlanRepository, PlanRepository>();
            services.AddScoped<IPolicyRepository, PolicyRepository>();
            services.AddScoped<IClaimRepository, ClaimRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IBusinessProfileRepository, BusinessProfileRepository>();
            services.AddScoped<IInsuranceTypeRepository, InsuranceTypeRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();

            // Services
            // Inject distinct application domain logic so controllers don't depend directly on repositories
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IPolicyService, PolicyService>();
            services.AddScoped<IClaimService, ClaimService>();
            services.AddScoped<IPlanService, PlanService>();
            services.AddScoped<IInsuranceTypeService, InsuranceTypeService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IBusinessProfileService, BusinessProfileService>();
            services.AddScoped<IAdminDashboardService, AdminDashboardService>();
            services.AddScoped<IAgentDashboardService, AgentDashboardService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<ISupportInquiryService, SupportInquiryService>();
            services.AddScoped<IAnalyticsService, AnalyticsService>();

            // Helpers
            // Central utility for issuing access tokens via symmetric encryption
            services.AddScoped<JwtTokenGenerator>();

            // Recaptcha
            services.AddHttpClient();
            services.AddScoped<IRecaptchaService, RecaptchaService>();

            return services;
        }

        // Sets up middleware allowing API endpoints to require Microsoft [Authorize] verification.
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                // Ensures all incoming tokens are unexpired and genuinely from our authority server.
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!))
                };

                // Add support for SignalR authentication
                // SignalR uses WebSockets which don't inherently carry Authorization request headers natively.
                // Ergo, we safely extract the token directly from the connection handshake URL params.
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationHub"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            return services;
        }

        // Enhances our standard autogenerated backend Swagger page so it displays the input box
        // allowing developers and QA engineers to manually paste in JWT Bearer tokens to test endpoints.
        public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Commercial Insurance API",
                    Version = "v1",
                    Description = "API for managing commercial insurance workflows"
                });

                // Instructs Swagger on the exact header structure expected: "Authorization: Bearer <TOKEN>"
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter your JWT token"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            return services;
        }
    }
}
