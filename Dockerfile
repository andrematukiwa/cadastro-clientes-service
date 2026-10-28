# ---- build ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restaura só com os arquivos de projeto: esta camada fica em cache enquanto as dependências não mudarem
COPY Directory.Build.props Directory.Packages.props .editorconfig ./
COPY src/CadastroClientes.Api/CadastroClientes.Api.csproj src/CadastroClientes.Api/
COPY src/CadastroClientes.Application/CadastroClientes.Application.csproj src/CadastroClientes.Application/
COPY src/CadastroClientes.Domain/CadastroClientes.Domain.csproj src/CadastroClientes.Domain/
COPY src/CadastroClientes.Infrastructure/CadastroClientes.Infrastructure.csproj src/CadastroClientes.Infrastructure/
RUN dotnet restore src/CadastroClientes.Api/CadastroClientes.Api.csproj

COPY src/ src/
RUN dotnet publish src/CadastroClientes.Api/CadastroClientes.Api.csproj -c Release -o /app/publish --no-restore

# ---- runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Usuário sem privilégios já existente na imagem oficial
USER $APP_UID
EXPOSE 8080

ENTRYPOINT ["dotnet", "CadastroClientes.Api.dll"]
