using CadastroClientes.Domain.Clientes;

namespace CadastroClientes.Application.Clientes.Dtos;

public sealed record ClienteResponse(
    Guid Id,
    string Nome,
    string Cpf,
    string Email,
    DateOnly DataNascimento,
    decimal RendaMensal,
    DateTime CriadoEm)
{
    /// <summary>O CPF sai mascarado: a resposta não precisa expor o documento completo.</summary>
    public static ClienteResponse De(Cliente cliente) => new(
        cliente.Id,
        cliente.Nome,
        Domain.Common.Cpf.Mascarar(cliente.Cpf), // qualificado: a propriedade Cpf do record esconde a classe
        cliente.Email,
        cliente.DataNascimento,
        cliente.RendaMensal,
        cliente.CriadoEm);
}
