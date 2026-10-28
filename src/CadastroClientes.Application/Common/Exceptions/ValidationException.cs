namespace CadastroClientes.Application.Common.Exceptions;

public sealed class ValidationException(IReadOnlyDictionary<string, string[]> errors)
    : AppException("Um ou mais campos são inválidos.")
{
    /// <summary>Erros por campo, no mesmo formato do ValidationProblemDetails do ASP.NET.</summary>
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}
