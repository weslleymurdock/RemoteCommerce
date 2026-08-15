# AI implementation instructions

- Act as a senior .NET 10 engineer.
- Inspect `modules/woocommerce` when mapping WooCommerce concepts, but do not copy PHP implementation details into the .NET architecture.
- Preserve the current single-project host until an explicit architectural refactoring task extracts class libraries.
- Organize the host internally into explicit `Domain`, `Application`, and `Infrastructure` boundaries so those folders can later become class library projects with root namespace `RemoteCommerce`.
- Domain must not depend on Application or Infrastructure.
- Application must depend only on Domain and Application abstractions, never directly on Infrastructure implementations.
- Infrastructure owns persistence, repositories, DbContexts, storage providers, and external integrations.
- Use primary constructors where they improve clarity.
- Use DI for all application services and plugin capabilities.
- Prefer async APIs for I/O and EF Core operations.
- Changes must be incremental, buildable, and testable.
- Every public API must include complete applicable XML documentation in en-US.
- Preserve existing OpenAPI/Scalar configuration when extending controllers.

## Feature folder structure

- Every Application feature follows this canonical structure when the concern exists:
  - `src/Application/Feature/Abstractions`
  - `src/Application/Feature/Commands`
  - `src/Application/Feature/Handlers`
  - `src/Application/Feature/Queries`
  - `src/Application/Feature/Requests`
  - `src/Application/Feature/Resources`
  - `src/Application/Feature/Results`
  - `src/Application/Feature/Validators`
- In the current host project these map under `src/RemoteCommerce/Application/Feature`.
- Do not create feature commands, queries, validators, results, or resources in global Application folders.
- Domain features belong under `src/RemoteCommerce/Domain/<Feature>`.
- Infrastructure features belong under `src/RemoteCommerce/Infrastructure/<Feature>` with persistence implementations behind repository/provider abstractions.
- Future class library extraction must preserve root namespace `RemoteCommerce`.

## Canonical request/command/query/result flow

Endpoints must receive an operation-specific Request object, never a MediatR Command or Query directly through HTTP body, form, route binding, or another transport binding mechanism.

For example, an endpoint receives `CreateProductRequest`.

The corresponding `CreateProductCommand` or `CreateProductQuery` receives that Request instance in its constructor and maps the Request values into the command/query's use-case values.

Controllers dispatch the mapped Command/Query through `IMediator`.

Application handlers return the standard `Result` when the operation has no response body and `Result<T>` when the response contains a body.

Controllers map `Result`/`Result<T>` to the HTTP response. Controllers must not expose EF entities, DbContexts, repositories, or provider types.

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

- Controllers receive operation Requests.
- Commands/Queries receive the corresponding Request instance and map its values.
- Handlers execute after MediatR Behaviors.
- Feature Services coordinate Application and Infrastructure through abstractions.
- Repository contracts are database-agnostic and storage-provider-agnostic.
- Repository implementations are Infrastructure-only and may use `DbContext` or storage providers.
- Domain and Application must not instantiate provider-specific persistence or storage types.

## Exception propagation and global error handling

The complete data flow `Controllers -> Handlers -> Behaviors -> Feature Services -> Repository<T> -> StorageProvider` must be instrumented with `try/catch/finally` where the layer performs executable work requiring exception logging or cleanup.

Every applicable catch must log the relevant context and rethrow the original exception. Catch blocks must not swallow exceptions, silently return fallback values, or translate exceptions into HTTP responses.

Exceptions must propagate upward until handled by the global exception handler.

The global exception handler is responsible for translating all exceptions that can arise from the canonical flow into RFC Problem Details and the appropriate HTTP status code. Known application, validation, authorization, not-found, conflict, persistence, provider, and unexpected exception categories must have explicit mappings where applicable, with a safe fallback for unknown exceptions.

Do not duplicate HTTP exception translation inside controllers, handlers, services, repositories, or storage providers.

## Source formatting

- One C# instruction or method call per source line.
- One logical statement per source line.
- Do not compress multiple statements with semicolons or expression chains solely for brevity.
- One Razor directive per line.
- One HTML or Razor component invocation per line when it has attributes or child content.
- Keep method calls, event callbacks, and executable Razor expressions independently readable.
- Apply this rule to production code, tests, generated templates, and Razor UI.

## Existing plugin rules

- The stable plugin contract is `src/RemoteCommerce.Plugin.Abstractions`.
- The only supported plugin distribution format is `.nupkg`.
- Plugin activation remains restart-based.
- Never mutate the running root service provider to activate a plugin.
- Plugin-specific REST controllers use `/api/rp/vX/<plugin_controller>`.
- WooCommerce/RemoteCommerce controllers use `/api/rc/vX`.
