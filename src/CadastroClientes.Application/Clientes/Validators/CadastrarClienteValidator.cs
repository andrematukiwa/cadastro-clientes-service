using System.Net.Mail;
using CadastroClientes.Application.Clientes.Dtos;
using CadastroClientes.Domain.Clientes;
using CadastroClientes.Domain.Common;
using FluentValidation;

namespace CadastroClientes.Application.Clientes.Validators;

public sealed class CadastrarClienteValidator : AbstractValidator<CadastrarClienteRequest>
{
    public const int IdadeMinima = 18;
    public const int IdadeMaxima = 120;

    /// <param name="timeProvider">Fonte da data atual; nos testes é substituída para fixar o "hoje".</param>
    public CadastrarClienteValidator(TimeProvider timeProvider)
    {
        // Para no primeiro erro de cada campo (ex.: CPF vazio não acusa também "CPF inválido"),
        // mas continua validando os demais campos para devolver todos os erros de uma vez.
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MaximumLength(Cliente.NomeTamanhoMaximo)
            .WithMessage($"O nome deve ter no máximo {Cliente.NomeTamanhoMaximo} caracteres.");

        RuleFor(x => x.Cpf)
            .NotEmpty().WithMessage("O CPF é obrigatório.")
            .Must(Cpf.EhValido).WithMessage("O CPF informado é inválido.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O e-mail é obrigatório.")
            .MaximumLength(Cliente.EmailTamanhoMaximo)
            .WithMessage($"O e-mail deve ter no máximo {Cliente.EmailTamanhoMaximo} caracteres.")
            .Must(EmailValido).WithMessage("O e-mail informado é inválido.");

        RuleFor(x => x.DataNascimento)
            .NotEmpty().WithMessage("A data de nascimento é obrigatória.")
            .Must(data => CalcularIdade(data, Hoje(timeProvider)) >= IdadeMinima)
            .WithMessage($"O cliente deve ter pelo menos {IdadeMinima} anos.")
            .Must(data => CalcularIdade(data, Hoje(timeProvider)) <= IdadeMaxima)
            .WithMessage("A data de nascimento é inválida.");

        RuleFor(x => x.RendaMensal)
            .GreaterThan(0).WithMessage("A renda mensal deve ser maior que zero.")
            .PrecisionScale(Cliente.RendaPrecisao, Cliente.RendaCasasDecimais, ignoreTrailingZeros: true)
            .WithMessage($"A renda mensal deve ter no máximo {Cliente.RendaCasasDecimais} casas decimais.");
    }

    private static DateOnly Hoje(TimeProvider timeProvider) =>
        DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

    private static int CalcularIdade(DateOnly dataNascimento, DateOnly hoje)
    {
        var idade = hoje.Year - dataNascimento.Year;

        // Ainda não fez aniversário este ano
        if (dataNascimento > hoje.AddYears(-idade))
            idade--;

        return idade;
    }

    private static bool EmailValido(string email)
    {
        try
        {
            var endereco = new MailAddress(email.Trim());

            // MailAddress aceita "x@y": exige um domínio com ponto (ex.: email.com)
            return endereco.Address.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase)
                && endereco.Host.Contains('.', StringComparison.Ordinal)
                && !endereco.Host.StartsWith('.')
                && !endereco.Host.EndsWith('.');
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
