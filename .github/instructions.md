# AI implementation instructions

- Act as a senior .NET 10 engineer.
- Inspect `modules/woocommerce` when mapping WooCommerce concepts, but do not copy PHP implementation details into the .NET architecture.
- Preserve the current single-project host until an explicit architectural refactoring task extracts class libraries.
- Organize the host internally into explicit `Domain`, `Application`, and `Infrastructure` boundaries so those folders can later become class library projects with root namespace `RemoteCommerce`.
- Domain must not depend on Application or Infrastructure.
- Application must depend only on Domain and Application abstractions, never directly on Infrastructure implementations.
- Infrastructure owns persistence, repositories, DbContexts, storage providers, and external integrations.
- The stable plugin contract is the deliberate exception to the host single-project rule and lives in `src/RemoteCommerce.Plugin.Abstractions` so independently built `.nupkg` plugins can consume a stable SDK without referencing host internals.
- Use primary constructors where they improve clarity.
- Use DI for all application services and plugin capabilities.
- Treat plugin installation and plugin activation as separate lifecycle phases.
- The only supported plugin distribution format is `.nupkg`.
- A successful installation writes the package payload and immutable/versioned installation state; activation occurs only after restart; if possible, update later to a runtime install/removal without restart.
- Never attempt to mutate the running root service provider to activate a plugin.
- Avoid reflection in normal application paths; reflection is isolated to the plugin bootstrap boundary.
- Prefer async APIs for I/O and EF Core operations.
- Changes must be incremental, buildable, and testable.
- Every public API must include complete applicable XML documentation in en-US.
- Preserve existing OpenAPI/Scalar configuration when extending controllers.

## Feature folder structure

- Every Application feature uses `Feature` as the feature placeholder and follows this canonical structure when the concern exists:
  - `src/Application/Feature/Abstractions`
  - `src/Application/Feature/Commands`
  - `src/Application/Feature/Handlers`
  - `src/Application/Feature/Queries`
  - `src/Application/Feature/Requests`
  - `src/Application/Feature/Resources`
  - `src/Application/Feature/Results`
  - `src/Application/Feature/Validators`
- In the current host project these map under `src/RemoteCommerce/Application/Feature`.
- Do not create feature commands, queries, validators, results, or resources in a global Application folder.
- Domain features belong under `src/RemoteCommerce/Domain/<Feature>`.
- Infrastructure features belong under `src/RemoteCommerce/Infrastructure/<Feature>` with persistence implementations kept behind repository or provider abstractions.
- Future class library extraction must preserve root namespace `RemoteCommerce`.

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

- Controllers receive Requests and call `IMediator`.
- Handlers execute Commands and Queries after the configured Behaviors.
- Behaviors provide cross-cutting concerns such as logging, validation, and transactions.
- Feature Services coordinate Application and Infrastructure through explicit abstractions.
- Repository contracts are database-agnostic and storage-provider-agnostic.
- Repository implementations are Infrastructure-only and may use `DbContext` or storage providers.
- Domain and Application must not instantiate provider-specific persistence or storage types.

## Source formatting

- One C# instruction or method call per source line.
- One logical statement per source line.
- Do not compress multiple statements with semicolons or expression chains solely for brevity.
- One Razor directive per line.
- One HTML or Razor component invocation per line when it has attributes or child content.
- Keep method calls, event callbacks, and executable Razor expressions independently readable.
- Apply this rule to production code, tests, generated templates, and Razor UI.
