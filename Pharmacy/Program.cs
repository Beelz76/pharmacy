using System.Net.Http.Headers;
using System.Text;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Minio;
using Pharmacy.BackgroundServices;
using Pharmacy.Database;
using Pharmacy.Database.Repositories;
using Pharmacy.Database.Repositories.Interfaces;
using Pharmacy.DateTimeProvider;
using Pharmacy.Endpoints.Users.Authentication;
using Pharmacy.Endpoints.Users.Verification;
using Pharmacy.ExternalServices;
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
    
    builder.Services.AddAuthorization();
    
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .WithOrigins("http://localhost:5174", "http://localhost:5173");
        });
    });

    builder.Services.AddFastEndpoints()
        .SwaggerDocument(options =>
        {
            options.EnableJWTBearerAuth = true;
            options.RemoveEmptyRequestSchema = true;
            options.DocumentSettings = settings =>
            {
                settings.Title = "Pharmacy API";
                settings.Version = "v1";
            };
        });
    
    builder.Services.AddHttpClient<YooKassaHttpClient>((sp, client) =>
    {
        var config = sp.GetRequiredService<IConfiguration>();
        var shopId = config["YooKassa:ShopId"];
        var apiKey = config["YooKassa:ApiKey"];

        client.BaseAddress = new Uri("https://api.yookassa.ru/v3/");
        var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{shopId}:{apiKey}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);
    });

    
    builder.Services.AddSingleton(sp =>
    {
        var config = sp.GetRequiredService<IConfiguration>();

        return new MinioClient()
            .WithEndpoint(config["S3:ServiceUrl"])
            .WithCredentials(config["S3:AccessKey"], config["S3:SecretKey"])
            .WithSSL()
            .Build();
    });

    builder.Services.AddSingleton<IStorageProvider, StorageProvider>();

    builder.Services.AddHostedService<EmailVerificationCleanupService>();
    builder.Services.AddHostedService<ExpiredOrderCleanupService>();
    
    builder.Services.AddSingleton<PasswordProvider>();
    builder.Services.AddSingleton<TokenProvider>();
    builder.Services.AddSingleton<CodeGenerator>();
    builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
    
    builder.Services.AddScoped<IProductImageService, ProductImageService>();
    builder.Services.AddScoped<IEmailSender, EmailSender>();
    builder.Services.AddScoped<IProductService, ProductService>();
    builder.Services.AddScoped<IProductRepository, ProductRepository>();
    builder.Services.AddScoped<ICartRepository, CartRepository>();
    builder.Services.AddScoped<ICartService, CartService>();
    builder.Services.AddScoped<IFavoritesRepository, FavoritesRepository>();
    builder.Services.AddScoped<IFavoritesService, FavoritesService>();
    builder.Services.AddScoped<IProductCategoryService, ProductCategoryService>();
    builder.Services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
    builder.Services.AddScoped<IEmailVerificationService, EmailVerificationService>();
    builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
    builder.Services.AddScoped<IManufacturerRepository, ManufacturerRepository>();
    builder.Services.AddScoped<IManufacturerService, ManufacturerService>();
    builder.Services.AddScoped<IProductRepository, ProductRepository>();
    builder.Services.AddScoped<IProductService, ProductService>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
    builder.Services.AddScoped<IPaymentService, PaymentService>();
    builder.Services.AddScoped<IOrderRepository, OrderRepository>();
    builder.Services.AddScoped<IOrderService, OrderService>();
    builder.Services.AddScoped<IEmailVerificationCodeRepository, EmailVerificationCodeRepository>();
    builder.Services.AddScoped<IUserAddressRepository, UserAddressRepository>();
    builder.Services.AddScoped<IUserAddressService, UserAddressService>();
    builder.Services.AddScoped<IPharmacyProductRepository, PharmacyProductRepository>();
    builder.Services.AddScoped<IPharmacyProductService, PharmacyProductService>();
    builder.Services.AddScoped<IAddressRepository, AddressRepository>();
    builder.Services.AddScoped<IPharmacyRepository, PharmacyRepository>();
    builder.Services.AddScoped<IPharmacyService, PharmacyService>();
    builder.Services.AddScoped<IDeliveryRepository, DeliveryRepository>();
    builder.Services.AddScoped<IDeliveryService, DeliveryService>();
    
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();
    
    builder.Services.AddOpenApi();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }
    
    app.UseHttpsRedirection();
    
    app.UseCors();
    
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