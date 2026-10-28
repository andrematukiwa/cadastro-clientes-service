using MassTransit;

namespace CadastroClientes.Application.Contracts;

/// <summary>
/// Evento publicado quando um cliente é cadastrado.
/// <para>
/// O contrato é identificado pela URN/exchange "cliente-cadastrado", e não pelo namespace C#:
/// cada microsserviço mantém a sua própria cópia deste record (sem pacote compartilhado),
/// e o roteamento continua funcionando desde que o nome e os campos coincidam.
/// </para>
/// <para>
/// Carrega só o necessário para a proposta (minimização de dados — LGPD): CPF e e-mail
/// não trafegam na mensageria.
/// </para>
/// </summary>
[MessageUrn("cliente-cadastrado")]
[EntityName("cliente-cadastrado")]
public sealed record ClienteCadastrado(
    Guid ClienteId,
    string Nome,
    DateOnly DataNascimento,
    decimal RendaMensal,
    DateTime CadastradoEm);
