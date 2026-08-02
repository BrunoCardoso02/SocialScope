---
name: socialscope-backend
description: Arquitetura, stack, regras de negócio, padrões e fases de implementação do SocialScope Backend (.NET 10, Clean Architecture, PostgreSQL)
when_to_use: Quando precisar de informações sobre a estrutura, arquitetura, tecnologias, integações ou regras de negócio do SocialScope Backend
---

# SocialScope — Backend Skill

## 1. Objetivo

Construir uma API backend em **C# / .NET 10** para um Social Media Management Dashboard.

A aplicação permitirá que um usuário conecte diferentes redes sociais e visualize, em um único lugar, informações como:

* contas conectadas;
* perfil;
* seguidores e seguindo;
* posts;
* curtidas;
* comentários;
* compartilhamentos;
* visualizações;
* métricas e analytics;
* notificações;
* sincronização periódica com APIs externas.

O backend deve ser desenvolvido com foco em **arquitetura limpa, separação de responsabilidades, segurança, integração com APIs externas e código de produção**.

O frontend será desenvolvido separadamente em **Next.js** e consumirá esta API.

---

# 2. Stack

## Backend

* C#
* .NET 10
* ASP.NET Core Web API
* Entity Framework Core
* PostgreSQL
* Redis
* FluentValidation
* Serilog
* Swagger / OpenAPI
* JWT
* OAuth 2.0
* SignalR
* BackgroundService

## Infraestrutura

* Docker
* Docker Compose
* Git
* GitHub Actions
* CI/CD

## Integrações futuras

A arquitetura deve permitir integração com:

* Instagram / Meta
* Facebook
* YouTube
* TikTok
* LinkedIn
* X

Não é necessário implementar todas as integrações inicialmente.

Começar com uma ou duas plataformas reais e deixar a arquitetura preparada para novas implementações.

---

# 3. Objetivos de aprendizado

O projeto deve ser utilizado para praticar:

* ASP.NET Core;
* .NET 10;
* Clean Architecture;
* SOLID;
* Dependency Injection;
* Entity Framework Core;
* Repository Pattern quando fizer sentido;
* Unit of Work quando fizer sentido;
* DTOs;
* validação;
* autenticação e autorização;
* JWT;
* OAuth 2.0;
* integração com APIs externas;
* HttpClient / IHttpClientFactory;
* tratamento de erros;
* rate limiting;
* retry;
* cache;
* Redis;
* Background Services;
* processamento assíncrono;
* SignalR;
* logs estruturados;
* observabilidade;
* testes unitários;
* testes de integração;
* Docker;
* CI/CD.

O projeto não deve ser transformado em um **microservices overkill**.

Começar como um **Modular Monolith** bem organizado.

---

# 4. Arquitetura

Utilizar Clean Architecture.

Estrutura inicial:

```text
SocialMediaHub/
│
├── src/
│   ├── SocialMediaHub.Api/
│   ├── SocialMediaHub.Application/
│   ├── SocialMediaHub.Domain/
│   └── SocialMediaHub.Infrastructure/
│
├── tests/
│   ├── SocialMediaHub.UnitTests/
│   └── SocialMediaHub.IntegrationTests/
│
├── docker-compose.yml
├── Directory.Build.props
├── Directory.Packages.props
└── README.md
```

## Responsabilidades

### Domain

Contém as regras de negócio centrais.

Não deve depender de:

* Entity Framework;
* ASP.NET;
* PostgreSQL;
* Redis;
* APIs externas.

Exemplos:

```text
Entities/
ValueObjects/
Enums/
Exceptions/
DomainEvents/
Interfaces/
```

---

### Application

Contém os casos de uso da aplicação.

Exemplos:

```text
Features/
DTOs/
Interfaces/
Validators/
Mappings/
Services/
```

A camada Application não deve conhecer detalhes de infraestrutura.

Exemplo:

```csharp
public interface ISocialMediaProvider
{
    Task<SocialProfileDto> GetProfileAsync(
        SocialAccount account,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<SocialPostDto>> GetPostsAsync(
        SocialAccount account,
        CancellationToken cancellationToken);

    Task<SocialMetricsDto> GetMetricsAsync(
        SocialAccount account,
        CancellationToken cancellationToken);
}
```

---

### Infrastructure

Responsável por detalhes externos:

* EF Core;
* PostgreSQL;
* Redis;
* OAuth;
* APIs das redes sociais;
* serviços de terceiros;
* persistência;
* criptografia;
* implementação de repositories;
* serviços de background.

Exemplo:

