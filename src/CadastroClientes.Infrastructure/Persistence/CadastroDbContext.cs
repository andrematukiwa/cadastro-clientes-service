using CadastroClientes.Application.Common.Abstractions;
using CadastroClientes.Application.Common.Exceptions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CadastroClientes.Infrastructure.Persistence;

public sealed class CadastroDbContext(DbContextOptions<CadastroDbContext> options) : DbContext(options), IUnitOfWork
{
    async Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // Corrida entre a checagem prévia e o insert: o índice único é a garantia final
            throw new ConflictException("Registro duplicado: já existe um cadastro com estes dados.");
        }
    }

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
