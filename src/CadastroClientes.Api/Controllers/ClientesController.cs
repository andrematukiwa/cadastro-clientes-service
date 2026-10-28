using CadastroClientes.Application.Clientes.Dtos;
using CadastroClientes.Application.Clientes.Services;
using Microsoft.AspNetCore.Mvc;

namespace CadastroClientes.Api.Clientes;

[ApiController]
[Route("api/v1/clientes")]
[Produces("application/json")]
public sealed class ClientesController(IClienteService clienteService) : ControllerBase
{
    /// <summary>Cadastra um cliente e publica o evento ClienteCadastrado.</summary>
    [HttpPost]
    [ProducesResponseType<ClienteResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClienteResponse>> Cadastrar(
        CadastrarClienteRequest request,
        CancellationToken cancellationToken)
    {
        var cliente = await clienteService.CadastrarAsync(request, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id = cliente.Id }, cliente);
    }

    /// <summary>Consulta um cliente pelo ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ClienteResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClienteResponse>> ObterPorId(Guid id, CancellationToken cancellationToken) =>
        Ok(await clienteService.ObterPorIdAsync(id, cancellationToken));
}
