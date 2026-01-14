using StackExchange.Redis;
using TODO_List.Application.Interfaces;
using TODO_List.Infrastructure.DepandancyInjections;

public static class InfrastructureDI
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var config = new ConfigurationOptions
            {
                EndPoints = { "localhost:6379" },
                ResolveDns = true,
                AbortOnConnectFail = false
            };
            return ConnectionMultiplexer.Connect(config);
        });
        services.AddScoped<IRedisService,RedisService>();
        services.Scan(scan => scan
            .FromAssemblyOf<InfrastructureAssemblyMarker>()
            .AddClasses(c => c.InNamespaces("TODO_List.Infrastructure.Repositories"))
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(c => c.InNamespaces("TODO_List.Infrastructure.Storage"))
                .AsSelf()
                .WithScopedLifetime()
            .AddClasses(c => c.InNamespaces("TODO_List.Infrastructure.NHibernateConfig"))
                .AsSelf()
                .WithSingletonLifetime()
            .AddClasses(c => c.InNamespaces("TODO_List.Infrastructure.Services"))
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );
        return services;
    }
}
