# AI implementation instructions

- Act as a senior .NET 10 engineer.
- Preserve the current single-project host for Domain, Application, Infrastructure, Presentation, and Plugin Runtime implementations.
- The only future shared class library is `src/RemoteCommerce.Abstractions/RemoteCommerce.Abstractions.csproj` with `RootNamespace=RemoteCommerce`.
- `RemoteCommerce.Abstractions` contains only non-concrete contracts and models. It must not contain EF Core, DbContext, provider implementations, ASP.NET Core concrete services, Blazor components, MudBlazor components, or plugin runtime implementations.
- Shared abstraction files must preserve the logical namespace architecture already used by the host.
- Do not plan or create separate future `RemoteCommerce.Domain`, `RemoteCommerce.Application`, or `RemoteCommerce.Infrastructure` assemblies unless explicitly requested later.

## Feature organization

Every Application feature follows:

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

The current host equivalent is `src/RemoteCommerce/Application/Feature/...`.

Domain and Infrastructure features remain under their respective feature folders.

## Request/Command/Query/Result flow

Endpoints receive operation-specific Request objects, never MediatR Commands or Queries through body, form, route binding, or another transport mechanism.

The corresponding Command or Query receives the Request instance in its constructor and explicitly maps its values into use-case data.

Controllers dispatch through `IMediator` and map the returned `Result` or `Result<T>` to HTTP responses.

## Canonical data flow

`Controllers(Requests) -> MediatR Commands/Queries -> Behaviors -> Feature Services -> Repository<T> -> DbContext|StorageProvider`.

Repository contracts are provider agnostic. Repository implementations are Infrastructure-only. Domain/Application never instantiate storage implementations.

## Exception flow

Applicable executable layers use `try/catch/finally` for exception logging/cleanup. Catches log context and rethrow the original exception. Exceptions propagate to the global exception handler.

The global exception handler translates known validation, authorization, not-found, conflict, persistence/provider, and unexpected failures to RFC Problem Details plus appropriate HTTP status codes.

No controller, handler, service, repository, or storage provider performs HTTP exception translation.

## Formatting

- One C# instruction/method call per source line.
- One logical statement per source line.
- One Razor directive per line.
- One HTML/Razor component invocation per line when it has attributes or child content.
- Keep executable Razor expressions and callbacks independently readable.
- Apply globally to production code, tests, generated templates, and plugin source.

## API documentation

Every public API requires complete applicable en-US XML documentation.

## Plugins

- Stable plugin contracts remain in `src/RemoteCommerce.Plugin.Abstractions`.
- Plugin APIs use `/api/rp/vX`.
- RemoteCommerce APIs use `/api/rc/vX`.
- Plugin activation remains restart-based.
