using FluentValidation;
using ValidationException = CadastroClientes.Application.Common.Exceptions.ValidationException;

namespace CadastroClientes.Application.Common.Validation;

public static class ValidatorExtensions
{
    /// <summary>
    /// Valida e, se houver erros, lança a ValidationException da aplicação (traduzida para 400
    /// pelo handler global) com todos os erros agrupados por campo.
    /// </summary>
    public static async Task ValidateOrThrowAsync<T>(
        this IValidator<T> validator,
        T instance,
        CancellationToken cancellationToken = default)
    {
        var result = await validator.ValidateAsync(instance, cancellationToken);

        if (!result.IsValid)
            throw new ValidationException(new Dictionary<string, string[]>(result.ToDictionary()));
    }
}
