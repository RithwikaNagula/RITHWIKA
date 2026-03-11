// Application entry point. Configures services (DI, JWT, EF Core, SignalR, CORS, Swagger), seeds the default admin user and insurance catalog, then builds and starts the ASP.NET pipeline.
using API.Extensions;
using API.ExceptionHandlers;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddDbContext<InsuranceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddApplicationServices();

builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddAuthorization();

builder.Services.AddSwaggerWithJwt();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

//Seed Admin User
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<InsuranceDbContext>();

    try
    {
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Migration failed. Database may already exist. Ensuring created...");
        context.Database.EnsureCreated();
    }

    var admin = context.Users.FirstOrDefault(u => u.Role == UserRole.Admin);
    if (admin == null)
    {
        admin = new User
        {
            Id = "adm1",
            FullName = "System Admin",
            Email = "admin@insurance.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(admin);
        context.SaveChanges();
        Console.WriteLine("Admin user seeded: admin@insurance.com / Admin@123");
    }
    else
    {
        // Force update password just in case
        admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
        context.SaveChanges();
        Console.WriteLine("Admin user password reset to Admin@123");
    }



    // Seed Insurance Types and Plans
    if (!context.InsuranceTypes.Any())
    {
        var types = new List<InsuranceType>
        {
            new InsuranceType
            {
                Id = "typ_gen_liab",
                TypeName = "General Liability Insurance",
                Description = "Protects against lawsuits and other claims arising from your general operations.",
                CreatedAt = DateTime.UtcNow,
                Plans = new List<Plan>
                {
                    new Plan { Id = "pln_gl_bas", PlanName = "Basic Liability", Description = "Essential coverage for small businesses.", MinCoverageAmount = 500000, MaxCoverageAmount = 1000000, BasePremium = 8000, DurationInMonths = 12, CreatedAt = DateTime.UtcNow },
                    new Plan { Id = "pln_gl_std", PlanName = "Standard Liability", Description = "Comprehensive protection for medium enterprises.", MinCoverageAmount = 1000000, MaxCoverageAmount = 2000000, BasePremium = 15000, DurationInMonths = 24, CreatedAt = DateTime.UtcNow },
                    new Plan { Id = "pln_gl_prm", PlanName = "Premium Liability", Description = "Maximum coverage and lowest deductibles for large corporations.", MinCoverageAmount = 2000000, MaxCoverageAmount = 5000000, BasePremium = 35000, DurationInMonths = 36, CreatedAt = DateTime.UtcNow }
                }
            },
            new InsuranceType
            {
                Id = "typ_auto",
                TypeName = "Auto Insurance",
                Description = "Insurance coverage for commercial vehicles, protecting against accidents and liability.",
                CreatedAt = DateTime.UtcNow,
                Plans = new List<Plan>
                {
                    new Plan { Id = "pln_au_bas", PlanName = "Basic Auto", Description = "Liability only for essential commercial vehicles.", MinCoverageAmount = 100000, MaxCoverageAmount = 500000, BasePremium = 5000, DurationInMonths = 6, CreatedAt = DateTime.UtcNow },
                    new Plan { Id = "pln_au_comp", PlanName = "Comprehensive Auto", Description = "Full coverage including collision for standard fleets.", MinCoverageAmount = 500000, MaxCoverageAmount = 1000000, BasePremium = 12000, DurationInMonths = 12, CreatedAt = DateTime.UtcNow },
                    new Plan { Id = "pln_au_prm", PlanName = "Premium Fleet Shield", Description = "Extensive coverage for large vehicle operations and heavy transport.", MinCoverageAmount = 1000000, MaxCoverageAmount = 3000000, BasePremium = 28000, DurationInMonths = 24, CreatedAt = DateTime.UtcNow }
                }
            },
            new InsuranceType
            {
                Id = "typ_work_comp",
                TypeName = "Workers Compensation Insurance",
                Description = "Provides wage replacement and medical benefits to employees injured in the course of employment.",
                CreatedAt = DateTime.UtcNow,
                Plans = new List<Plan>
                {
                    new Plan { Id = "pln_wc_bas", PlanName = "Basic Comp", Description = "Meets minimum state requirements for worker compensation.", MinCoverageAmount = 100000, MaxCoverageAmount = 500000, BasePremium = 6000, DurationInMonths = 6, CreatedAt = DateTime.UtcNow },
                    new Plan { Id = "pln_wc_std", PlanName = "Standard Support", Description = "Extended rehabilitation support and larger payout limits.", MinCoverageAmount = 500000, MaxCoverageAmount = 1500000, BasePremium = 14000, DurationInMonths = 12, CreatedAt = DateTime.UtcNow },
                    new Plan { Id = "pln_wc_prm", PlanName = "Premium Care", Description = "Full protection including legal defence and extensive lifetime care options.", MinCoverageAmount = 1500000, MaxCoverageAmount = 5000000, BasePremium = 32000, DurationInMonths = 24, CreatedAt = DateTime.UtcNow }
                }
            },
            new InsuranceType
            {
                Id = "typ_bus_int",
                TypeName = "Business Interruption Insurance",
                Description = "Protects business income during unexpected disruptions and disaster recovery.",
                CreatedAt = DateTime.UtcNow,
                Plans = new List<Plan>
                {
                    new Plan { Id = "pln_bi_sht", PlanName = "Short Term Shield", Description = "Coverage for brief operational interruptions up to 3 months.", MinCoverageAmount = 50000, MaxCoverageAmount = 200000, BasePremium = 4000, DurationInMonths = 6, CreatedAt = DateTime.UtcNow },
                    new Plan { Id = "pln_bi_std", PlanName = "Standard Continuation", Description = "Robust coverage supporting up to 6 months of lost income.", MinCoverageAmount = 200000, MaxCoverageAmount = 500000, BasePremium = 9000, DurationInMonths = 12, CreatedAt = DateTime.UtcNow },
                    new Plan { Id = "pln_bi_ext", PlanName = "Extended Resilience", Description = "Ultimate long-term protection for critical operational continuity.", MinCoverageAmount = 500000, MaxCoverageAmount = 2000000, BasePremium = 25000, DurationInMonths = 24, CreatedAt = DateTime.UtcNow }
                }
            }
        };
        context.InsuranceTypes.AddRange(types);
        context.SaveChanges();
        Console.WriteLine("Seeded new insurance types and plans.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Commercial Insurance API v1");
    });
}

app.UseCors("AllowAll");
app.UseExceptionHandler();


app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<Application.Hubs.NotificationHub>("/notificationHub");

try
{
    await app.RunAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"Critical failure during app startup: {ex}");
    throw;
}