```text
Infrastructure/
├── Persistence/
├── Identity/
├── SocialMedia/
│   ├── Instagram/
│   ├── YouTube/
│   ├── LinkedIn/
│   └── TikTok/
├── Cache/
├── Messaging/
├── BackgroundJobs/
└── Services/
```

---

### API

Responsável somente pela camada HTTP.

Contém:

* Controllers;
* middleware;
* filtros;
* autenticação;
* configuração;
* DI;
* Swagger;
* endpoints.

Não colocar regras de negócio diretamente nos Controllers.

---

# 5. Modelo de domínio

Entidades principais:

```text
User
SocialAccount
SocialProfile
SocialPost
SocialPostMetric
SocialComment
SocialMetricSnapshot
Notification
SyncExecution
```

Possível relacionamento:

```text
User
 │
 ├── SocialAccount
 │       │
 │       └── SocialProfile
 │
 ├── Notification
 │
 └── SocialMetricSnapshot

SocialAccount
 │
 └── SocialPost
        │
        ├── SocialPostMetric
        └── SocialComment
```

---

# 6. User

Representa o usuário da aplicação.

Campos sugeridos:

```text
Id
Email
PasswordHash
Name
CreatedAt
UpdatedAt
LastLoginAt
```

Regras:

* Email deve ser único;
* senha nunca deve ser armazenada em texto puro;
* senha deve utilizar hashing seguro;
* usuário só pode acessar seus próprios recursos;
* endpoints devem validar autorização.

---

# 7. SocialAccount

Representa uma conta de rede social conectada.

Campos sugeridos:

```text
Id
UserId
Platform
ExternalAccountId
Username
DisplayName
AccessToken
RefreshToken
TokenExpiresAt
ConnectedAt
LastSyncedAt
IsActive
```

`Platform`:

```csharp
public enum SocialPlatform
{
    Instagram,
    Facebook,
    YouTube,
    TikTok,
    LinkedIn,
    X
}
```

Regras:

* uma conta externa não deve ser conectada duas vezes ao mesmo usuário;
* tokens nunca devem aparecer em respostas da API;
* tokens devem ser armazenados de forma segura;
* tokens expirados devem ser renovados quando possível;
* contas desconectadas não devem participar de sincronizações;
* toda sincronização deve respeitar os limites da plataforma.

---

# 8. Abstração das redes sociais

Não criar Controllers específicos para cada plataforma.

Criar uma abstração:

```csharp
public interface ISocialMediaProvider
{
    SocialPlatform Platform { get; }

    Task<SocialProfileDto> GetProfileAsync(
        SocialAccount account,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<SocialPostDto>> GetPostsAsync(
        SocialAccount account,
        CancellationToken cancellationToken);

    Task<SocialMetricsDto> GetMetricsAsync(
        SocialAccount account,
        CancellationToken cancellationToken);
}
```

Cada plataforma implementa a interface.

Exemplo:

```text
ISocialMediaProvider
       │
       ├── InstagramProvider
       ├── YouTubeProvider
       ├── LinkedInProvider
       └── TikTokProvider
```

O sistema deve selecionar automaticamente o provider correspondente à plataforma.

Evitar:

```csharp
if (platform == Instagram) { ... }
else if (platform == YouTube) { ... }
else if (platform == TikTok) { ... }
```

espalhado pela aplicação.

---

# 9. OAuth 2.0

A conexão com redes sociais deve utilizar OAuth sempre que a plataforma suportar.

Fluxo:

```text
Frontend
   │
   │ Connect Instagram
   ▼
Backend
   │
   │ Authorization URL
   ▼
Social Platform
   │
   │ User authorizes
   ▼
Callback
   │
   ▼
Backend
   │
   ├── Validate state
   ├── Exchange authorization code
   ├── Obtain tokens
   ├── Persist encrypted tokens
   └── Create SocialAccount
```

Nunca pedir ao usuário para copiar tokens manualmente quando OAuth estiver disponível.

---

# 10. Autenticação

Implementar:

```text
POST /api/auth/register
POST /api/auth/login
POST /api/auth/refresh
POST /api/auth/logout
GET  /api/auth/me
```

Utilizar:

* JWT Access Token;
* Refresh Token;
* expiração curta para Access Token;
* rotação de Refresh Token;
* revogação de Refresh Token.

Não colocar informações sensíveis dentro do JWT.

---

# 11. Autorização

Todo recurso deve pertencer a um usuário.

Exemplo:

```text
GET /api/social-accounts
```

deve retornar apenas as contas do usuário autenticado.

