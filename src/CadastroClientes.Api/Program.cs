using CadastroClientes.Api.Middlewares;
using CadastroClientes.Application;
using CadastroClientes.Infrastructure;
using CadastroClientes.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Configuration.GetValue<bool>("Database:AplicarMigrationsNoStartup"))
    await app.Services.AplicarMigrationsAsync();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Cadastro de Clientes v1"));
}

app.MapControllers();
app.MapHealthChecks("/health");

await app.RunAsync();
