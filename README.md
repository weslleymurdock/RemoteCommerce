# RemoteCommerce

RemoteCommerce is a .NET 10 single-project commerce platform inspired by the extensibility model of WordPress + WooCommerce, implemented with Blazor Interactive Server, ASP.NET Core controllers, EF Core/SQL Server and MudBlazor.

The repository contains the original WooCommerce source as a submodule under `modules/woocommerce`; it is used as a reference for domain concepts, not as a runtime dependency. The submodule URL is defined in `.gitmodules`.

## Architecture

```text
RemoteCommerce (single ASP.NET Core project)
├── Blazor UI (Interactive Server)
├── Controllers / HTTP API
├── Application services
├── EF Core + SQL Server
└── Plugin runtime
    ├── Manifest contract
    ├── Installation state
    ├── Package validation
    └── Startup activation into DI
```

A plugin is installed first and activated only after the application restarts. The runtime never attempts to mutate the root DI container after `builder.Build()`.

## Incremental implementation stages

- `stage/01-foundation` — .NET 10 host, Blazor Server UI, controllers, EF Core/SQL Server registration, MudBlazor and the initial plugin manifest/DI boundary.
- `stage/02-plugin-runtime` — persistent plugin installations, package validation and a test plugin that proves restart activation.
- `stage/03-plugin-management-ui` — MudBlazor plugin management screens and installation workflow.
- `stage/04-commerce-core` — product/catalog/order foundations modeled from WooCommerce concepts.

Each stage is intended to be pulled and validated independently before continuing.

## Stage 01 validation

Prerequisites:

- .NET 10 SDK
- SQL Server LocalDB (or another SQL Server instance)

Run:

```bash
dotnet restore
dotnet build RemoteCommerce.sln --configuration Release
dotnet run --project src/RemoteCommerce
```

Then verify:

- `/` renders the MudBlazor dashboard.
- `/api/health` returns HTTP 200 JSON.
- The application starts with the configured SQL Server connection string.

## Development rules

See `AGENTS.md`, `.github/instructions.md` and `.github/skills/remotecommerce/SKILL.md`.