A identidade principal deve vir do token autenticado.

Implementar autorização baseada no usuário autenticado.

---

# 12. Social Accounts API

Endpoints sugeridos:

```text
GET    /api/social-accounts
GET    /api/social-accounts/{id}
DELETE /api/social-accounts/{id}
POST   /api/social-accounts/{id}/sync
```

OAuth:

```text
GET /api/social-accounts/connect/{platform}
GET /api/social-accounts/callback/{platform}
```

---

# 13. Posts API

Endpoints:

```text
GET /api/posts
GET /api/posts/{id}
```

Filtros:

```text
platform
from
to
page
pageSize
```

Exemplo:

```text
GET /api/posts?platform=Instagram&page=1&pageSize=20
```

Utilizar paginação.

Nunca retornar milhares de registros de uma vez.

---

# 14. Analytics API

Endpoints:

```text
GET /api/analytics/overview
GET /api/analytics/followers
GET /api/analytics/engagement
GET /api/analytics/reach
GET /api/analytics/views
GET /api/analytics/platforms
```

Permitir filtros por:

```text
from
to
platform
```

Exemplo:

```text
GET /api/analytics/overview?from=2026-07-01&to=2026-07-26
```

O backend deve agregar os dados necessários para o frontend gerar gráficos.

---

# 15. Dashboard

Criar um endpoint consolidado:

```text
GET /api/dashboard
```

Retornar:

```text
TotalFollowers
TotalFollowing
TotalPosts
TotalLikes
TotalComments
TotalShares
TotalViews
EngagementRate
FollowerGrowth
PlatformSummaries
RecentPosts
RecentNotifications
```

Evitar que o frontend precise fazer várias requisições para montar a tela inicial.

---

# 16. Sincronização

Criar sincronização periódica das contas.

Fluxo:

```text
BackgroundService
       ↓
Find active SocialAccounts
       ↓
Resolve provider
       ↓
Fetch external data
       ↓
Normalize
       ↓
Persist
       ↓
Update LastSyncedAt
```

Implementar inicialmente com:

```csharp
BackgroundService
```

Não utilizar RabbitMQ/Kafka apenas por utilizar.

Se futuramente o volume justificar, a sincronização pode evoluir para filas.

---

# 17. Idempotência

A sincronização deve ser idempotente.

Não criar duplicações quando o mesmo post for sincronizado várias vezes.

Utilizar identificadores externos:

```text
Platform
ExternalPostId
```

Criar índice único apropriado.

Exemplo:

```text
Instagram + 123456789
```

não pode existir duas vezes para a mesma conta.

---

# 18. Rate Limiting

As APIs externas possuem limites.

O backend deve:

* controlar frequência das chamadas;
* respeitar respostas `429`;
* implementar retry com backoff quando apropriado;
* evitar chamadas desnecessárias;
* utilizar cache quando possível.

Não fazer retry infinito.

---

# 19. Redis

Utilizar Redis para dados que não precisam ser consultados no banco a todo momento.

Possíveis usos:

* cache de perfil;
* métricas recentes;
* dashboard;
* rate limiting;
* locks distribuídos futuramente.

Não utilizar Redis como banco principal.

---

# 20. Notifications

Entidade:

```text
Notification
```

Campos:

```text
Id
UserId
Type
Title
Message
IsRead
CreatedAt
```

Endpoints:

```text
GET   /api/notifications
PATCH /api/notifications/{id}/read
PATCH /api/notifications/read-all
```

---

# 21. SignalR

Utilizar SignalR para notificações em tempo real.

Fluxo:

```text
External Event / Background Job
          ↓
Notification Service
          ↓
SignalR Hub
          ↓
Next.js
          ↓
Notification UI
```

Exemplo:

```text
"Você ganhou 120 seguidores hoje."
```

pode aparecer no dashboard sem refresh.

---

# 22. Tratamento de erros

Utilizar middleware global.

Todas as exceções devem ser convertidas em respostas consistentes.

Utilizar `ProblemDetails`.

Formato sugerido:

```json
{
  "type": "https://example.com/errors/validation",
  "title": "Validation failed",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "errors": {
    "email": [
      "Invalid email."
    ]
  },
  "traceId": "..."
}
```

Não retornar stack trace em produção.

---

# 23. Validação

Utilizar FluentValidation.

Validar:

* requests;
* parâmetros;
* filtros;
* paginação;
* regras de negócio simples.

Exemplo:

```text
Email
Password
Date ranges
Page
PageSize
Platform
```

---

# 24. DTOs

