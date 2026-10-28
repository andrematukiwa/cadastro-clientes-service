namespace CadastroClientes.Application.Common.Abstractions;

/// <summary>
/// Publica eventos de integração. A implementação grava no Outbox: o evento só sai para o
/// RabbitMQ depois que <see cref="IUnitOfWork.SaveChangesAsync"/> confirma a transação.
/// </summary>
public interface IEventPublisher
{
    Task PublicarAsync<TEvento>(TEvento evento, CancellationToken cancellationToken = default)
        where TEvento : class;
}
