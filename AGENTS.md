# RemoteCommerce engineering rules

## Stack

- .NET 10 / ASP.NET Core / Blazor Web App using Interactive Server.
- EF Core + SQL Server is the current persistence implementation.
- MudBlazor is a UI component library, never the RemoteCommerce theming contract.
- Plugins are distributed as `.nupkg` packages and activation remains restart-based.
- MediatR 12.5.0 and FluentValidation are mandatory for application workflows.

## Architectural boundaries

The current repository remains a single host application, but `Domain`, `Application`, and `Infrastructure` are explicit architectural boundaries.

- Domain contains business entities, value objects, domain rules, and domain abstractions. It must not depend on Application, Infrastructure, ASP.NET Core, EF Core, Blazor, MudBlazor, or provider SDKs.
- Application contains feature use cases, requests, commands, queries, handlers, behaviors, validators, feature services, resources, results, and abstractions. It must not depend on concrete Infrastructure implementations.
- Infrastructure contains persistence, repository implementations, DbContexts, storage providers, provider strategy, external integrations, and concrete implementations of abstractions.
- Presentation and Blazor UI are adapters over Application and must not access EF Core or storage providers directly.

## Future shared class library

The repository must not be planned around three future class libraries for Domain/Application/Infrastructure. That proposal is obsolete.

The only future shared class library is:

```text
src/RemoteCommerce.Abstractions/
└── RemoteCommerce.Abstractions.csproj
    RootNamespace = RemoteCommerce
```

`RemoteCommerce.Abstractions` is a non-concrete shared contract/model assembly. It may contain interfaces, DTOs, request/result models, value-independent contracts, enums, and other code that does not represent a concrete implementation.

The package must preserve the same logical namespace architecture already used by the host. For example, shared persistence contracts may retain `RemoteCommerce.Application.Persistence.Abstractions`, domain contracts may retain `RemoteCommerce.Domain.Shared.Abstractions`, and presentation contracts may retain `RemoteCommerce.Application.Presentation`.

`RemoteCommerce.Abstractions` must never contain EF Core, DbContext, SQL/MongoDB/filesystem implementations, ASP.NET Core concrete services, Blazor components, MudBlazor components, plugin runtime implementations, or other concrete infrastructure.

The host remains responsible for concrete Domain, Application, Infrastructure, Presentation, and Plugin Runtime implementations. Future extraction of those implementations is not implied by this rule.

## Application feature organization

Every Application feature uses this canonical structure whenever the concern exists:

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

In the current host the physical path is `src/RemoteCommerce/Application/Feature/...`.

Feature-specific artifacts must remain inside their feature. Do not create global command/query/validator folders for new features.

Domain features belong under `src/RemoteCommerce/Domain/<Feature>` and Infrastructure features under `src/RemoteCommerce/Infrastructure/<Feature>`.

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

- Controllers receive operation-specific Requests only.
- Endpoints must never receive MediatR Commands or Queries through body, form, route binding, or any other transport binding.
- Each Command/Query receives the corresponding Request instance in its constructor and explicitly maps request values into use-case data.
- Handlers execute after MediatR Behaviors.
- Feature Services coordinate use cases with infrastructure abstractions.
- Repository contracts are database/storage-provider agnostic.
- Repository implementations are Infrastructure-only.
- Application and Domain never instantiate DbContext, SqlConnection, SqlCommand, MongoDB drivers, filesystem providers, or other storage implementations.
- Controllers return `Result` when no response body exists and `Result<T>` when a response body exists.

## Exception propagation

The canonical flow is instrumented with `try/catch/finally` wherever executable work requires exception logging or cleanup:

`Controllers -> Handlers -> Behaviors -> Feature Services -> Repository<T> -> StorageProvider`.

Each applicable catch logs relevant context and rethrows the original exception. Catch blocks must not swallow exceptions, return silent fallbacks, or translate exceptions into HTTP responses.

The global exception handler translates application, validation, authorization, not-found, conflict, persistence/provider, and unexpected exceptions into RFC Problem Details with the appropriate HTTP status code, using a safe fallback for unknown exceptions.

## Formatting

- One C# instruction or method call per source line.
- One logical statement per line.
- One Razor directive per line.
- One HTML/Razor component invocation per line when it has attributes or child content.
- Keep executable Razor expressions and callbacks independently readable.
- Apply these rules to production code, tests, generated templates, and plugin source.

## Public API documentation

Every public API must have complete applicable XML documentation in en-US. Document behavior and contracts using applicable `summary`, `remarks`, `param`, `returns`, `typeparam`, `value`, `exception`, `example`, `see`, `seealso`, and `inheritdoc` tags.

## Plugins and API namespaces

- Stable plugin contracts remain under `src/RemoteCommerce.Plugin.Abstractions`.
- Plugin REST APIs use `/api/rp/vX/<plugin_controller>`.
- RemoteCommerce/WooCommerce-compatible APIs use `/api/rc/vX`.
- Plugin lifecycle remains restart-based.
- Plugin packages must not reference concrete host implementation details.

## Git

- Maintain exactly one open PR.
- Do not create parallel stage PRs.
- Preserve linear history.
- Do not merge unless explicitly requested.
- Keep the active Stage PR draft until repository owner validation is complete.

## Validation

Every stage must be clean-buildable, testable, and packable. Architectural boundaries and dependency direction must be validated in addition to compiler/test success.
