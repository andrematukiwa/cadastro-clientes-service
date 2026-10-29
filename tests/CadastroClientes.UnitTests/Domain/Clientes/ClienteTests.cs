using System.Globalization;

using CadastroClientes.Domain.Clientes;

namespace CadastroClientes.UnitTests.Domain.Clientes;

public class ClienteTests
{
    private static readonly DateTime Agora = new(2026, 10, 28, 12, 0, 0, DateTimeKind.Utc);

    private static Cliente Criar(
        string nome = "Maria Silva",
        string cpf = "52998224725",
        string email = "maria@email.com",
        decimal rendaMensal = 5000m,
        DateTime? criadoEm = null) =>
        Cliente.Criar(nome, cpf, email, new DateOnly(1990, 5, 10), rendaMensal, criadoEm ?? Agora);

    [Fact]
    public void Criar_DeveRemoverEspacosDoNome()
    {
        // Act
        var cliente = Criar(nome: "  Maria Silva  ");

        // Assert
        Assert.Equal("Maria Silva", cliente.Nome);
    }

    [Fact]
    public void Criar_DeveGuardarCpfApenasComDigitos()
    {
        // Act
        var cliente = Criar(cpf: "529.982.247-25");

        // Assert
        Assert.Equal("52998224725", cliente.Cpf);
    }

    [Fact]
    public void Criar_DeveGuardarEmailEmMinusculasESemEspacos()
    {
        // Act
        var cliente = Criar(email: " Maria@Email.COM ");

        // Assert
        Assert.Equal("maria@email.com", cliente.Email);
    }

    [Fact]
    public void Criar_DeveGravarRendaComDuasCasasDecimais()
    {
        // Act
        var cliente = Criar(rendaMensal: 5000m);

        // Assert: decimal compara só o valor (5000m == 5000.00m), por isso a escala é conferida pela string
        Assert.Equal("5000.00", cliente.RendaMensal.ToString(CultureInfo.InvariantCulture));
    }

    [Fact]
    public void Criar_DeveTruncarDataDeCriacaoEmMicrossegundos()
    {
        // Arrange: 5 ticks = 500 nanossegundos, abaixo da precisão do Postgres
        var criadoEm = Agora.AddTicks(5);

        // Act
        var cliente = Criar(criadoEm: criadoEm);

        // Assert
        Assert.Equal(Agora, cliente.CriadoEm);
    }

    [Fact]
    public void Criar_DeveGerarIdNovo()
    {
        // Act
        var cliente = Criar();

        // Assert
        Assert.NotEqual(Guid.Empty, cliente.Id);
    }
}
