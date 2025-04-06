using System.Text;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Pharmacy.BackgroundServices;
using Pharmacy.Data;
using Pharmacy.Data.Authorization;
using Pharmacy.Data.Repositories;
using Pharmacy.Data.Repositories.Interfaces;
using Pharmacy.Middleware;
using Pharmacy.Services;
using Pharmacy.Services.Interfaces;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    Log.Information("Starting web application");
        
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddSerilog();
    builder.Host.UseSerilog((context, configuration) =>
    {
        configuration.ReadFrom.Configuration(context.Configuration);
    });
    
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    builder.Services.AddDbContext<PharmacyDbContext>(options =>
        options.UseNpgsql(connectionString));
    
    builder.Services.AddFastEndpoints()
        .SwaggerDocument(options =>
        {
            options.EnableJWTBearerAuth = true;
            options.RemoveEmptyRequestSchema = true;
            options.DocumentSettings = settings =>
            {
                settings.Title = "Pharmacy API";
                settings.Version = "v1";
                // settings.AddAuth("Bearer", new()
                // {
                //     Type = OpenApiSecuritySchemeType.ApiKey,
                //     Scheme = JwtBearerDefaults.AuthenticationScheme,
                //     BearerFormat = "JWT",
                // });
            };
        });
    
    builder.Services.AddHostedService<EmailVerificationCleanupService>();
    
    builder.Services.AddSingleton<PasswordProvider>();
    builder.Services.AddSingleton<TokenProvider>();
    
    builder.Services.AddScoped<IManufacturerRepository, ManufacturerRepository>();
    builder.Services.AddScoped<IManufacturerService, ManufacturerService>();
    
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();
    
    builder.Services.AddAuthorization();
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ClockSkew = TimeSpan.Zero,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
        };
    });
    
    builder.Services.AddOpenApi();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }
    
    app.UseHttpsRedirection();

    app.UseExceptionHandler();
    
    app.UseAuthentication();
    app.UseAuthorization();

    app.UseFastEndpoints().
        UseSwaggerGen();
    
    app.UseSerilogRequestLogging();
        
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}