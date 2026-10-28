using CadastroClientes.Api.Errors;
using CadastroClientes.Application;
using CadastroClientes.Infrastructure;
using CadastroClientes.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddErrorHandling();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
    await app.Services.ApplyMigrationsAsync();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Cadastro de Clientes v1"));
}

app.MapControllers();
app.MapHealthChecks("/health");

await app.RunAsync();
