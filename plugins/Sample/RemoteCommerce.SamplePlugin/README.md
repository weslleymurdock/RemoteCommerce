# RemoteCommerce

RemoteCommerce is a .NET 10 single-project commerce platform inspired by the extensibility model of WordPress + WooCommerce, implemented with Blazor Interactive Server, ASP.NET Core controllers, EF Core/SQL Server and MudBlazor.

The repository contains the original WooCommerce source as a submodule under `modules/woocommerce`; it is used as a reference for domain concepts, not as a runtime dependency.

## Architecture

```text
RemoteCommerce host (single ASP.NET Core project)
├── Blazor UI (Interactive Server)
├── Controllers / OpenAPI + Scalar
├── Application services
├── EF Core + SQL Server
└── Plugin runtime
    ├── RemoteCommerce.Plugin.Abstractions SDK
    ├── .nupkg package validation
    ├── Installation state
    └── Startup activation into DI
```

Every plugin is distributed as a `.nupkg` containing `plugin.manifest.json` at the package root and the entry assembly under `lib/net10.0/`. Installation persists the package state, while activation occurs only after the application restarts. The runtime never attempts to mutate the root DI container after `builder.Build()`.

## Incremental implementation stages

- `stage/01-foundation` — .NET 10 host, Blazor Server UI, controllers, EF Core/SQL Server registration, MudBlazor and the initial plugin boundary.
- `stage/02-plugin-runtime` — persistent plugin installations, validation and startup activation.
- `stage/03-nupkg-plugin-system` — stable plugin SDK, `.nupkg` distribution, package installation API, MudBlazor plugin management UI, enable/disable/uninstall lifecycle and reference plugin package.
- `stage/04-commerce-core` — product/catalog/order foundations modeled from WooCommerce concepts.

Each stage is intended to be pulled and validated independently before continuing.

## Stage 03 validation

Prerequisites:

- .NET 10 SDK
- SQL Server LocalDB or another SQL Server instance

Run:

```bash
git submodule update --init --recursive
dotnet restore RemoteCommerce.slnx
dotnet build RemoteCommerce.slnx --configuration Release
dotnet pack plugins/Sample/RemoteCommerce.SamplePlugin/RemoteCommerce.SamplePlugin.csproj --configuration Release --output ./artifacts
dotnet run --project src/RemoteCommerce
```

Then open `/plugins`, select `RemoteCommerce.SamplePlugin.1.0.0.nupkg`, install it, and restart the application. After restart the plugin loader should discover the persisted installation and register the sample plugin services before the final DI container is built.

The API is available under `/api/v1/plugins`, and the non-production OpenAPI/Scalar reference is available under `/s/rc`.

## Development rules

See `AGENTS.md`, `.github/instructions.md` and `.github/skills/remotecommerce/SKILL.md`.
