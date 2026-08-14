# RemoteCommerce engineering rules

## Stack

- .NET 10 / ASP.NET Core / Blazor Web App using Interactive Server.
- Controllers are hosted in the same project as the Blazor UI.
- EF Core + SQL Server is the persistence boundary.
- MudBlazor is the UI component library.
- Plugins are distributed as `.nupkg` packages and loaded before the application host is built.
- MediatR + FluentValidation are used for application workflows.

## Architecture rules

- Organize the host by explicit Domain, Application, and Infrastructure boundaries even while those boundaries remain in the current host project.
- The physical layout must be migration-ready so `Domain`, `Application`, and `Infrastructure` can later become independent class library projects with root namespace `RemoteCommerce` without changing feature ownership or dependency direction.
- Domain contains business entities, value objects, domain rules, domain events, and domain abstractions that do not depend on Application, Infrastructure, ASP.NET Core, EF Core, Blazor, MudBlazor, or provider-specific SDKs.
- Application contains use cases, requests, commands, queries, handlers, behaviors, validators, application services, abstractions, resources, and results.
- Infrastructure contains persistence, repository implementations, EF Core, DbContexts, storage providers, external integrations, and infrastructure implementations of Application or Domain abstractions.
- A feature must be organized consistently across Domain, Application, and Infrastructure instead of creating cross-feature utility folders that bypass the feature boundary.
- Application features must use the following canonical structure when the corresponding concern exists:
  - `src/Application/Feature/Abstractions`
  - `src/Application/Feature/Commands`
  - `src/Application/Feature/Handlers`
  - `src/Application/Feature/Queries`
  - `src/Application/Feature/Requests`
  - `src/Application/Feature/Resources`
  - `src/Application/Feature/Results`
  - `src/Application/Feature/Validators`
- The current repository may retain `src/RemoteCommerce/Application`, `src/RemoteCommerce/Domain`, and `src/RemoteCommerce/Infrastructure` while it remains a single host project.
- Future class library extraction must preserve the `RemoteCommerce` root namespace and must not introduce feature-specific root namespaces that encode the current host project layout.
- Application must not depend on Infrastructure implementations directly. Depend on abstractions and resolve implementations through dependency injection.
- Domain must not depend on Application or Infrastructure.
- Infrastructure may depend on Application and Domain abstractions as required by the dependency direction.
- Presentation and Blazor UI are adapters over Application use cases and must not access EF Core or storage providers directly.
- Prefer primary constructors for services and infrastructure types.
- Prefer dependency injection over service location or static state.
- Stable plugin contracts live in `src/RemoteCommerce.Plugin.Abstractions` and are consumed by the host and plugin packages.
- A plugin package must contain `plugin.manifest.json`, `LICENSE.md`, and `README.md` at its root and its entry assembly under `lib/net10.0/`.
- The manifest is the source of truth for package metadata. Installation state remains in EF Core; static package metadata is read from the installed manifest rather than duplicated in the database.
- The manifest `EntryAssembly` must use a package-relative path and `EntryType` must implement `IRemoteCommercePlugin`.
- Never load an installed plugin into an already-running service provider. Installation is transactional; activation happens after the next process restart.
- Enable, disable, and uninstall operations update persistent state; they do not attempt to mutate the current DI container.
- Plugin discovery must be deterministic and failures must not prevent the host from starting; failed plugins are reported through structured logging.
- Plugin packages must not reference internal host implementation details. Expose capabilities through stable SDK contracts.
- EF Core entities and DbContexts belong under Infrastructure persistence boundaries or a domain-specific extension boundary where explicitly required.
- Controllers are thin HTTP adapters. They validate transport concerns, create or map Requests, call MediatR, and map Results to HTTP responses. Controllers must not contain business rules or persistence logic.
- The `rc-plugin` dotnet tool generates one Razor SDK plugin project that can contain Razor pages, controllers, or both. During repository development it uses a ProjectReference to the SDK; released templates use the SDK NuGet package.
- Template source files must live under the tool's `Resources` directory. The generator must not embed generated source files as C# string literals. Placeholders are rendered into resource templates.
- Every generated plugin includes the plugin information Razor page and plugin health controller by default, regardless of the selected optional extension type.
- The default plugin API prefix is `/api/rp/v1`; future plugin API versions must use the newest supported `vX` prefix, starting from `v1`.
- Plugin-specific REST controllers use `/api/rp/vX/<plugin_controller>`.
- Controllers ported from WooCommerce use `/api/rc/vX`; these are distinct namespaces and must not use the plugin `/api/rp` prefix.
- Do not merge pull requests unless the user explicitly requests a merge. PRs remain open for user validation by default.
- Maintain exactly one open pull request for the repository at a time. Do not open a new stage PR while another PR is open. New stages must be based on the latest integrated main history so integration remains linear.
- Preserve a linear integration history. Prefer fast-forward or rebase-based integration; do not introduce merge commits unless explicitly requested by the user.
- After a pull request has been successfully integrated and all required CI/jobs have passed, delete its working branch. Historical stage branches must not be retained after successful integration unless the user explicitly requests preservation.

