using TODO_List.Application.DependancyInjection;

namespace TODO_List.Application.DependencyInjection
{
    public static class ApplicationDI
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.Scan(scan => scan
                .FromAssemblyOf<ApplicationAssemblyMarker>()
                .AddClasses(c => c.InNamespaces("TODO_List.Application.ValidatorFastEndpoint"))
                    .AsSelf()
                    .WithTransientLifetime()
                .AddClasses(c => c.InNamespaces("TODO_List.Application.Interfaces"))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
            );
            return services;
        }
    }
}
