using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CadastroClientes.Infrastructure.Persistence;

public sealed class CadastroDbContext(DbContextOptions<CadastroDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CadastroDbContext).Assembly);

        // Tabelas do Outbox do MassTransit: o evento é gravado na mesma transação do cliente
        // e só depois publicado no RabbitMQ, evitando cliente salvo sem evento (ou o contrário).
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}
