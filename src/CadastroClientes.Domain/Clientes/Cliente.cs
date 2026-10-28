namespace CadastroClientes.Domain.Clientes;

public sealed class Cliente
{
    // Limites compartilhados entre a validação de entrada e o mapeamento do banco
    public const int NomeTamanhoMaximo = 150;
    public const int EmailTamanhoMaximo = 254;
    public const int RendaPrecisao = 18;
    public const int RendaCasasDecimais = 2;

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
        decimal rendaMensal,
        DateTime criadoEm)
    {
        return new Cliente
        {
            Id = Guid.NewGuid(),
            Nome = nome.Trim(),
            // "Common.Cpf" porque a propriedade Cpf desta classe esconde o nome da classe Cpf
            Cpf = Common.Cpf.Normalizar(cpf),
            Email = NormalizarEmail(email),
            DataNascimento = dataNascimento,
            // Multiplicar por 1.00m fixa a escala em 2 casas (5000 → 5000.00), igual ao banco
            RendaMensal = decimal.Round(rendaMensal, RendaCasasDecimais) * 1.00m,
            // O Postgres guarda microssegundos: truncar aqui evita a resposta do POST divergir do GET
            CriadoEm = TruncarParaMicrossegundos(criadoEm)
        };
    }

    /// <summary>
    /// Forma canônica do e-mail (sem espaços, minúsculo). Usada ao gravar e ao checar duplicidade,
    /// para "Maria@Email.com" e "maria@email.com" serem o mesmo cliente.
    /// </summary>
    public static string NormalizarEmail(string email) => email.Trim().ToLowerInvariant();

    private static DateTime TruncarParaMicrossegundos(DateTime data) =>
        new(data.Ticks - data.Ticks % (TimeSpan.TicksPerMillisecond / 1000), DateTimeKind.Utc);
}
