# Architecture

RemoteCommerce is a .NET 10 ASP.NET Core application with Interactive Server Blazor, EF Core persistence, MediatR application workflows, MudBlazor administration, and a restart-based plugin model.

## Architectural boundaries

The repository is currently a single host project. Domain, Application, Infrastructure, Presentation, and Plugin Runtime are explicit logical boundaries inside that host.

- Domain contains business entities and rules and has no dependency on Application or Infrastructure.
- Application contains feature use cases and abstractions and does not depend on concrete Infrastructure implementations.
- Infrastructure owns EF Core, DbContexts, repositories, storage providers, provider strategy, and external integrations.
- UI/Presentation is an adapter and must not access EF Core or storage providers directly.

## Future shared assembly

The previous plan to extract Domain, Application, and Infrastructure into three class libraries is retired.

The only future shared class library is:

```text
src/RemoteCommerce.Abstractions/
└── RemoteCommerce.Abstractions.csproj
    RootNamespace = RemoteCommerce
```

Its purpose is to hold reusable non-concrete code: contracts, interfaces, request/result models, DTOs, enums, and other boundary-neutral models.

It must not contain concrete implementations or references to EF Core, DbContext, SQL/MongoDB/filesystem providers, ASP.NET Core concrete services, Blazor, MudBlazor, or plugin runtime implementation details.

The class library preserves the same logical namespace architecture used by the current host. A file can therefore retain namespaces such as `RemoteCommerce.Application.Persistence.Abstractions` while physically living in `RemoteCommerce.Abstractions`.

The host continues to own concrete Domain, Application, Infrastructure, Presentation, and Plugin Runtime implementations. No additional implementation assemblies are implied by this rule.

## Application feature layout

Every Application feature must organize its artifacts under the following canonical structure:

```text
src/Application/Feature/
├── Abstractions/
├── Commands/
├── Handlers/
├── Queries/
├── Requests/
├── Resources/
├── Results/
└── Validators/
```

The current host path is `src/RemoteCommerce/Application/Feature/...`.

A feature must not introduce feature-local `Services`, `Models`, `Dtos`, `Contracts`, `Controllers`, or alternative command/query folders. Concrete feature services belong in the corresponding Infrastructure feature boundary; non-concrete contracts/models belong in `Abstractions`, `Requests`, `Results`, or `Resources` according to their role.

Domain features remain under `src/RemoteCommerce/Domain/<Feature>` and Infrastructure features under `src/RemoteCommerce/Infrastructure/<Feature>`.

## Canonical data flow

```text
 ___________________       ___________________________
|    (Requests)     |      |    (Commands,Queries)    |
|    Controllers    |=====>| MediatR Handlers         |
|___________________|      |          └── Behaviors   |
                           |__________________________|
                                         |
                                       \ | /
                           _____________\|/_____________
                           |(Application/Infrastructure)|
                           |     Feature  Services      |
                           |____________________________|
                                         |
                                       \ | /
                           _____________\|/_____________
                           |      (Infrastructure)      |
                           |    Repository<T> *         |  *Repository for dbcontext or storage provider,
                           |    └──DbContext|Storage    |   db agnostic
                           |____________________________|
```

Controllers receive operation-specific Requests and never bind MediatR Commands/Queries directly. Each Command/Query receives the exact corresponding Request instance in its constructor and maps Request values into use-case data.

Handlers run through configured MediatR Behaviors. Feature Services coordinate application work through abstractions and must not expose or depend on transport/controller concerns. Repository contracts are provider agnostic; implementations belong to Infrastructure and may use DbContext or a storage provider.

Application handlers return `Result` for body-less operations and `Result<T>` for operations with a response body. Controllers map these results to HTTP responses.

## Exception propagation

Every executable layer in `Controllers -> Handlers -> Behaviors -> Feature Services -> Repository<T> -> StorageProvider` must participate in an exception logging/cleanup boundary using `try/catch/finally` directly or through the layer's explicit cross-cutting wrapper/decorator.

Catch blocks must log relevant context and always rethrow the original exception. They must never swallow exceptions, return silent fallbacks, or translate exceptions into HTTP responses.

The global exception handler is the only HTTP exception translator. It maps validation, authorization, not-found, conflict, persistence/provider, cancellation, and unexpected exceptions to Problem Details and appropriate HTTP status codes, with a safe fallback for unknown failures.

## Product Catalog

Stage 08 introduces the host-owned catalog domain: Product, ProductVariant, Category, Brand, Tag, ProductAttribute, ProductAttributeValue, ProductMetadata, and product media references.

Catalog persistence uses the existing CommerceDbContext and Stage 06/07 provider strategy. Media binaries remain behind IMediaStorageProvider. Catalog entities participate in the shared soft-delete and operation-history mechanisms.

## Administration and theming

The presentation architecture is:

```text
Application use case
    ↓
Request / Result or Page ViewModel
    ↓
UI Components
    ↓
Theme / presentation contracts
    ↓
Component library implementation
```

Theme contracts live in the shared abstractions boundary. MudBlazor remains an internal component library and is not the RemoteCommerce theme contract.

Dynamic administration menus are composed from core and plugin contributions. Menu visibility is not authorization; protected routes and APIs continue to use ASP.NET Core policies.

## Plugin boundaries

Stable plugin contracts remain in `src/RemoteCommerce.Plugin.Abstractions`. Plugins cannot access host DbContext, provider-specific persistence objects, or concrete host implementation details. Plugin APIs use `/api/rp/vX`; RemoteCommerce catalog APIs use `/api/rc/v1`.

## Formatting and documentation

C# instructions/method calls, Razor directives, and multi-line HTML/Razor component invocations follow the one-statement/one-instruction-per-line rule. Public APIs require complete en-US XML documentation.