Nunca retornar diretamente entidades do EF Core nos endpoints.

Utilizar DTOs:

```text
Request DTO
Response DTO
```

Exemplo:

```csharp
public sealed record SocialAccountResponse(
    Guid Id,
    SocialPlatform Platform,
    string Username,
    string DisplayName,
    DateTimeOffset ConnectedAt,
    DateTimeOffset? LastSyncedAt,
    bool IsActive);
```

Tokens jamais devem estar nos Response DTOs.

---

# 25. Persistência

Utilizar Entity Framework Core.

Organização:

```text
Infrastructure/
└── Persistence/
    ├── AppDbContext.cs
    ├── Configurations/
    ├── Repositories/
    └── Migrations/
```

Preferir `IEntityTypeConfiguration<T>` para configurações das entidades.

Evitar colocar configurações complexas dentro do `DbContext`.

---

# 26. PostgreSQL

Utilizar PostgreSQL como banco principal.

Criar migrations.

Não utilizar `EnsureCreated()` no fluxo normal da aplicação.

Configuração via environment variables:

```text
ConnectionStrings__DefaultConnection
```

Nunca commitar:

```text
password
connection strings reais
API keys
client secrets
OAuth secrets
JWT signing keys
```

---

# 27. Segurança

Implementar:

* HTTPS;
* JWT;
* Refresh Token rotation;
* hashing de senha;
* autorização;
* CORS configurado;
* rate limiting;
* validação de entrada;
* secrets via environment variables;
* logs sem informações sensíveis;
* proteção contra exposição de tokens;
* criptografia de credenciais externas quando armazenadas.

Nunca logar:

```text
AccessToken
RefreshToken
Password
ClientSecret
Authorization header
```

---

# 28. Logs

Utilizar Serilog.

Registrar eventos importantes:

```text
UserRegistered
UserLoggedIn
SocialAccountConnected
SocialAccountDisconnected
SyncStarted
SyncCompleted
SyncFailed
ExternalApiRateLimited
OAuthFailed
UnhandledException
```

Logs devem conter `CorrelationId` / `TraceId` quando apropriado.

---

# 29. Observabilidade

Preparar o backend para observar:

* requests;
* duração das requisições;
* erros;
* sincronizações;
* chamadas externas;
* falhas de autenticação;
* cache;
* banco.

Se possível, adicionar OpenTelemetry posteriormente.

---

# 30. Testes

## Unit Tests

Testar:

* regras de domínio;
* services;
* validators;
* cálculo de métricas;
* seleção de providers;
* casos de erro.

Utilizar:

```text
xUnit
Moq
```

ou outra biblioteca equivalente quando fizer sentido.

## Integration Tests

Testar:

* autenticação;
* endpoints;
* banco;
* repositories;
* fluxo de sincronização.

Não mockar tudo nos testes de integração.

---

# 31. Estrutura de Features

Na Application, preferir organização por feature quando o projeto crescer:

```text
Application/
├── Auth/
│   ├── Register/
│   ├── Login/
│   └── RefreshToken/
│
├── SocialAccounts/
│   ├── Connect/
│   ├── Get/
│   ├── Disconnect/
│   └── Sync/
│
├── Posts/
│   ├── GetPosts/
│   └── GetPost/
│
├── Analytics/
│   ├── Overview/
│   ├── Followers/
│   └── Engagement/
│
└── Notifications/
```

Evitar criar uma camada gigantesca de `Services` com dezenas de métodos sem organização.

---

# 32. Paginação

Toda listagem potencialmente grande deve possuir paginação.

Modelo:

```json
{
  "items": [],
  "page": 1,
  "pageSize": 20,
  "totalItems": 150,
  "totalPages": 8
}
```

Implementar paginação no banco, não depois de carregar todos os registros em memória.

---

# 33. Cancelamento

Métodos assíncronos devem aceitar:

```csharp
CancellationToken cancellationToken
```

Especialmente:

* chamadas HTTP;
* queries;
* BackgroundService;
* sincronizações;
* operações de longa duração.

---

# 34. HttpClient

Para APIs externas, utilizar:

```text
IHttpClientFactory
```

ou typed clients.

Evitar:

```csharp
new HttpClient()
```

espalhado pela aplicação.

Criar clientes específicos:

```text
InstagramHttpClient
YouTubeHttpClient
LinkedInHttpClient
```

quando necessário.

---

# 35. Resiliência

Chamadas externas devem considerar:

