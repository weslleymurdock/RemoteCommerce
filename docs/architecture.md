# Architecture

```text
RemoteCommerce host (single ASP.NET Core project)
├── Blazor UI (Interactive Server)
├── Controllers / OpenAPI + Scalar
├── Application services
├── EF Core + SQL Server
├── Plugin runtime
|   ├── RemoteCommerce.Plugin.Abstractions SDK
|   ├── .nupkg package validation
|   ├── Installation state
|   └── Startup activation into DI
└── Tools runtime 
```

Every plugin is distributed as a `.nupkg` containing `plugin.manifest.json` at the package root and the entry assembly under `lib/net10.0/`. Installation persists the package state, while activation occurs only after the application restarts. The runtime never attempts to mutate the root DI container after `builder.Build()`.

## Product model

RemoteCommerce is a .NET 10 server application intended to provide the core experience of a WordPress + WooCommerce installation while remaining modular through runtime-installable plugins.

## Host

The host is a single main application containing:

- Blazor UI with server-side interactivity.
- ASP.NET Core controllers.
- EF Core and SQL Server persistence.
- MudBlazor presentation components.
- OpenAPI/Scalar for API discovery.
- Plugin discovery, installation, lifecycle, and runtime assembly registration.

## Database isolation and multi-store federation

The default deployment model is **one RemoteCommerce application instance per store with one exclusive database per store**. Multiple store databases may reside on the same SQL Server instance, but a store must never share its application database with another store.

A deployment may run several independent store stacks, for example with Docker Compose or Docker Swarm:

```text
Organization / Federation
├── Store A stack → RemoteCommerce A → StoreA database
├── Store B stack → RemoteCommerce B → StoreB database
└── Store C stack → RemoteCommerce C → StoreC database
```

The architecture must remain federation-ready. A future federation plugin may connect independent stores and make them operate as a logical multi-store organization while preserving database isolation. Shared catalog, inventory, configuration, and other capabilities must be synchronized through explicit APIs/events/commands rather than direct cross-database writes.

The preferred integration model is event/command based:

```text
Store A → Federation integration → Store B
             │
             ├── catalog synchronization
             ├── inventory reservations/transfers
             ├── shared configuration
             └── organization-level policies
```

This model permits stores to remain independently deployable, backed up, upgraded, and recovered. A future control plane may hold organization/store metadata without containing the transactional store domain itself.

RemoteCommerce should not introduce a mandatory `TenantId` into every domain entity merely to support this scenario. Database-per-store is the isolation boundary unless a later stage explicitly introduces another hosting model.

## Plugins

A plugin is distributed as a `.nupkg`. During development the generated plugin may reference `RemoteCommerce.Plugin.Abstractions` by project reference; released plugins consume the abstraction as a NuGet package.

Plugins may consume compatible third-party libraries, including SQL/EF Core libraries when their feature requires persistence. Plugin templates should pin the EF Core version to the backend-compatible version and provide a placeholder plugin `DbContext`/registration boundary so persistence can be added without coupling the plugin to an arbitrary EF version.

The plugin manifest describes identity, package metadata, compatibility, entry point, README, and LICENSE information. Installation is persisted and the plugin becomes active after the host is restarted. The entry point receives the host `IConfiguration` so plugins can consume host defaults without duplicating configuration sources.

Plugin controllers are registered as MVC application parts. Plugin Razor components are registered with the Blazor router as additional assemblies.

The current activation model uses application restart because the root DI container and endpoint/routing model are immutable after application build. A later stage should investigate collectible `AssemblyLoadContext`, isolated plugin service scopes/registries, dynamic endpoint refresh, and safe unload/reload so compatible plugins can be installed, enabled, disabled, updated, or removed without restarting the host. Runtime reload must not leak assemblies, singleton state, endpoints, or background services.

## Database providers

Persistence must be abstracted behind application/provider contracts so the concrete database strategy can be selected through `IConfiguration`, with a documented default when no provider is configured.

The initial relational provider remains SQL Server. A strategy/provider pattern should allow additional providers without leaking provider-specific APIs into domain/application contracts. Provider selection must be validated at startup and migrations must be provider-aware.

A separate document/blob provider boundary should support non-relational assets. MongoDB/GridFS is a candidate provider for media and potentially virtual-product payloads, but it must not become an implicit replacement for the transactional relational store. The provider should be consumed through application services so the domain does not depend directly on MongoDB types.

## Localization

Localization is a first-class cross-cutting service. The preferred abstraction is a RemoteCommerce `ILocalizer` wrapper over `IStringLocalizer<T>`/the ASP.NET Core localization infrastructure, allowing resource selection by resource type while keeping UI/application code independent from the underlying resource mechanism.

Initial cultures are `en-US` and `pt-BR`. The localization system must support loading resource XML files through the administration UI, validation, versioning, and safe replacement without requiring code changes. Resource ownership and fallback behavior must be explicit.

## API namespaces

- `/api/rp/v1/...` is the RemoteCommerce plugin API namespace.
- `/api/rc/v1/...` is reserved for APIs ported from WooCommerce.
- Future versions increment the version segment rather than changing an existing contract.

## Target domain boundaries

The eventual product is organized around WordPress/WooCommerce-equivalent capabilities:

1. Site/application configuration.
2. Users, roles, authentication, and authorization.
3. Plugin lifecycle and administration.
4. Product catalog, categories, attributes, variations, inventory, and media.
5. Customers and addresses.
6. Cart, checkout, orders, taxes, coupons, and shipping.
7. Payments and payment-provider integrations.
8. Storefront/admin UI and navigation.
9. Webhooks, REST APIs, observability, and background processing.
10. Multi-store federation and organization-level synchronization without breaking database-per-store isolation.

Plugins should extend these boundaries without requiring changes to the host for normal feature additions.
