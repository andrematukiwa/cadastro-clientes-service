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
}
