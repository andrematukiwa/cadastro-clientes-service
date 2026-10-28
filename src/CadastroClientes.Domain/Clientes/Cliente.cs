namespace CadastroClientes.Domain.Clientes;

public class Cliente
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Cpf { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public DateOnly DataNascimento { get; private set; }
    public decimal RendaMensal { get; private set; }
    public DateTime CriadoEm { get; private set; }

    private Cliente() { } // usado pelo EF Core

    /// <summary>
    /// Cria o cliente a partir de dados já validados pela camada de aplicação
    /// (CadastrarClienteValidator). Aqui só acontece a normalização.
    /// </summary>
    public static Cliente Criar(
        string nome,
        string cpf,
        string email,
        DateOnly dataNascimento,
        decimal rendaMensal)
    {
        return new Cliente
        {
            Id = Guid.NewGuid(),
            Nome = nome.Trim(),
            // "Common.Cpf" porque a propriedade Cpf desta classe esconde o nome da classe Cpf
            Cpf = Common.Cpf.Normalizar(cpf),
            Email = email.Trim().ToLowerInvariant(),
            DataNascimento = dataNascimento,
            RendaMensal = rendaMensal,
            CriadoEm = DateTime.UtcNow
        };
    }
}
