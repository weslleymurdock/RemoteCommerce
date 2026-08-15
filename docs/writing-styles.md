# Writing Styles

## Code

- Prefer clear, explicit .NET 10 APIs over clever abstractions.
- Prefer file-scoped namespaces.
- Public APIs require complete applicable XML documentation in en-US.
- Use RemoteCommerce domain terminology consistently.

## Feature organization

Application code is organized by feature, with:

- `Abstractions`
- `Commands`
- `Handlers`
- `Queries`
- `Requests`
- `Resources`
- `Results`
- `Validators`

The current host path is `src/RemoteCommerce/Application/Feature/...`.

## Shared abstractions

The only future shared class library is `RemoteCommerce.Abstractions`, with root namespace `RemoteCommerce`. It contains non-concrete contracts and models only and preserves the logical namespaces already used by the host.

Do not place concrete implementations, EF Core, DbContexts, storage providers, ASP.NET Core services, Blazor components, or MudBlazor components in this library.

## Source formatting

- One C# instruction per line.
- One C# method call per line when it is an executable operation.
- One logical C# statement per line.
- One Razor directive per line.
- One HTML/Razor component invocation per line when it has attributes or child content.
- Keep executable Razor expressions and callbacks independently readable.
- Apply globally to production code, tests, templates, and generated source.

## Data flow

Document and implement the canonical flow:

`Requests -> MediatR Commands/Queries -> Behaviors -> Feature Services -> Repository<T> -> DbContext|Storage`.

Controllers never bind Commands/Queries directly. Commands/Queries receive their operation Request and map it into use-case data. Results use `Result` or `Result<T>`.

## Documentation

Write technical documentation in concise English unless another language is requested. Document implemented contracts and constraints, not planned behavior.
