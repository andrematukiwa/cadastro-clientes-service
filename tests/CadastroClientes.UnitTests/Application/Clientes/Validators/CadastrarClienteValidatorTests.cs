using CadastroClientes.Application.Clientes.Dtos;
using CadastroClientes.Application.Clientes.Validators;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Time.Testing;

namespace CadastroClientes.UnitTests.Application.Clientes.Validators;

public class CadastrarClienteValidatorTests
{
    // "Hoje" fixo: o resultado dos testes de idade não depende do dia em que rodam
    private static readonly DateTimeOffset Hoje = new(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);

    private readonly CadastrarClienteValidator _validator = new(new FakeTimeProvider(Hoje));

    private static CadastrarClienteRequest RequestValido() => new(
        Nome: "Maria Silva",
        Cpf: "52998224725",
        Email: "maria@email.com",
        DataNascimento: new DateOnly(1990, 5, 10),
        RendaMensal: 5000m);

    [Fact]
    public void Validate_NaoDeveRetornarErros_QuandoRequestForValido()
    {
        // Arrange
        var request = RequestValido();

        // Act
        var resultado = _validator.TestValidate(request);

        // Assert
        resultado.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_DeveAceitar_QuandoClienteFizer18AnosHoje()
    {
        // Arrange: "with" copia o request válido trocando só o campo do cenário
        var request = RequestValido() with { DataNascimento = new DateOnly(2008, 10, 28) };

        // Act
        var resultado = _validator.TestValidate(request);

        // Assert
        resultado.ShouldNotHaveValidationErrorFor(x => x.DataNascimento);
    }
}
