# Agent Instructions

## Scope

These instructions apply to work under `RemoteCommerce` and complement the root `AGENTS.md` and `.github/instructions.md`.

## Workflow

1. Read `AGENTS.md` and this directory before changing code.
2. Read the current target branch and its open PR before implementing changes.
3. Maintain exactly one open PR at a time.
4. Create the next work branch from the current `main` only after the previous PR has been successfully integrated.
5. Keep integration history linear; prefer fast-forward/rebase strategies.
6. Do not merge a PR unless the user explicitly asks for the merge.
7. After a PR is successfully integrated and required jobs pass, delete its work branch.
8. Historical branches may exist only when needed for audit purposes; do not revive superseded branches.
9. Implement one stage/PR at a time and keep each stage testable.

## Architecture

- Keep Domain, Application, and Infrastructure as explicit architectural boundaries inside the current host project.
- Organize each feature consistently across those boundaries.
- Prepare the physical layout for future class library extraction with root namespace `RemoteCommerce`.
- Domain must not reference Application or Infrastructure.
- Application must depend on Domain and abstractions, not Infrastructure implementations.
- Infrastructure owns persistence implementations, repositories, DbContexts, storage providers, and external integrations.
- Controllers are transport adapters and must not contain business or persistence rules.

## Application feature layout

Every Application feature must follow this structure when the concern exists:

- `src/Application/Feature/Abstractions`
- `src/Application/Feature/Commands`
- `src/Application/Feature/Handlers`
- `src/Application/Feature/Queries`
- `src/Application/Feature/Requests`
- `src/Application/Feature/Resources`
- `src/Application/Feature/Results`
- `src/Application/Feature/Validators`

In the current host, use the equivalent `src/RemoteCommerce/Application/Feature/...` path until the class library extraction is explicitly performed.

## Data flow

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

- Controllers receive Requests and dispatch MediatR Commands or Queries.
- MediatR Handlers execute use cases after configured Behaviors.
- Feature Services coordinate application and infrastructure operations through abstractions.
- Repository contracts are provider-independent.
- Repository implementations are Infrastructure-only.
- DbContext and storage provider types never cross into Domain or Application contracts.

## Implementation

- Target .NET 10.
- Use Blazor Server/Blazor Web App server interactivity plus ASP.NET Core controllers in the main project.
- Use EF Core with SQL Server for persistence.
- Use MudBlazor for UI.
- Public APIs require complete XML documentation in en-US.
- Plugins are NuGet packages and must be loadable after installation and application restart.
- Plugin Razor assemblies must be registered with Blazor routing through `AdditionalAssemblies`; do not call `AddAdditionalAssemblies` on `IRazorComponentsBuilder`.
- Plugin controllers are MVC application parts and use `/api/rp/vX/<plugin_controller>`.
- WooCommerce-compatible controllers use `/api/rc/vX`.

## Source formatting

- One C# instruction or method call per source line.
- One Razor directive per line.
- One HTML/Razor component invocation per line when it has attributes or child content.
- Keep executable Razor expressions and event callbacks independently readable.
- Apply these formatting rules to production code, tests, templates, and generated source.

## Validation

Every stage must have a buildable/testable checkpoint. Prefer isolated plugin builds as well as the main solution build when plugin tooling changes. Validate architecture and dependency direction, not only compilation and tests.
