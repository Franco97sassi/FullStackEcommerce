using Ecommerce.API.Middleware;
using Ecommerce.API.Options;
using Ecommerce.API.Services;
using Ecommerce.Infrastructure;
using Ecommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Ecommerce.API.Grpc;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();
builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<ServiceAuthenticationInterceptor>();
    options.Interceptors.Add<GrpcExceptionInterceptor>();
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
});
builder.Services.AddGrpcHealthChecks();
builder.Services.Configure<ServiceAuthenticationOptions>(builder.Configuration.GetSection(ServiceAuthenticationOptions.SectionName));
builder.Services.AddSingleton<InventoryReservationStore>();
builder.Services.AddInternalGrpcClients(builder.Configuration);
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("ecommerce-api"))
    .WithTracing(tracing => tracing.AddAspNetCoreInstrumentation().AddGrpcClientInstrumentation().AddOtlpExporter());

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(kvp => kvp.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors
                    .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage) ? "Valor inválido." : error.ErrorMessage)
                    .ToArray());

        var response = new
        {
            type = "validation_error",
            title = "Request inválida",
            status = StatusCodes.Status400BadRequest,
            detail = "Uno o más campos son inválidos.",
            traceId = context.HttpContext.TraceIdentifier,
            errors
        };

        return new BadRequestObjectResult(response);
    };
});
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(); 
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresá el token JWT. Ejemplo: Bearer eyJhbGciOi..."
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

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSingleton<RequestMetricsStore>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("Jwt options are not configured.");
if (!builder.Environment.IsDevelopment() && jwtOptions.Key.Length < 32)
{
    throw new InvalidOperationException("Jwt key must be at least 32 characters in non-development environments.");
}

builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

        var permitLimit = 100;
        var window = TimeSpan.FromMinutes(1);

        if (path.StartsWith("/api/auth"))
        {
            permitLimit = 10;
        }
        else if (path.StartsWith("/api/checkout"))
        {
            permitLimit = 30;
        }

        return RateLimitPartition.GetFixedWindowLimiter(
            $"{remoteIp}:{path}",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = window,
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true
            });
    });
});
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<EcommerceDbContext>();
    if (app.Environment.IsEnvironment("Testing"))
    {
        await dbContext.Database.EnsureCreatedAsync();
    }
    else
    {
        await dbContext.Database.MigrateAsync();
    }
    await CatalogSeed.SeedAsync(dbContext);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<RequestMetricsMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseCors("AllowFrontend");
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/healthz");
app.MapGet("/metrics", (RequestMetricsStore store) => Results.Text(store.ToPrometheus(), "text/plain; version=0.0.4"));
app.MapControllers();
app.MapGrpcService<CatalogGrpcService>();
app.MapGrpcService<InventoryGrpcService>();
app.MapGrpcService<OrderGrpcService>();
app.MapGrpcService<PaymentGrpcService>();
app.MapGrpcHealthChecksService();

app.Run();

public partial class Program;
