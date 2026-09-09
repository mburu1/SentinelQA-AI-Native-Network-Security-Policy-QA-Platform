using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using MongoDB.Driver;
using Scalar.AspNetCore;
using SentinelQA.Api.Authorization;
using SentinelQA.Api.Filters;
using SentinelQA.Api.Infrastructure;
using SentinelQA.Api.Middleware;
using SentinelQA.Application;
using SentinelQA.Application.Abstractions;
using SentinelQA.Infrastructure;
using SentinelQA.Modules.Ai;
using SentinelQA.Modules.Audit;
using SentinelQA.Modules.ChangeManagement;
using SentinelQA.Modules.Defects;
using SentinelQA.Modules.Firewalls;
using SentinelQA.Modules.Identity;
using SentinelQA.Modules.Networks;
using SentinelQA.Modules.Notifications;
using SentinelQA.Modules.Policies;
using SentinelQA.Modules.Tenants;
using SentinelQA.Modules.Testing;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ---------- Serilog ----------
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// ---------- MVC + OpenAPI ----------
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info = new OpenApiInfo
        {
            Title = "SentinelQA API",
            Version = "v1",
            Description = "AI-native network security policy & QA platform."
        };
        return Task.CompletedTask;
    });
});

// ---------- Clean Architecture wiring ----------
builder.Services.AddApplication(
    typeof(SentinelQA.Modules.Identity.DependencyInjection).Assembly,
    typeof(SentinelQA.Modules.Tenants.DependencyInjection).Assembly,
    typeof(SentinelQA.Modules.Firewalls.DependencyInjection).Assembly,
    typeof(SentinelQA.Modules.Networks.DependencyInjection).Assembly,
    typeof(SentinelQA.Modules.Policies.DependencyInjection).Assembly,
    typeof(SentinelQA.Modules.ChangeManagement.DependencyInjection).Assembly,
    typeof(SentinelQA.Modules.Testing.DependencyInjection).Assembly,
    typeof(SentinelQA.Modules.Defects.DependencyInjection).Assembly,
    typeof(SentinelQA.Modules.Notifications.DependencyInjection).Assembly,
    typeof(SentinelQA.Modules.Audit.DependencyInjection).Assembly,
    typeof(SentinelQA.Modules.Ai.DependencyInjection).Assembly);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddIdentityModule();
builder.Services.AddTenantsModule();
builder.Services.AddFirewallsModule();
builder.Services.AddNetworksModule();
builder.Services.AddPoliciesModule();
builder.Services.AddChangeManagementModule();
builder.Services.AddTestingModule();
builder.Services.AddDefectsModule();
builder.Services.AddNotificationsModule();
builder.Services.AddAuditModule();
builder.Services.AddAiModule();

builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
builder.Services.AddScoped<IdempotencyFilter>();

// ---------- Authentication & authorization ----------
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SigningKey"]!)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromSeconds(10)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(PolicyNames.CanDeployPolicy, p => p.RequireRole(Roles.Admin, Roles.SecurityEngineer));
    options.AddPolicy(PolicyNames.CanApproveChange, p => p.RequireRole(Roles.Admin, Roles.Approver));
    options.AddPolicy(PolicyNames.CanManageUsers, p => p.RequireRole(Roles.Admin));
    options.AddPolicy(PolicyNames.CanRunSecurityTests, p => p.RequireRole(Roles.Admin, Roles.QAEngineer, Roles.SecurityEngineer));
    options.AddPolicy(PolicyNames.CanViewAuditTrail, p => p.RequireRole(Roles.Admin, Roles.QAEngineer));
});

// ---------- Rate limiting ----------
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 300,
                Window = TimeSpan.FromMinutes(1)
            }));
});

// ---------- Health checks ----------
builder.Services
    .AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("Postgres")!, name: "postgresql")
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!, name: "redis")
    .AddMongoDb(
        sp => new MongoClient(builder.Configuration.GetConnectionString("Mongo")!),
        name: "mongodb");

var app = builder.Build();

// ---------- Pipeline ----------
app.UseSerilogRequestLogging();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.Run();

// Exposed for SentinelQA.ApiTests (WebApplicationFactory).
public partial class Program;