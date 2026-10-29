using CadastroClientes.Domain.Common;

namespace CadastroClientes.UnitTests.Domain.Common;

public class CpfTests
{
    [Theory]
    [InlineData("52998224725")]
    [InlineData("529.982.247-25")]
    public void EhValido_DeveRetornarTrue_QuandoCpfForValido(string cpf)
    {
        // Act
        var resultado = Cpf.EhValido(cpf);

        // Assert
        Assert.True(resultado);
    }

    [Theory]
    [InlineData("52998224724")]     // dígito verificador errado
    [InlineData("11111111111")]     // todos iguais: passa no cálculo, mas é inválido
    [InlineData("529.982.24725")]   // formato misturado
    [InlineData("5299822472")]      // 10 dígitos
    [InlineData("529 982 247 25")]  // caractere fora do formato aceito
    [InlineData("")]
    [InlineData(null)]
    public void EhValido_DeveRetornarFalse_QuandoCpfForInvalido(string? cpf)
    {
        // Act
        var resultado = Cpf.EhValido(cpf);

        // Assert
        Assert.False(resultado);
    }

    [Theory]
    [InlineData("529.982.247-25", "52998224725")]
    [InlineData("52998224725", "52998224725")]
    [InlineData(null, "")]
    public void Normalizar_DeveManterApenasOsDigitos(string? cpf, string esperado)
    {
        // Act
        var resultado = Cpf.Normalizar(cpf);

        // Assert
        Assert.Equal(esperado, resultado);
    }

    [Fact]
    public void Mascarar_DeveOcultarInicioEDigitosVerificadores()
    {
        // Act
        var resultado = Cpf.Mascarar("52998224725");

        // Assert
        Assert.Equal("***.982.247-**", resultado);
    }

    [Fact]
    public void Mascarar_NaoDeveExporNada_QuandoCpfTiverTamanhoInvalido()
    {
        // Act
        var resultado = Cpf.Mascarar("123");

        // Assert
        Assert.Equal("***", resultado);
    }
}
