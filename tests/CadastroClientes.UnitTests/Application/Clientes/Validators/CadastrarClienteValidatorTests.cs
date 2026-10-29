using System.Globalization;

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
    public void Validate_DeveRetornarErroEmTodosOsCampos_QuandoTodosForemInvalidos()
    {
        // Arrange: o 400 da API devolve todos os erros de uma vez, não só o primeiro
        var request = new CadastrarClienteRequest(
            Nome: "",
            Cpf: "12345678900",
            Email: "maria@email",
            DataNascimento: new DateOnly(2015, 1, 10),
            RendaMensal: 0m);

        // Act
        var resultado = _validator.TestValidate(request);

        // Assert
        resultado.ShouldHaveValidationErrorFor(x => x.Nome);
        resultado.ShouldHaveValidationErrorFor(x => x.Cpf);
        resultado.ShouldHaveValidationErrorFor(x => x.Email);
        resultado.ShouldHaveValidationErrorFor(x => x.DataNascimento);
        resultado.ShouldHaveValidationErrorFor(x => x.RendaMensal);
    }

    // ---------- Nome ----------

    [Fact]
    public void Validate_DeveAceitarNome_QuandoTiver150Caracteres()
    {
        // Arrange
        var request = RequestValido() with { Nome = new string('a', 150) };

        // Act
        var resultado = _validator.TestValidate(request);

        // Assert
        resultado.ShouldNotHaveValidationErrorFor(x => x.Nome);
    }

    [Fact]
    public void Validate_DeveRetornarErro_QuandoNomeForVazio()
    {
        // Arrange
        var request = RequestValido() with { Nome = "" };

        // Act
        var resultado = _validator.TestValidate(request);

        // Assert
        resultado.ShouldHaveValidationErrorFor(x => x.Nome).WithErrorMessage("O nome é obrigatório.");
    }

    [Fact]
    public void Validate_DeveRetornarErro_QuandoNomeTiver151Caracteres()
    {
        // Arrange
        var request = RequestValido() with { Nome = new string('a', 151) };

        // Act
        var resultado = _validator.TestValidate(request);

        // Assert
        resultado.ShouldHaveValidationErrorFor(x => x.Nome)
            .WithErrorMessage("O nome deve ter no máximo 150 caracteres.");
    }

    // ---------- CPF ----------

    [Fact]
    public void Validate_DeveAceitarCpf_QuandoEstiverFormatado()
    {
        // Arrange
        var request = RequestValido() with { Cpf = "529.982.247-25" };

        // Act
        var resultado = _validator.TestValidate(request);

        // Assert
        resultado.ShouldNotHaveValidationErrorFor(x => x.Cpf);
    }

    [Fact]
    public void Validate_DeveRetornarSoErroDeObrigatorio_QuandoCpfForVazio()
    {
        // Arrange
        var request = RequestValido() with { Cpf = "" };

        // Act
        var resultado = _validator.TestValidate(request);

        // Assert: CascadeMode.Stop impede que "CPF inválido" apareça junto com "obrigatório"
        resultado.ShouldHaveValidationErrorFor(x => x.Cpf).WithErrorMessage("O CPF é obrigatório.");
        Assert.Single(resultado.Errors, e => e.PropertyName == nameof(CadastrarClienteRequest.Cpf));
    }

    [Fact]
    public void Validate_DeveRetornarErro_QuandoCpfForInvalido()
    {
        // Arrange
        var request = RequestValido() with { Cpf = "12345678900" };

        // Act
        var resultado = _validator.TestValidate(request);

        // Assert
        resultado.ShouldHaveValidationErrorFor(x => x.Cpf).WithErrorMessage("O CPF informado é inválido.");
    }

    // ---------- E-mail ----------

    [Theory]
    [InlineData("maria@email")]               // domínio sem ponto
    [InlineData("Maria <maria@email.com>")]   // nome de exibição junto do endereço
    [InlineData("maria.email.com")]           // sem @
    public void Validate_DeveRetornarErro_QuandoEmailForInvalido(string email)
    {
        // Arrange
        var request = RequestValido() with { Email = email };

        // Act
        var resultado = _validator.TestValidate(request);

        // Assert
        resultado.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage("O e-mail informado é inválido.");
    }

    [Fact]
    public void Validate_DeveRetornarErro_QuandoEmailTiver255Caracteres()
    {
        // Arrange: 245 caracteres + "@email.com" (10) = 255
        var request = RequestValido() with { Email = new string('a', 245) + "@email.com" };

        // Act
        var resultado = _validator.TestValidate(request);

        // Assert
        resultado.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("O e-mail deve ter no máximo 254 caracteres.");
    }

    // ---------- Data de nascimento ----------

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

    [Fact]
    public void Validate_DeveRetornarErro_QuandoClienteFizer18AnosAmanha()
    {
        // Arrange
        var request = RequestValido() with { DataNascimento = new DateOnly(2008, 10, 29) };

        // Act
        var resultado = _validator.TestValidate(request);

        // Assert
        resultado.ShouldHaveValidationErrorFor(x => x.DataNascimento)
            .WithErrorMessage("O cliente deve ter pelo menos 18 anos.");
    }

    [Fact]
    public void Validate_DeveAceitar_QuandoClienteFizer120AnosHoje()
    {
        // Arrange
        var request = RequestValido() with { DataNascimento = new DateOnly(1906, 10, 28) };

        // Act
        var resultado = _validator.TestValidate(request);

        // Assert
        resultado.ShouldNotHaveValidationErrorFor(x => x.DataNascimento);
    }

    [Fact]
    public void Validate_DeveRetornarErro_QuandoClienteTiver121Anos()
    {
        // Arrange
        var request = RequestValido() with { DataNascimento = new DateOnly(1905, 10, 28) };

        // Act
        var resultado = _validator.TestValidate(request);

        // Assert
        resultado.ShouldHaveValidationErrorFor(x => x.DataNascimento)
            .WithErrorMessage("A data de nascimento é inválida.");
    }

    [Fact]
    public void Validate_DeveRetornarErro_QuandoDataDeNascimentoNaoForInformada()
    {
        // Arrange
        var request = RequestValido() with { DataNascimento = default };

        // Act
        var resultado = _validator.TestValidate(request);

        // Assert
        resultado.ShouldHaveValidationErrorFor(x => x.DataNascimento)
            .WithErrorMessage("A data de nascimento é obrigatória.");
    }

    // ---------- Renda mensal ----------
    // Renda como string porque [InlineData] não aceita decimal

    [Theory]
    [InlineData("0.01")]      // menor valor positivo
    [InlineData("1500.500")]  // zero à direita não conta como terceira casa decimal
    public void Validate_DeveAceitarRenda_QuandoForPositivaComAteDuasCasas(string renda)
    {
        // Arrange
        var request = RequestValido() with { RendaMensal = decimal.Parse(renda, CultureInfo.InvariantCulture) };

        // Act
        var resultado = _validator.TestValidate(request);

        // Assert
        resultado.ShouldNotHaveValidationErrorFor(x => x.RendaMensal);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    public void Validate_DeveRetornarErro_QuandoRendaNaoForPositiva(string renda)
    {
        // Arrange
        var request = RequestValido() with { RendaMensal = decimal.Parse(renda, CultureInfo.InvariantCulture) };

        // Act
        var resultado = _validator.TestValidate(request);

        // Assert
        resultado.ShouldHaveValidationErrorFor(x => x.RendaMensal)
            .WithErrorMessage("A renda mensal deve ser maior que zero.");
    }

    [Fact]
    public void Validate_DeveRetornarErro_QuandoRendaTiverTresCasasDecimais()
    {
        // Arrange
        var request = RequestValido() with { RendaMensal = 1500.555m };

        // Act
        var resultado = _validator.TestValidate(request);

        // Assert
        resultado.ShouldHaveValidationErrorFor(x => x.RendaMensal)
            .WithErrorMessage("A renda mensal deve ter no máximo 2 casas decimais.");
    }
}
