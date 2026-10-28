using CadastroClientes.Application.Clientes.Dtos;

namespace CadastroClientes.Application.Clientes.Services;

public interface IClienteService
{
    Task<ClienteResponse> CadastrarAsync(CadastrarClienteRequest request, CancellationToken cancellationToken = default);

    Task<ClienteResponse> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
}
