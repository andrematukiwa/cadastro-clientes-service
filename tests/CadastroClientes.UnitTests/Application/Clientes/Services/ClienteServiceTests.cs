using CadastroClientes.Application.Clientes.Dtos;
using CadastroClientes.Application.Clientes.Repositories;
using CadastroClientes.Application.Clientes.Services;
using CadastroClientes.Application.Clientes.Validators;
using CadastroClientes.Application.Common.Abstractions;
using CadastroClientes.Application.Common.Exceptions;
using CadastroClientes.Application.Contracts;
using CadastroClientes.Domain.Clientes;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;

using NSubstitute;

namespace CadastroClientes.UnitTests.Application.Clientes.Services;

public class ClienteServiceTests
{
    private static readonly DateTimeOffset Agora = new(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);

    private readonly IClienteRepository _repository = Substitute.For<IClienteRepository>();
    private readonly IEventPublisher _eventPublisher = Substitute.For<IEventPublisher>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ClienteService _service;

    // xUnit cria uma instância nova da classe por teste: os mocks nunca vazam entre testes.
    // O validator é o real: mockar ele testaria só o mock, e ele já tem testes próprios.
    public ClienteServiceTests()
    {
        var timeProvider = new FakeTimeProvider(Agora);
        _service = new ClienteService(
            new CadastrarClienteValidator(timeProvider),
            _repository,
            _eventPublisher,
            _unitOfWork,
            timeProvider,
            NullLogger<ClienteService>.Instance);
    }

    private static CadastrarClienteRequest RequestValido() => new(
        Nome: "Maria Silva",
        Cpf: "529.982.247-25",
        Email: "Maria@Email.com",
        DataNascimento: new DateOnly(1990, 5, 10),
        RendaMensal: 5000m);

    [Fact]
    public async Task CadastrarAsync_DeveSalvarClienteEPublicarEvento_QuandoRequestForValido()
    {
        // Arrange: sem configuração, ExisteComCpf/ExisteComEmail devolvem false
        var request = RequestValido();

        // Act
        var resposta = await _service.CadastrarAsync(request);

        // Assert: cliente e evento entram juntos e um único SaveChanges confirma os dois (Outbox)
        _repository.Received(1).Adicionar(Arg.Is<Cliente>(c => c.Id == resposta.Id && c.Cpf == "52998224725"));
        await _eventPublisher.Received(1).PublishAsync(
            Arg.Is<ClienteCadastrado>(e => e.ClienteId == resposta.Id && e.RendaMensal == 5000m),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CadastrarAsync_DeveDevolverCpfMascaradoEEmailNormalizado()
    {
        // Act
        var resposta = await _service.CadastrarAsync(RequestValido());

        // Assert
        Assert.Equal("***.982.247-**", resposta.Cpf);
        Assert.Equal("maria@email.com", resposta.Email);
        Assert.Equal(Agora.UtcDateTime, resposta.CriadoEm);
    }

    [Fact]
    public async Task CadastrarAsync_DeveLancarConflict_QuandoCpfJaExistir()
    {
        // Arrange: o repositório recebe o CPF já normalizado (só dígitos)
        _repository.ExisteComCpfAsync("52998224725", Arg.Any<CancellationToken>()).Returns(true);

        // Act
        var acao = () => _service.CadastrarAsync(RequestValido());

        // Assert
        var excecao = await Assert.ThrowsAsync<ConflictException>(acao);
        Assert.Equal("Já existe um cliente cadastrado com este CPF.", excecao.Message);
        _repository.DidNotReceive().Adicionar(Arg.Any<Cliente>());
        await _eventPublisher.DidNotReceive().PublishAsync(Arg.Any<ClienteCadastrado>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CadastrarAsync_DeveLancarConflict_QuandoEmailJaExistir()
    {
        // Arrange: o repositório recebe o e-mail já normalizado ("Maria@Email.com" → "maria@email.com")
        _repository.ExisteComEmailAsync("maria@email.com", Arg.Any<CancellationToken>()).Returns(true);

        // Act
        var acao = () => _service.CadastrarAsync(RequestValido());

        // Assert
        var excecao = await Assert.ThrowsAsync<ConflictException>(acao);
        Assert.Equal("Já existe um cliente cadastrado com este e-mail.", excecao.Message);
        _repository.DidNotReceive().Adicionar(Arg.Any<Cliente>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CadastrarAsync_DeveLancarValidationException_QuandoRequestForInvalido()
    {
        // Arrange
        var request = RequestValido() with { Cpf = "123" };

        // Act
        var acao = () => _service.CadastrarAsync(request);

        // Assert: a validação vem antes de qualquer consulta ao banco
        var excecao = await Assert.ThrowsAsync<ValidationException>(acao);
        Assert.True(excecao.Errors.ContainsKey(nameof(CadastrarClienteRequest.Cpf)));
        await _repository.DidNotReceive().ExisteComCpfAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveDevolverCliente_QuandoExistir()
    {
        // Arrange
        var cliente = Cliente.Criar(
            "Maria Silva", "52998224725", "maria@email.com", new DateOnly(1990, 5, 10), 5000m, Agora.UtcDateTime);
        _repository.ObterPorIdAsync(cliente.Id, Arg.Any<CancellationToken>()).Returns(cliente);

        // Act
        var resposta = await _service.ObterPorIdAsync(cliente.Id);

        // Assert
        Assert.Equal(cliente.Id, resposta.Id);
        Assert.Equal("Maria Silva", resposta.Nome);
        Assert.Equal("***.982.247-**", resposta.Cpf);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveLancarNotFound_QuandoClienteNaoExistir()
    {
        // Arrange: sem configuração, o mock devolve null
        var id = Guid.NewGuid();

        // Act
        var acao = () => _service.ObterPorIdAsync(id);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(acao);
    }
}
