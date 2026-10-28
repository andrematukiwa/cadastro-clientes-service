using System.Text.Json;
using CadastroClientes.Application.Clientes.Services;
using CadastroClientes.Application.Clientes.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CadastroClientes.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddValidatorsFromAssemblyContaining<CadastrarClienteValidator>();

        // Erros de validação com o nome do campo igual ao JSON (rendaMensal, não RendaMensal)
        ValidatorOptions.Global.PropertyNameResolver = (_, member, _) =>
            member is null ? null : JsonNamingPolicy.CamelCase.ConvertName(member.Name);

        services.AddScoped<IClienteService, ClienteService>();

        return services;
    }
}