## Application data-flow rule

The canonical application data flow is:

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

- HTTP controllers receive transport Requests and must not expose infrastructure types.
- Requests are mapped into MediatR Commands or Queries.
- MediatR Handlers execute the use case after registered Behaviors such as logging, validation, and transaction handling.
- Feature Services provide application/infrastructure coordination through explicit abstractions.
- Repository abstractions must remain database-agnostic and storage-provider-agnostic.
- Repository implementations belong to Infrastructure and may use DbContext or a storage provider internally.
- Application and Domain must never instantiate `DbContext`, `SqlConnection`, `SqlCommand`, MongoDB driver types, filesystem providers, or other storage implementations.
- Infrastructure is the only layer allowed to translate repository contracts into EF Core, SQL, MongoDB/GridFS, filesystem, or other provider-specific operations.
- A repository must not leak provider-specific query objects, sessions, contexts, commands, or connection details through its public contract.
- This data flow is the architectural target for all new features and is the required direction for future refactoring.

## C# readability rule

- Keep C# implementations vertically readable.
- Do not combine multiple executable statements, declarations, assignments, conditionals, or side effects on a single source line.
- Use one logical statement per line.
- Expand compound control-flow bodies when they contain more than a single simple statement.
- Expression-bodied members are allowed only when the member consists of a single expression and expanding it would not improve readability.

## Global one-statement-per-line rule

- Every C# instruction or method call must occupy its own source line.
- Do not place multiple statements or method calls on the same line separated by semicolons, braces, operators, or other formatting tricks.
- Every Razor directive must occupy its own line.
- Every Razor component or HTML element must begin and end on separate readable lines unless the element is genuinely a single self-contained tag with no attributes or child content.
- Do not place multiple Razor components, HTML tags, attributes containing executable expressions, or method calls on the same line merely to reduce line count.
- Each Razor event callback or method invocation must be independently readable.
- These rules apply globally to production code, tests, generated source templates, and Razor UI source.

## Public API documentation rule

- Every public API introduced by RemoteCommerce must have XML documentation comments written in en-US.
- Document every possible XML documentation element applicable to the API: `summary`, `remarks`, `param`, `returns`, `value`, `typeparam`, `exception`, `example`, `see`, `seealso`, and `inheritdoc` where applicable.
- Public types, constructors, methods, properties, fields, events, delegates, interfaces, enum members, and public extension methods are included in this rule.
- XML documentation must describe behavior and contracts rather than restating identifiers.
- Configure the compiler to generate XML documentation and treat missing public documentation as a build error.

## Validation

- Every stage must build from a clean checkout.
- Plugin packages and the template dotnet tool must be packed as part of stage validation.
- Add automated tests before introducing non-trivial business behavior.
- Validate architectural boundaries in addition to compiler and test success.