* timeout;
* retry;
* exponential backoff;
* circuit breaker quando apropriado;
* tratamento de 401;
* tratamento de 403;
* tratamento de 404;
* tratamento de 429;
* tratamento de 5xx.

Não aplicar retry indiscriminadamente.

---

# 36. Docker

Criar:

```text
Dockerfile
docker-compose.yml
```

O ambiente local deve conseguir subir:

```text
API
PostgreSQL
Redis
```

com um comando:

```bash
docker compose up -d
```

---

# 37. Configuração

Separar configurações por ambiente:

```text
Development
Staging
Production
```

Utilizar:

```text
appsettings.json
appsettings.Development.json
Environment Variables
```

Secrets não devem ficar no Git.

---

# 38. CI/CD

Criar pipeline no GitHub Actions.

Pipeline mínimo:

```text
Push
 ↓
Restore
 ↓
Build
 ↓
Unit Tests
 ↓
Integration Tests
 ↓
Publish
```

Posteriormente:

```text
Docker Build
 ↓
Push Image
 ↓
Deploy
```

---

# 39. Ordem de implementação

Implementar incrementalmente.

## Fase 1 — Foundation

* criar solução;
* criar projetos;
* configurar Clean Architecture;
* configurar DI;
* configurar PostgreSQL;
* configurar EF Core;
* criar migrations;
* configurar Swagger;
* configurar Docker.

## Fase 2 — Authentication

* User;
* Register;
* Login;
* JWT;
* Refresh Token;
* Authorization.

## Fase 3 — Social Accounts

* SocialAccount;
* SocialPlatform;
* CRUD;
* provider abstraction;
* primeira integração real.

## Fase 4 — Social Data

* profile;
* posts;
* metrics;
* comments;
* followers.

## Fase 5 — Dashboard

* overview;
* analytics;
* aggregations;
* filtros;
* paginação.

## Fase 6 — Synchronization

* BackgroundService;
* sync;
* idempotência;
* retry;
* rate limit;
* logs.

## Fase 7 — Redis + SignalR

* cache;
* notifications;
* real-time updates.

## Fase 8 — OAuth completo

* authorization flow;
* callbacks;
* token refresh;
* secure token storage.

## Fase 9 — Qualidade

* unit tests;
* integration tests;
* error handling;
* observability;
* security hardening.

## Fase 10 — Deploy

* Docker;
* GitHub Actions;
* CI/CD;
* documentação.

---

# 40. Regras de desenvolvimento

1. Não colocar regra de negócio em Controllers.
2. Não retornar entidades do EF Core diretamente.
3. Não acessar `DbContext` diretamente em qualquer lugar da aplicação.
4. Não colocar lógica de APIs externas dentro da Application.
5. Não duplicar lógica específica de cada rede social.
6. Não armazenar secrets no Git.
7. Não retornar tokens para o frontend.
8. Não criar microservices sem necessidade.
9. Priorizar código simples e testável.
10. Utilizar `async/await` corretamente.
11. Utilizar `CancellationToken`.
12. Utilizar Dependency Injection.
13. Validar entradas.
14. Utilizar paginação.
15. Criar logs úteis, mas sem dados sensíveis.
16. Tratar falhas de APIs externas.
17. Garantir idempotência na sincronização.
18. Escrever testes para regras importantes.
19. Preferir interfaces quando houver uma necessidade real de abstração.
20. Evitar overengineering.

---

# 41. MVP Final

O primeiro objetivo do projeto é chegar a:

```text
User
 │
 ├── Login
 │
 └── Dashboard
       │
       ├── Instagram
       │      ├── Profile
       │      ├── Followers
       │      ├── Posts
       │      └── Metrics
       │
       └── YouTube
              ├── Profile
              ├── Subscribers
              ├── Videos
              └── Metrics
```

Com:

```text
.NET 10
Clean Architecture
PostgreSQL
EF Core
JWT
OAuth
REST API
Swagger
Docker
Tests
```

Depois do MVP, adicionar Redis, BackgroundService, SignalR, mais providers e observabilidade.

---

# 42. Resultado esperado

Ao finalizar, o projeto deve demonstrar domínio prático de desenvolvimento backend moderno com .NET, incluindo:

* arquitetura;
* domínio;
* persistência;
* APIs REST;
* autenticação;
* autorização;
* integração externa;
* OAuth;
* cache;
* processamento em background;
* resiliência;
* testes;
* segurança;
* observabilidade;
* Docker;
* CI/CD.

O projeto deve continuar sendo um **monólito modular**, simples de executar localmente e suficientemente completo para servir como projeto de estudo e portfólio.
