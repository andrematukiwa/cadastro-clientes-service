using CadastroClientes.Application.Clientes.Dtos;
using CadastroClientes.Application.Clientes.Repositories;
using CadastroClientes.Application.Common.Abstractions;
using CadastroClientes.Application.Common.Exceptions;
using CadastroClientes.Application.Common.Validation;
using CadastroClientes.Application.Contracts;
using CadastroClientes.Domain.Clientes;
using CadastroClientes.Domain.Common;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CadastroClientes.Application.Clientes.Services;

public sealed partial class ClienteService(
    IValidator<CadastrarClienteRequest> validator,
    IClienteRepository repository,
    IEventPublisher eventPublisher,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger<ClienteService> logger) : IClienteService
{
    public async Task<ClienteResponse> CadastrarAsync(
        CadastrarClienteRequest request,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateOrThrowAsync(request, cancellationToken);

        if (await repository.ExisteComCpfAsync(Cpf.Normalizar(request.Cpf), cancellationToken))
            throw new ConflictException("Já existe um cliente cadastrado com este CPF.");

        var cliente = Cliente.Criar(
            request.Nome,
            request.Cpf,
            request.Email,
            request.DataNascimento,
            request.RendaMensal,
            timeProvider.GetUtcNow().UtcDateTime);

        repository.Adicionar(cliente);
        await eventPublisher.PublishAsync(
            new ClienteCadastrado(
                cliente.Id,
                cliente.Nome,
                cliente.DataNascimento,
                cliente.RendaMensal,
                cliente.CriadoEm),
            cancellationToken);

        // Cliente e evento (Outbox) são gravados na mesma transação
        await unitOfWork.SaveChangesAsync(cancellationToken);

        LogClienteCadastrado(logger, cliente.Id);

        return ClienteResponse.De(cliente);
    }

    public async Task<ClienteResponse> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cliente = await repository.ObterPorIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Cliente {id} não encontrado.");

        return ClienteResponse.De(cliente);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Cliente {ClienteId} cadastrado")]
    private static partial void LogClienteCadastrado(ILogger logger, Guid clienteId);
}
