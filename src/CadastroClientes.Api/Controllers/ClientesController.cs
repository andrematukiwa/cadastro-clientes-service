using CadastroClientes.Application.Clientes.Dtos;
using CadastroClientes.Application.Clientes.Services;
using Microsoft.AspNetCore.Mvc;

namespace CadastroClientes.Api.Controllers;

[ApiController]
[Route("api/v1/clientes")]
public sealed class ClientesController(IClienteService clienteService) : ControllerBase
{
    private const string ProblemJson = "application/problem+json";

    /// <summary>Cadastra um cliente e publica o evento ClienteCadastrado.</summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType<ClienteResponse>(StatusCodes.Status201Created, "application/json")]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, ProblemJson)]
    public async Task<ActionResult<ClienteResponse>> Cadastrar(
        CadastrarClienteRequest request,
        CancellationToken cancellationToken)
    {
        var cliente = await clienteService.CadastrarAsync(request, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id = cliente.Id }, cliente);
    }

    /// <summary>Consulta um cliente pelo ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ClienteResponse>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, ProblemJson)]
    public async Task<ActionResult<ClienteResponse>> ObterPorId(Guid id, CancellationToken cancellationToken) =>
        Ok(await clienteService.ObterPorIdAsync(id, cancellationToken));
}
