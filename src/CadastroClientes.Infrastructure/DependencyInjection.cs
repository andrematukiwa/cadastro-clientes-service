using CadastroClientes.Application.Clientes.Repositories;
using CadastroClientes.Application.Common.Abstractions;
using CadastroClientes.Infrastructure.Clientes;
using CadastroClientes.Infrastructure.Messaging;
using CadastroClientes.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CadastroClientes.Infrastructure;

public static class DependencyInjection
{
    private const string ConnectionStringName = "CadastroDb";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPersistence(configuration);
        services.AddMessaging(configuration);

        return services;
    }

    private static void AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        // Falha ao subir se a connection string não estiver configurada (fail-fast)
        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string '{ConnectionStringName}' não configurada (ConnectionStrings__{ConnectionStringName}).");

        services.AddDbContext<CadastroDbContext>(options => options
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention());

        // Mesma instância (por requisição) do DbContext: repositórios e Outbox na mesma transação
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CadastroDbContext>());
        services.AddScoped<IClienteRepository, ClienteRepository>();

        services.AddHealthChecks()
            .AddDbContextCheck<CadastroDbContext>("postgres");
    }

    private static void AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<RabbitMqOptions>()
            .Bind(configuration.GetSection(RabbitMqOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

        services.AddMassTransit(bus =>
        {
            bus.AddEntityFrameworkOutbox<CadastroDbContext>(outbox =>
            {
                outbox.UsePostgres();
                outbox.UseBusOutbox();
            });

            bus.UsingRabbitMq((context, cfg) =>
            {
                var rabbitMq = context.GetRequiredService<IOptions<RabbitMqOptions>>().Value;

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
