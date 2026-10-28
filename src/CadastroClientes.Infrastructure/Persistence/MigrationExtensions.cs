using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CadastroClientes.Infrastructure.Persistence;

public static class MigrationExtensions
{
    /// <summary>
    /// Aplica as migrations pendentes. Usado no ambiente local/docker para o projeto subir com
    /// um único comando; em produção as migrations rodariam no pipeline de deploy.
    /// </summary>
    public static async Task AplicarMigrationsAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<CadastroDbContext>();
        await context.Database.MigrateAsync();
    }
}
