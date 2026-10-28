using CadastroClientes.Application.Clientes.Repositories;
using CadastroClientes.Application.Common.Abstractions;
using CadastroClientes.Infrastructure.Clientes;
using CadastroClientes.Infrastructure.Messaging;
using CadastroClientes.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CadastroClientes.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CadastroDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("CadastroDb")));

        // Mesma instância (por requisição) do DbContext: repositórios e Outbox na mesma transação
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CadastroDbContext>());
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

        services.AddHealthChecks()
            .AddDbContextCheck<CadastroDbContext>("postgres");

        services.AddMessaging(configuration);

        return services;
    }

    private static void AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitMq = configuration.GetSection(RabbitMqOptions.SectionName).Get<RabbitMqOptions>()
            ?? new RabbitMqOptions();

        services.AddMassTransit(bus =>
        {
            bus.AddEntityFrameworkOutbox<CadastroDbContext>(outbox =>
            {
                outbox.UsePostgres();
                outbox.UseBusOutbox();
            });

            bus.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitMq.Host, rabbitMq.Port, rabbitMq.VirtualHost, host =>
                {
                    host.Username(rabbitMq.Username);
                    host.Password(rabbitMq.Password);
                });

                cfg.ConfigureEndpoints(context);
            });
        });
    }
}
