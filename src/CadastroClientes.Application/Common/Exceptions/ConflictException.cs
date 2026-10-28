namespace CadastroClientes.Application.Common.Exceptions;

public sealed class ConflictException(string message) : AppException(message);
