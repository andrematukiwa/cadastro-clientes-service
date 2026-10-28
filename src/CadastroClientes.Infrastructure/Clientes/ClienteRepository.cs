using CadastroClientes.Application.Clientes.Repositories;
using CadastroClientes.Domain.Clientes;
using CadastroClientes.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CadastroClientes.Infrastructure.Clientes;

internal sealed class ClienteRepository(CadastroDbContext context) : IClienteRepository
{
    public Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Set<Cliente>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<bool> ExisteComCpfAsync(string cpf, CancellationToken cancellationToken = default) =>
        context.Set<Cliente>().AnyAsync(c => c.Cpf == cpf, cancellationToken);

    public void Adicionar(Cliente cliente) => context.Set<Cliente>().Add(cliente);
}
