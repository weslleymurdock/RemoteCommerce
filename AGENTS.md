# RemoteCommerce engineering rules

## Stack

- .NET 10 / ASP.NET Core / Blazor Web App using Interactive Server.
- Controllers are hosted in the same project as the Blazor UI.
- EF Core + SQL Server is the persistence boundary.
- MudBlazor is the UI component library.
- Plugins are distributed as `.nupkg` packages and loaded before the application host is built.
- MediatR + FluentValidation for invoke Application/Infrastructure services from controllers.


## Architecture rules

- Prefer primary constructors for services and infrastructure types.
- Prefer dependency injection over service location or static state.
- Stable plugin contracts live in `src/RemoteCommerce.Plugin.Abstractions` and are consumed by the host and plugin packages.
- A plugin package must contain `plugin.manifest.json`, `LICENSE.md`, and `README.md` at its root and its entry assembly under `lib/net10.0/`.
- The manifest is the source of truth for package metadata. Installation state remains in EF Core; static package metadata is read from the installed manifest rather than duplicated in the database.
- The manifest `EntryAssembly` must use a package-relative path and `EntryType` must implement `IRemoteCommercePlugin`.
- Never load an installed plugin into an already-running service provider. Installation is transactional; activation happens after the next process restart.
- Enable, disable, and uninstall operations update persistent state; they do not attempt to mutate the current DI container.
- Plugin discovery must be deterministic and failures must not prevent the host from starting; failed plugins are reported through structured logging.
- Plugin packages must not reference internal host implementation details. Expose capabilities through the stable SDK contracts.
- EF Core entities and DbContexts belong under `Infrastructure|Persistence` or a domain-specific extension boundary.
- Controllers are thin HTTP adapters; application behavior belongs in DI services.
- Never load arbitrary assemblies from uploaded files without validating the package manifest, path boundaries, target framework location, and plugin contract.
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

## C# readability rule

- Keep C# implementations vertically readable: do not combine multiple executable statements, declarations, assignments, conditionals, or side effects on a single source line.
- Use one logical statement per line and expand compound control-flow bodies when they contain more than a single simple statement.
- Expression-bodied members are allowed only when the member consists of a single expression and expanding it would not improve readability.

## Public API documentation rule

- Every public API introduced by RemoteCommerce must have XML documentation comments written in en-US.
- Document every possible XML documentation element applicable to the API: `summary`, `remarks`, `param`, `returns`, `value`, `typeparam`, `exception`, `example`, `see`, `seealso`, and `inheritdoc` where applicable.
- Public types, constructors, methods, properties, fields, events, delegates, interfaces, enum members, and public extension methods are included in this rule.
- XML documentation must describe behavior and contracts rather than restating identifiers.
- Configure the compiler to generate XML documentation and treat missing public documentation as a build error.

## Validation

Every stage must build from a clean checkout. Plugin packages and the template dotnet tool must be packed as part of stage validation. Add automated tests before introducing non-trivial business behavior.
