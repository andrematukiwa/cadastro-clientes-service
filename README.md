# Cadastro de Clientes

Microsserviço responsável por **cadastrar clientes via API REST** e **publicar o evento `ClienteCadastrado`** no RabbitMQ. É o ponto de entrada do fluxo de concessão de crédito:

```
Cadastro de Clientes ──ClienteCadastrado──▶ Proposta de Crédito ──PropostaGerada──▶ Cartão de Crédito
```

![Arquitetura da solução](docs/arquitetura.png)

O [fluxo detalhado](docs/fluxo-detalhado.png) mostra as decisões de cada serviço: validações, idempotência, retry e DLQ. Para editar os diagramas, abra a fonte no draw.io: [docs/fluxograma.drawio](docs/fluxograma.drawio).

## Stack

| | |
|---|---|
| Plataforma | .NET 10 (ASP.NET Core Web API) |
| Banco | PostgreSQL 17 + EF Core 10 (migrations, `snake_case`) |
| Mensageria | RabbitMQ 4.1 + MassTransit 8 com **Transactional Outbox** |
| Validação | FluentValidation |
| Testes | xUnit |
| Documentação da API | OpenAPI + Swagger UI |

## Como executar

### Pré-requisitos

- [Docker](https://www.docker.com/) — para rodar tudo em containers
- [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0) — apenas para rodar fora do Docker ou executar os testes

### Fluxo completo (Cadastro → Proposta → Cartão)

Os serviços trocam eventos por um **RabbitMQ único**. Com os repositórios clonados **lado a lado na mesma pasta**:

```
pasta/
├── cadastro-clientes-service
├── proposta-credito-service
└── cartao-credito-service
```

rode a partir deste repositório:

```bash
docker compose -f docker-compose.fluxo-completo.yml up -d --build
```

| Serviço | Swagger |
|---|---|
| Cadastro de Clientes | http://localhost:8081/swagger |
| Proposta de Crédito | http://localhost:8082/swagger |
| Cartão de Crédito | http://localhost:8083/swagger |
| Painel do RabbitMQ | http://localhost:15672 (guest / guest) |

Cadastre um cliente e, com o `id` devolvido, consulte `GET http://localhost:8082/api/v1/propostas/{id}` e `GET http://localhost:8083/api/v1/cartoes/{id}`.

### Opção 1 — só este serviço no Docker

```bash
docker compose up -d --build
```

Sobe a API, o PostgreSQL e o RabbitMQ. As migrations são aplicadas automaticamente na inicialização. Rode um compose de cada vez: os dois usam as mesmas portas.

| Recurso | Endereço |
|---|---|
| Swagger | http://localhost:8081/swagger |
| Health check | http://localhost:8081/health |
| Painel do RabbitMQ | http://localhost:15672 (guest / guest) |

### Opção 2 — API local, infraestrutura no Docker

```bash
docker compose up -d postgres rabbitmq
dotnet run --project src/CadastroClientes.Api
```

A API sobe em http://localhost:5138 e abre o Swagger. O arquivo [`CadastroClientes.Api.http`](src/CadastroClientes.Api/CadastroClientes.Api.http) traz requisições prontas para testar o fluxo pelo VS Code ou pelo Visual Studio.

### Testes

```bash
dotnet test
```

Os testes de unidade cobrem a regra do CPF (dígitos verificadores, formato e máscara), a normalização do cliente, cada limite do validador dos dois lados (nome, idade de 18 e 120 anos, e-mail, renda e casas decimais) e o serviço de cadastro: gravação com publicação do evento, CPF e e-mail duplicados e request inválido. Repositório, publisher e unit of work são substituídos por mocks (NSubstitute), e o relógio por `FakeTimeProvider`.

## Endpoints

| Método | Rota | Descrição | Respostas |
|---|---|---|---|
| `POST` | `/api/v1/clientes` | Cadastra um cliente e publica `ClienteCadastrado` | `201` · `400` · `409` · `415` |
| `GET` | `/api/v1/clientes/{id}` | Consulta um cliente pelo ID | `200` · `404` |

### Exemplo

```http
POST /api/v1/clientes
Content-Type: application/json

{
  "nome": "Maria Silva",
  "cpf": "529.982.247-25",
  "email": "maria.silva@email.com",
  "dataNascimento": "1990-05-10",
  "rendaMensal": 5000.00
}
```

```http
HTTP/1.1 201 Created
Location: /api/v1/clientes/8339abb5-c9c3-4a91-a276-db5222375216

{
  "id": "8339abb5-c9c3-4a91-a276-db5222375216",
  "nome": "Maria Silva",
  "cpf": "***.982.247-**",
  "email": "maria.silva@email.com",
  "dataNascimento": "1990-05-10",
  "rendaMensal": 5000.00,
  "criadoEm": "2026-10-28T21:12:46.560075Z"
}
```

### Regras de validação

| Campo | Regra |
|---|---|
| `nome` | obrigatório, até 150 caracteres |
| `cpf` | obrigatório, dígitos verificadores válidos, formato `52998224725` ou `529.982.247-25`, **único** |
| `email` | obrigatório, formato válido com domínio, até 254 caracteres, **único** (sem diferenciar maiúsculas) |
| `dataNascimento` | obrigatória, cliente entre 18 e 120 anos |
| `rendaMensal` | maior que zero, até 2 casas decimais |

### Formato dos erros

Todos os erros seguem o padrão **ProblemDetails (RFC 9457)**, em português, com `traceId` para rastreio. Erros de validação trazem todos os campos de uma vez:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Requisição inválida",
  "status": 400,
  "detail": "Um ou mais campos são inválidos.",
  "instance": "/api/v1/clientes",
  "errors": {
    "cpf": ["O CPF informado é inválido."],
    "email": ["O e-mail informado é inválido."]
  },
  "traceId": "00-a077d96f16e40dbffd1411318ffecccb-e6e0f63fda2bdf47-00"
}
```

## Evento publicado

**`ClienteCadastrado`** — exchange `cliente-cadastrado` (fanout), URN `urn:message:cliente-cadastrado`

```json
{
  "clienteId": "8339abb5-c9c3-4a91-a276-db5222375216",
  "nome": "Maria Silva",
  "dataNascimento": "1990-05-10",
  "rendaMensal": "5000.00",
  "criadoEm": "2026-10-28T21:12:46.560075Z"
}
```

O MassTransit serializa `decimal` como string para não perder precisão; consumidores MassTransit desserializam de volta para `decimal` automaticamente.

## Variáveis de ambiente

| Variável | Descrição | Exemplo |
|---|---|---|
| `ConnectionStrings__CadastroDb` | Conexão com o PostgreSQL | `Host=postgres;Port=5432;Database=cadastro;Username=postgres;Password=postgres` |
| `RabbitMq__Host` | Host do RabbitMQ | `rabbitmq` |
| `RabbitMq__Port` | Porta AMQP (padrão `5672`) | `5672` |
| `RabbitMq__VirtualHost` | Virtual host (padrão `/`) | `/` |
| `RabbitMq__Username` | Usuário | `guest` |
| `RabbitMq__Password` | Senha | `guest` |
| `Database__ApplyMigrationsOnStartup` | Aplica as migrations ao subir (ligado em `Development`) | `true` |
| `ASPNETCORE_ENVIRONMENT` | `Development` habilita Swagger e migrations automáticas | `Development` |

A aplicação **não sobe** se a connection string ou as credenciais do RabbitMQ estiverem ausentes (fail-fast), em vez de falhar só na primeira requisição.

## Estrutura

```
src/
├── CadastroClientes.Api              → Controllers, tratamento de erros, Program.cs
├── CadastroClientes.Application      → Casos de uso (services), DTOs, validação, contratos de evento
├── CadastroClientes.Domain           → Entidade Cliente e regra de CPF (sem dependências externas)
└── CadastroClientes.Infrastructure   → EF Core, repositórios, MassTransit/RabbitMQ, migrations
tests/
└── CadastroClientes.UnitTests        → xUnit, espelhando a estrutura de src/
```

As dependências apontam para dentro: `Api → Application → Domain`, e a `Infrastructure` implementa as interfaces da `Application` (`IClienteRepository`, `IUnitOfWork`, `IEventPublisher`). A regra de negócio não conhece EF Core nem RabbitMQ.

## Decisões técnicas

**Transactional Outbox.** O cliente e o evento são gravados na mesma transação do banco; o MassTransit entrega o evento ao RabbitMQ depois do commit. Se o broker estiver fora do ar, nada se perde: o evento fica no Outbox e é publicado quando ele voltar. Sem isso, seria possível ter cliente salvo sem evento (ou evento de um cliente que não foi salvo).

**Minimização de dados no evento (LGPD).** `ClienteCadastrado` carrega só o que a proposta precisa: renda, data de nascimento e nome. CPF e e-mail não trafegam na mensageria. Na API, o CPF é devolvido mascarado.

**Contrato do evento identificado por nome, sem biblioteca compartilhada.** O evento usa a URN `cliente-cadastrado` em vez do namespace C#, e cada microsserviço mantém sua própria cópia do contrato. Um pacote NuGet compartilhado acoplaria os deploys dos serviços.

**Unicidade garantida em duas camadas.** O serviço verifica CPF e e-mail antes de gravar (mensagem clara), e índices únicos no banco cobrem requisições simultâneas. A violação do índice também vira `409`, indicando o campo duplicado.

**Validação fora da entidade.** As regras de entrada ficam no `CadastrarClienteValidator`, que devolve todos os erros de uma vez; a regra do CPF fica no domínio (`Cpf`), reaproveitável e testável isoladamente. Os limites de tamanho são constantes da entidade, compartilhadas entre o validador e o mapeamento do banco.

**`TimeProvider` em vez de `DateTime.Now`.** O cálculo de idade e a data de criação usam um relógio injetável, o que permite testar fronteiras como "faz 18 anos hoje".

**MassTransit 8.x.** A partir da versão 9 o MassTransit passou a exigir licença comercial; a 8.x segue open source (Apache 2.0).

**O que ficou de fora, de propósito.**
- *MediatR/CQRS:* com dois endpoints, seria cerimônia sem ganho.
- *Repositório genérico:* cada repositório expõe só o que os casos de uso usam.
- *Autenticação:* não faz parte do escopo do desafio.

**Ambiente.** As migrations são aplicadas na inicialização apenas quando configurado (Docker/desenvolvimento); em produção rodariam no pipeline de deploy. A imagem Docker é multi-stage e executa com usuário sem privilégios.
