# Architecture

```text
RemoteCommerce host (single ASP.NET Core project)
├── Blazor UI (Interactive Server)
├── Controllers / OpenAPI + Scalar
├── Application services
│   ├── Site/application settings
│   ├── ASP.NET Core Identity + authorization
│   ├── Secret provider boundary
│   ├── Localization/resource administration
│   └── Administrative audit logging
├── EF Core + SQL Server
├── Plugin runtime
|   ├── RemoteCommerce.Plugin.Abstractions SDK
|   ├── .nupkg package validation
|   ├── Installation/version/dependency state
|   ├── Restart-required orchestration
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

## Site and deployment configuration

`IConfiguration` represents deployment/host configuration: connection strings, infrastructure settings, environment variables, and references to deployment-managed secrets. It is not the persistence mechanism for values that an administrator edits through the application UI.

Persistent application/site settings are stored in the exclusive store database and are exposed through `ISiteSettingsService` and the typed `SiteSettingsModel`. The initial settings include site name, description, public URL, time zone, default culture, and locale. Defaults are safe and validated before persistence.

This separation keeps the deployment boundary independent from future database, media, payment, shipping, and federation provider configuration. Application settings may later be extended with provider selections without turning `IConfiguration` into an administrator-editable database.

## Identity and authorization

Authentication and user persistence use ASP.NET Core Identity with `ApplicationUser` and `ApplicationRole` stored in the existing `CommerceDbContext`. Browser authentication uses Identity application cookies. Passwords are hashed by Identity and are never persisted as application secrets.

The first administrator is created through the one-time `/admin/setup` bootstrap endpoint when the user store is empty. The bootstrap creates the `Administrator` role and the baseline permission claims, then signs the administrator in.

Authorization is expressed through named policies and permission claims. The `Administrator` role is accepted by the baseline administrative policies, while individual permission claims provide an extension point for more granular roles and future plugin-declared permissions. No WooCommerce-style ACL is introduced in this stage.

The Blazor route boundary uses `AuthorizeRouteView`, and sensitive administration pages additionally declare their required policy. Plugin administration remains protected by the plugin-management policy and therefore continues to work without changing the plugin runtime lifecycle.

## Secrets

Application code consumes deployment-managed sensitive values through `ISecretProvider`. The initial `ConfigurationSecretProvider` delegates to ASP.NET Core configuration, which supports environment variables and other built-in configuration providers without introducing a proprietary secret store.

Secret values are never persisted in the application database and are never displayed by the administration UI. The security status page reports only configured/not-configured state. The contract is intentionally small so future Docker Secrets, Kubernetes Secrets, Azure Key Vault, or other providers can be introduced without changing consumers.

## Audit logging

Administrative security events use an `AuditLog` persistence model and `IAuditLogService` boundary. Site configuration changes and localization imports are persisted with non-secret context. The audit model records actor, operation, resource, result, context, and UTC timestamp and is deliberately not a SIEM or observability implementation.

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

The manifest is the package metadata source of truth. It declares identity, version, entry point, host compatibility, optional EF Core compatibility, required README/LICENSE files, and plugin dependencies. Static package metadata is not duplicated into the relational database; the database stores administrative state, integrity hashes, retained versions, dependencies, settings, and lifecycle diagnostics.

Package administration is separated into explicit validation and lifecycle boundaries. `IPluginManifestValidator` checks manifest semantics, `IPluginCompatibilityValidator` checks host/EF compatibility, and `IPluginPackageValidator` inspects the `.nupkg` structure and SHA-256 integrity before extraction. Validation never instantiates or activates the plugin entry point. Entry assemblies are inspected only for assembly metadata during package validation; executable activation occurs only during startup.

The persisted lifecycle distinguishes administrative intent from runtime reality. `PluginDesiredState` records whether the administrator wants a plugin enabled or disabled, while `PluginInstallationState` records states such as `Discovered`, `Validated`, `Installed`, `ActivationPending`, `Disabled`, `Loaded`, and `Failed`. Enable, disable, install, update, rollback, and uninstall operations persist their requested changes and use `IApplicationRestartService` to report that the current DI container remains unchanged until restart.

Plugin dependencies are version-ranged and persisted separately. Installation and update validation rejects missing, disabled, incompatible, duplicate, or circular dependencies. Uninstall rejects a plugin that is still required by another installed plugin. Previous package versions are retained as explicit `PluginVersion` records so rollback can be scheduled without deleting the previous artifact immediately.

The package administration UI is implemented with MudBlazor and provides plugin metadata, version/state, README/LICENSE content, dependency information, validation diagnostics, activation errors, install/update, enable/disable, and uninstall operations. A trusted local package source may also be configured through `PluginAdministration:TrustedPackageDirectory`.

Plugin controllers are registered as MVC application parts. Plugin Razor components are registered with the Blazor router as additional assemblies.

The current activation model uses application restart because the root DI container and endpoint/routing model are immutable after application build. A later stage should investigate collectible `AssemblyLoadContext`, isolated plugin service scopes/registries, dynamic endpoint refresh, and safe unload/reload so compatible plugins can be installed, enabled, disabled, updated, or removed without restarting the host. Runtime reload must not leak assemblies, singleton state, endpoints, or background services.

## Database providers

Persistence must be abstracted behind application/provider contracts so the concrete database strategy can be selected through `IConfiguration`, with a documented default when no provider is configured.

The initial relational provider remains SQL Server. A strategy/provider pattern should allow additional providers without leaking provider-specific APIs into domain/application contracts. Provider selection must be validated at startup and migrations must be provider-aware.

A separate document/blob provider boundary should support non-relational assets. MongoDB/GridFS is a candidate provider for media and potentially virtual-product payloads, but it must not become an implicit replacement for the transactional relational store. The provider should be consumed through application services so the domain does not depend directly on MongoDB types.

## Localization

Localization is a first-class cross-cutting service. The RemoteCommerce `ILocalizer` wrapper is resource-type-aware and sits over the ASP.NET Core `IStringLocalizer<T>` infrastructure while also consulting administratively imported XML resources.

Initial cultures are `en-US` and `pt-BR`. Site configuration supplies the default culture, while ASP.NET Core request localization establishes `CurrentCulture`/`CurrentUICulture`. The configured site culture is used as the lowest-priority application provider so explicit query/cookie/browser preferences can still take precedence.

Administratively imported resources are validated as `.resx`-compatible XML, protected against DTD/external-entity processing, assigned a monotonically increasing version, stored as files under `App_Data/localization`, and represented in SQL Server only by metadata, hash, version, importer, and activation state. `en-US` is the final fallback when a localized key is unavailable. Invalid resources are never activated.

## API namespaces

- `/api/rp/v1/...` is the RemoteCommerce plugin API namespace.
- `/api/rc/v1/...` is reserved for APIs ported from WooCommerce.
- Future versions increment the version segment rather than changing an existing contract.

Plugin administration is a host administration API and currently uses `/api/v1/plugins`; it is not part of either plugin `/api/rp` or WooCommerce `/api/rc` namespace.

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
