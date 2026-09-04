using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SentinelQA.Application.Abstractions;
using SentinelQA.Application.Abstractions.External;
using SentinelQA.Application.Abstractions.Identity;
using SentinelQA.Application.Abstractions.Infrastructure;
using SentinelQA.Application.Abstractions.Messaging;
using SentinelQA.Application.Abstractions.Persistence;
using SentinelQA.Domain.Aggregates;
using SentinelQA.Infrastructure.Ai;
using SentinelQA.Infrastructure.Email;
using SentinelQA.Infrastructure.Firewalls;
using SentinelQA.Infrastructure.Messaging;
using SentinelQA.Infrastructure.Messaging.Kafka;
using SentinelQA.Infrastructure.Messaging.RabbitMq;
using SentinelQA.Infrastructure.MongoDb;
using SentinelQA.Infrastructure.Persistence;
using SentinelQA.Infrastructure.Persistence.Outbox;
using SentinelQA.Infrastructure.Persistence.Repositories;
using SentinelQA.Infrastructure.Redis;
using SentinelQA.Infrastructure.Security;
using StackExchange.Redis;

namespace SentinelQA.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // --- Options ---
        services.Configure<MessagingOptions>(configuration.GetSection("Messaging"));
        services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMq"));
        services.Configure<KafkaOptions>(configuration.GetSection("Kafka"));

        // --- PostgreSQL ---
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres")));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPolicyRepository, PolicyRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IChangeRequestRepository, ChangeRequestRepository>();
        services.AddScoped<ITestRunRepository, TestRunRepository>();
        services.AddScoped<ITestSuiteRepository, TestSuiteRepository>();
        services.AddScoped<IRepository<Firewall, Guid>, EfRepository<Firewall>>();
        services.AddScoped<IRepository<Network, Guid>, EfRepository<Network>>();
        services.AddScoped<IRepository<TestRun, Guid>, EfRepository<TestRun>>();
        services.AddScoped<IRepository<TestSuite, Guid>, EfRepository<TestSuite>>();
        services.AddScoped<IRepository<Defect, Guid>, EfRepository<Defect>>();
        services.AddScoped<IRepository<Tenant, Guid>, EfRepository<Tenant>>();
        services.AddScoped<IRepository<AuditEntry, Guid>, EfRepository<AuditEntry>>();
        services.AddScoped<IRepository<NotificationDelivery, Guid>, EfRepository<NotificationDelivery>>();

        // --- Outbox & integration event mapping ---
        services.AddSingleton<ICorrelationContextAccessor, AsyncLocalCorrelationContext>();
        services.AddScoped<ICorrelationContext>(sp => new CorrelationContextAdapter(sp.GetRequiredService<ICorrelationContextAccessor>()));
        services.AddSingleton<IntegrationEventMapper>();
        services.AddHostedService<OutboxProcessor>();

        // --- RabbitMQ (commands) ---
        services.AddSingleton<RabbitMqConnectionProvider>();
        services.AddSingleton<ICommandPublisher, RabbitMqCommandPublisher>();
        services.AddHostedService<RabbitMqCommandConsumer>();

        // --- Kafka (events) ---
        services.AddSingleton<KafkaEventPublisher>();
        services.AddHostedService<KafkaEventConsumer>();

        // --- Redis ---
        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis") ?? "localhost:6379"));
        services.AddSingleton<ICacheService, RedisCacheService>();
        services.AddSingleton<ILockService, RedisLockService>();
        services.AddSingleton<IIdempotencyStore, RedisIdempotencyStore>();

        // --- MongoDB ---
        services.AddSingleton<IDocumentStore, MongoDocumentStore>();

        // --- Email ---
        services.AddSingleton<IEmailSender, SmtpEmailSender>();

        // --- AI providers ---
        var aiProvider = configuration["Ai:Provider"] ?? "Fake";
        if (string.Equals(aiProvider, "Ollama", StringComparison.OrdinalIgnoreCase))
        {
            services.AddHttpClient<OllamaAssistantProvider>();
            services.AddSingleton<IAssistantProvider>(sp =>
                sp.GetRequiredService<OllamaAssistantProvider>());
        }
        else
        {
            services.AddSingleton<IAssistantProvider, FakeAssistantProvider>();
        }

        // --- Firewall simulator (real vendor adapters plug in here later) ---
        services.AddSingleton<IFirewallProvider, SimulatedFirewallProvider>();

        // --- Security ---
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();

        return services;
    }

    private sealed class CorrelationContextAdapter(ICorrelationContextAccessor accessor) : ICorrelationContext
    {
        public Guid CorrelationId => accessor.CorrelationId;
    }
}