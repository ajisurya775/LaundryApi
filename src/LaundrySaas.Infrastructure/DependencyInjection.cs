using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using LaundrySaas.Application.Identity.Services;
using LaundrySaas.Infrastructure.Authentication;
using LaundrySaas.Infrastructure.Persistence;
using LaundrySaas.Infrastructure.Persistence.MultiTenancy;
using LaundrySaas.Infrastructure.Persistence.Queries;
using LaundrySaas.Infrastructure.Persistence.Repositories;
using LaundrySaas.SharedKernel.Interfaces;
using LaundrySaas.SharedKernel.MultiTenancy;

namespace LaundrySaas.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Database Context
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("PostgresConnection")));

        // 2. Multi-tenancy Providers
        services.AddScoped<ITenantProvider, TenantProvider>();
        services.AddScoped<IBranchProvider, BranchProvider>();

        // 3. Authentication & Tokens
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddScoped<ITokenService, TokenService>();
        services.AddSingleton<IFirebaseAuthService, FirebaseAuthService>();

        // 4. Domain Events & Unit of Work
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // 5. Query Services
        services.AddScoped<IAuthQueryService, AuthQueryService>();

        // 6. Register all Repositories automatically
        var assembly = Assembly.GetExecutingAssembly();
        var repositoryTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface && t.Name.EndsWith("Repository"));

        foreach (var type in repositoryTypes)
        {
            var interfaceType = type.GetInterfaces().FirstOrDefault(i => i.Name == "I" + type.Name);
            if (interfaceType != null)
            {
                services.AddScoped(interfaceType, type);
            }
        }

        return services;
    }
}
