using CadastroClientes.Domain.Clientes;

namespace CadastroClientes.Application.Clientes.Repositories;

public interface IClienteRepository
{
    Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExisteComCpfAsync(string cpf, CancellationToken cancellationToken = default);

    Task AdicionarAsync(Cliente cliente, CancellationToken cancellationToken = default);
}
