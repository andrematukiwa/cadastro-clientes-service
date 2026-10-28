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
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } postgres)
        {
            // Corrida entre a checagem prévia e o insert: o índice único é a garantia final
            throw new ConflictException(MensagemDeDuplicidade(postgres.ConstraintName));
        }
    }

    /// <summary>Usa o nome do índice violado para dizer qual campo está duplicado.</summary>
    private static string MensagemDeDuplicidade(string? constraint) => constraint switch
    {
        "ix_clientes_cpf" => "Já existe um cliente cadastrado com este CPF.",
        "ix_clientes_email" => "Já existe um cliente cadastrado com este e-mail.",
        _ => "Registro duplicado: já existe um cadastro com estes dados."
    };

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
