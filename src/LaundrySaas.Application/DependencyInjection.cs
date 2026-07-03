using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using LaundrySaas.SharedKernel.Interfaces;

namespace LaundrySaas.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Register Command Handlers
        var commandHandlerTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface && t.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>)));

        foreach (var type in commandHandlerTypes)
        {
            var interfaceType = type.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>));
            services.AddScoped(interfaceType, type);
        }

        // Register Query Handlers
        var queryHandlerTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface && t.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)));

        foreach (var type in queryHandlerTypes)
        {
            var interfaceType = type.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>));
            services.AddScoped(interfaceType, type);
        }

        // Register Domain Event Handlers
        var domainEventHandlerTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface && t.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>)));

        foreach (var type in domainEventHandlerTypes)
        {
            var interfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>));
            foreach (var @interface in interfaces)
            {
                services.AddScoped(@interface, type);
            }
        }

        return services;
    }
}
