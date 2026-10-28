namespace CadastroClientes.Application.Clientes.Dtos;

public sealed record CadastrarClienteRequest(
    string Nome,
    string Cpf,
    string Email,
    DateOnly DataNascimento,
    decimal RendaMensal);
