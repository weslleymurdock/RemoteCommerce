# Agent Instructions

These instructions apply to work under RemoteCommerce and complement `AGENTS.md` and `.github/instructions.md`.

## Workflow

1. Read repository rules, the target branch, and its open PR before changing code.
2. Maintain exactly one open PR.
3. Work only on the active Stage branch until that stage is integrated.
4. Do not merge unless explicitly requested.
5. Preserve linear integration history.

## Architecture

The repository remains a single host project for concrete Domain, Application, Infrastructure, Presentation, and Plugin Runtime implementations.

The only future shared class library is `RemoteCommerce.Abstractions`, with `RootNamespace=RemoteCommerce`. It contains only contracts/models/non-concrete code and preserves the logical namespaces already used by the host.

Do not create separate future Domain/Application/Infrastructure class library projects unless explicitly requested later.

## Application features

Use:

- `src/Application/Feature/Abstractions`
- `src/Application/Feature/Commands`
- `src/Application/Feature/Handlers`
- `src/Application/Feature/Queries`
- `src/Application/Feature/Requests`
- `src/Application/Feature/Resources`
- `src/Application/Feature/Results`
- `src/Application/Feature/Validators`

Current host path: `src/RemoteCommerce/Application/Feature/...`.

## Data flow

`Controllers(Requests) -> MediatR Commands/Queries -> Behaviors -> Feature Services -> Repository<T> -> DbContext|StorageProvider`.

Controllers never receive Commands/Queries from transport binding. Commands/Queries receive the operation Request instance in their constructors and map its values into use-case data. Handlers return `Result` or `Result<T>`.

## Exceptions

Applicable flow layers use `try/catch/finally` for logging/cleanup. Catches log and rethrow. HTTP translation occurs only in the global exception handler, which returns Problem Details and appropriate status codes.

## Source formatting

One C# instruction or method call per line. One logical statement per line. One Razor directive per line. One HTML/Razor component invocation per line when attributes or child content are present. Apply to production, tests, templates, and generated source.

## Validation

Every stage must build, test, and pack successfully. Validate architecture and dependency direction, not only compilation.
