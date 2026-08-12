# RemoteCommerce engineering rules

## Stack
- .NET 10 / ASP.NET Core / Blazor Web App using Interactive Server.
- Controllers are hosted by the same single project as the Blazor UI.
- EF Core + SQL Server is the persistence boundary.
- MudBlazor is the UI component library.
- Plugins are trusted, versioned extensions loaded before the application host is built.

## Architecture rules
- Prefer primary constructors for services and infrastructure types.
- Prefer dependency injection over service location or static state.
- Keep plugin contracts in `Plugins/Abstractions`; host implementation stays in `Plugins`.
- Never load an installed plugin into an already-running service provider. Installation is transactional; activation happens after the next process restart.
- Plugin discovery must be deterministic and failures must not prevent the host from starting; failed plugins are reported through structured logging.
- Do not let plugin assemblies reference internal host implementation details. Expose capabilities through explicit contracts.
- EF Core entities and DbContexts belong under `Infrastructure/Persistence` or a domain-specific extension boundary.
- Controllers are thin HTTP adapters; application behavior belongs in DI services.

## Public API documentation rule
- Every public API introduced by RemoteCommerce must have XML documentation comments written in en-US.
- Document every possible XML documentation element applicable to the API: `summary`, `remarks`, `param`, `returns`, `value`, `typeparam`, `exception`, `example`, `see`, `seealso`, and `inheritdoc` where applicable.
- Public types, constructors, methods, properties, fields, events, delegates, interfaces, enum members, and public extension methods are included in this rule.
- XML documentation must describe behavior and contracts rather than restating identifiers.
- Configure the compiler to generate XML documentation and treat missing public documentation as a build error.

## Validation
Every stage must build from a clean checkout. Add automated tests before introducing non-trivial business behavior.
