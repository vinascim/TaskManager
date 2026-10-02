# TaskManager API

[![CI](https://github.com/vinascim/TaskManager/actions/workflows/ci.yml/badge.svg)](https://github.com/vinascim/TaskManager/actions/workflows/ci.yml)

API REST para gestão de tarefas: cadastro, listagem com filtros e paginação, busca, edição e exclusão.

Desenvolvida em **.NET 10** com **Clean Architecture**, persistência em **EF Core InMemory**, validação com **FluentValidation**, documentação com **Swagger** e testes automatizados em três níveis.

---

## Sumário
- [Tecnologias](#tecnologias)
- [Como executar](#como-executar)
- [Como testar](#como-testar)
- [Endpoints](#endpoints)
- [Regras de negócio](#regras-de-negócio)
- [Arquitetura](#arquitetura)
- [Testes automatizados](#testes-automatizados)
- [Melhorias futuras](#melhorias-futuras)

---

## Tecnologias

| Categoria | Tecnologia |
|---|---|
| Framework | .NET 10 / ASP.NET Core |
| Persistência | Entity Framework Core 10 (provider InMemory) |
| Validação | FluentValidation |
| Documentação | Swagger (Swashbuckle) com comentários XML |
| Logging | Serilog |
| Testes | xUnit, FluentAssertions, NSubstitute, `WebApplicationFactory` |
| CI | GitHub Actions |
| Container | Docker |

---

## Como executar

### Pré-requisitos
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/) (opcional, apenas para executar via container)

Não é necessário banco de dados: os dados ficam em memória enquanto a aplicação estiver rodando.

### Com o .NET
```bash
git clone https://github.com/vinascim/TaskManager.git
cd TaskManager
dotnet run --project src/TaskManager.Api
```

Com a aplicação rodando, acesse o Swagger em **http://localhost:5101/swagger**.

### Com Docker
```bash
docker build -t taskmanager-api .
docker run --rm -p 8080:8080 -e ASPNETCORE_ENVIRONMENT=Development taskmanager-api
```

Acesse **http://localhost:8080/swagger**.

> O Swagger fica disponível apenas no ambiente `Development`, por isso a variável `ASPNETCORE_ENVIRONMENT` é informada ao subir o container.

### Health check
`GET /health` retorna `200 Healthy` quando a aplicação está no ar.

---

## Como testar

### Testes automatizados
```bash
dotnet test
```
Executa os 83 testes (unitários e de integração). Os mesmos testes rodam no GitHub Actions a cada push e pull request.

### Pelo Swagger
Acesse `/swagger`, expanda um endpoint, clique em **Try it out** e depois em **Execute**. Os campos já vêm preenchidos com exemplos.

### Exemplo com curl
```bash
curl -X POST http://localhost:5101/api/tasks \
  -H "Content-Type: application/json" \
  -d '{"title":"Implementar tela de login","dueDate":"2026-12-31","status":"InProgress"}'

curl "http://localhost:5101/api/tasks?status=InProgress&search=login&page=1&pageSize=10"
```
---

## Endpoints

| Método | Rota | Descrição | Sucesso | Erros |
|---|---|---|---|---|
| `POST` | `/api/tasks` | Cria uma tarefa | `201 Created` + header `Location` | `400` |
| `GET` | `/api/tasks` | Lista tarefas com filtros e paginação | `200 OK` | `400` |
| `GET` | `/api/tasks/{id}` | Busca uma tarefa pelo id | `200 OK` | `404` |
| `PUT` | `/api/tasks/{id}` | Atualiza todos os campos de uma tarefa | `200 OK` | `400`, `404` |
| `DELETE` | `/api/tasks/{id}` | Exclui uma tarefa | `204 No Content` | `404` |

### Criar tarefa
```json
POST /api/tasks
{
  "title": "Implementar tela de login",
  "description": "Criar formulário com e-mail e senha e validação dos campos",
  "dueDate": "2026-12-31",
  "status": "InProgress"
}
```
Apenas `title` é obrigatório. Sem `status`, a tarefa é criada como `Pending`.

### Filtros e paginação da listagem
Todos os parâmetros são opcionais e podem ser combinados.

| Parâmetro | Descrição | Exemplo |
|---|---|---|
| `status` | `Pending`, `InProgress` ou `Completed` | `status=Pending` |
| `dueDateFrom` | Vencimento a partir da data | `dueDateFrom=2026-01-01` |
| `dueDateTo` | Vencimento até a data | `dueDateTo=2026-12-31` |
| `search` | Texto no título ou na descrição, sem diferenciar maiúsculas | `search=login` |
| `page` | Página, a partir de 1 (padrão: 1) | `page=2` |
| `pageSize` | Itens por página, de 1 a 100 (padrão: 20) | `pageSize=10` |

Para filtrar por uma data de vencimento específica, informe a mesma data em `dueDateFrom` e `dueDateTo`.

Resposta:
```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "title": "Implementar tela de login",
      "description": "Criar formulário com e-mail e senha e validação dos campos",
      "dueDate": "2026-12-31",
      "status": "InProgress"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 1,
  "totalPages": 1
}
```
Tarefas com vencimento aparecem primeiro, ordenadas pela data; tarefas sem vencimento vêm em seguida.

### Erros
Todas as respostas de erro seguem o padrão **Problem Details** ([RFC 9457](https://www.rfc-editor.org/rfc/rfc9457)):
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Um ou mais erros de validação ocorreram.",
  "status": 400,
  "errors": {
    "title": ["O título é obrigatório."],
    "description": ["A descrição deve ter no máximo 500 caracteres."]
  },
  "traceId": "00-..."
}
```

---

## Regras de negócio

| Campo | Regra |
|---|---|
| Id | Gerado pelo sistema (`Guid`) |
| Título | Obrigatório, até 100 caracteres; espaços nas pontas são removidos |
| Descrição | Opcional, até 500 caracteres; texto vazio é tratado como ausente |
| Vencimento | Opcional; **datas no passado são aceitas**, pois tarefas retroativas e atrasadas são comuns |
| Status | `Pending`, `InProgress` ou `Completed`; padrão `Pending` na criação e obrigatório na edição |

A edição segue a semântica do `PUT`: todos os campos são enviados e substituem os atuais. Enviar `description` ou `dueDate` como `null` remove o valor.

---

## Arquitetura

O projeto segue a **Clean Architecture**: as dependências apontam sempre para o domínio, que não conhece banco de dados nem framework web.

| Camada | Responsabilidade | Depende de |
|---|---|---|
| **Domain** | Entidade `TaskItem`, enum de status e invariantes | Nada |
| **Application** | `TaskService`, DTOs, validators e a interface `ITaskRepository` | Domain |
| **Infrastructure** | `AppDbContext` e implementação do repositório com EF Core | Application, Domain |
| **Api** | Controller, tratamento de erros, Swagger e configuração da injeção de dependência | Application, Infrastructure |

**Por que essa escolha:**
- Regras de negócio e casos de uso testáveis sem banco nem HTTP.
- O `TaskService` depende de `ITaskRepository`, não do EF Core: trocar o InMemory por um banco relacional afeta apenas o Infrastructure.
- Responsabilidades separadas e fáceis de localizar.

### Estrutura de pastas
```
src/
├── TaskManager.Domain/          Entities, Enums, Exceptions
├── TaskManager.Application/     Abstractions, DTOs, Validators, Services, Exceptions
├── TaskManager.Infrastructure/  Persistence (DbContext, Configuration, Repositories)
└── TaskManager.Api/             Controllers, ExceptionHandling, Program.cs
tests/
├── TaskManager.Domain.Tests/
├── TaskManager.Application.Tests/
└── TaskManager.Api.IntegrationTests/
```

---

## Testes automatizados

São 83 testes em três níveis:

| Projeto | Testes | Escopo |
|---|---|---|
| `TaskManager.Domain.Tests` | 18 | Invariantes da entidade, incluindo a atomicidade da atualização |
| `TaskManager.Application.Tests` | 42 | Validators e `TaskService`, com o repositório mockado (NSubstitute) |
| `TaskManager.Api.IntegrationTests` | 23 | Fluxo HTTP completo com `WebApplicationFactory`: status codes, serialização, filtros, paginação, ordenação e erros |

Nos testes de integração, cada classe de teste usa um banco InMemory exclusivo, o que mantém os testes independentes mesmo quando executados em paralelo.

---

## Melhorias futuras
- Banco de dados relacional (PostgreSQL ou SQL Server) com migrations, e testes de integração com Testcontainers.
- Autenticação e autorização (JWT), com tarefas associadas ao usuário.
- Campos de auditoria (`CreatedAt`, `UpdatedAt`).
- Health check que verifique também a conexão com o banco.
- Versionamento da API.
