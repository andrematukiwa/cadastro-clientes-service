using System.Text.RegularExpressions;

namespace CadastroClientes.Domain.Common;

/// <summary>
/// Regras do CPF: formato aceito, normalização (apenas dígitos) e validação dos dígitos verificadores.
/// </summary>
public static partial class Cpf
{
    public const int Tamanho = 11;

    public static string Normalizar(string? cpf) =>
        cpf is null ? string.Empty : new string(cpf.Where(char.IsDigit).ToArray());

    public static bool EhValido(string? cpf)
    {
        // Só aceita "52998224725" ou "529.982.247-25": qualquer outro caractere invalida,
        // em vez de ser descartado pela normalização
        if (cpf is null || !FormatoAceito().IsMatch(cpf))
            return false;

        var digitos = Normalizar(cpf);

        // CPFs com todos os dígitos iguais (111.111.111-11) passam no cálculo, mas são inválidos
        if (digitos.Distinct().Count() == 1)
            return false;

        var numeros = digitos.Select(c => c - '0').ToArray();

        return numeros[9] == CalcularDigito(numeros, 9)
            && numeros[10] == CalcularDigito(numeros, 10);
    }

    /// <summary>Oculta parte do CPF para exibição: 12345678909 → ***.456.789-**.</summary>
    public static string Mascarar(string cpf)
    {
        var digitos = Normalizar(cpf);
        return digitos.Length == Tamanho
            ? $"***.{digitos[3..6]}.{digitos[6..9]}-**"
            : "***";
    }

    /// <summary>
    /// Calcula o dígito verificador a partir dos primeiros <paramref name="quantidade"/> números:
    /// pesos decrescentes de (quantidade + 1) até 2, módulo 11.
    /// </summary>
    private static int CalcularDigito(int[] numeros, int quantidade)
    {
        var soma = 0;
        for (var i = 0; i < quantidade; i++)
            soma += numeros[i] * (quantidade + 1 - i);

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    [GeneratedRegex(@"^(\d{11}|\d{3}\.\d{3}\.\d{3}-\d{2})$")]
    private static partial Regex FormatoAceito();
}
