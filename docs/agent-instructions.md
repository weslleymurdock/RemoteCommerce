# Agent Instructions

## Scope

These instructions apply to work under `RemoteCommerce` and complement the root `AGENTS.md`.

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

## Validation

Every stage must have a buildable/testable checkpoint. Prefer isolated plugin builds as well as the main solution build when plugin tooling changes.
