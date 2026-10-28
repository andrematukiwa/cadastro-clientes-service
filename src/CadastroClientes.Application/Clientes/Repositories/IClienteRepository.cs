using CadastroClientes.Domain.Clientes;

namespace CadastroClientes.Application.Clientes.Repositories;

public interface IClienteRepository
{
    Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExisteComCpfAsync(string cpf, CancellationToken cancellationToken = default);

    Task<bool> ExisteComEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>Marca o cliente para inclusão; a gravação acontece em <c>IUnitOfWork.SaveChangesAsync</c>.</summary>
    void Adicionar(Cliente cliente);
}
