# Architecture

```text
RemoteCommerce host (single ASP.NET Core project)
├── Blazor UI (Interactive Server)
├── Controllers / OpenAPI + Scalar
├── Application
│   ├── Feature commands / queries / notifications
│   ├── Feature handlers and FluentValidation validators
│   ├── Application pipeline behaviors
│   ├── Site/application settings
│   ├── ASP.NET Core Identity stores + authorization
│   ├── Secret provider boundary
│   ├── Localization/resource administration
│   ├── Persistence provider contracts
│   └── Media storage provider contracts
├── Infrastructure
│   ├── EF Core + SQL Server
│   ├── Transactional persistence behavior
│   ├── Database provider selection and topology setup
│   ├── SQL Server replication boundary
│   ├── Filesystem media storage
│   ├── MongoDB/GridFS media storage
│   └── JWT authentication implementation
├── Plugin runtime
|   ├── RemoteCommerce.Plugin.Abstractions SDK
|   ├── .nupkg package validation
|   ├── Installation/version/dependency state
|   ├── Restart-required orchestration
|   └── Startup activation into DI
└── Tools runtime
```

Every plugin is distributed as a `.nupkg` containing `plugin.manifest.json` at the package root and the entry assembly under `lib/net10.0/`. Installation persists the package state, while activation occurs after the application restarts. The runtime never attempts to mutate the root DI container after `builder.Build()`.

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

## Application request pipeline

Application use cases are organized by feature. Commands, queries, and notifications are kept in their respective `Commands`, `Queries`, and `Notifications` namespaces. Their handlers live under the feature `Handlers` namespace and applicable FluentValidation validators live under `Validators`.

Controllers are thin HTTP adapters and dispatch application work through `IMediator.Send(...)`. They do not contain application orchestration or persistence logic. A Razor feature's controller is the HTTP code-behind boundary for that feature and delegates its work to the corresponding MediatR request/handler rather than implementing the use case itself.

The common pipeline is:

```text
Controller
    ↓
IMediator.Send
    ↓
LoggingBehavior
    ↓
ValidationBehavior
    ↓
TransactionalBehavior
    ↓
Handler
    ↓
Persistence
```

`ValidationBehavior` belongs to `Application.Common.Behaviors`. `TransactionalBehavior` belongs to `Infrastructure.Common.Behaviors` because its responsibility is EF Core transaction management. Transactional commands commit successful persistence and roll back on exceptions; read-only queries do not receive write transactions by default.

All C# projects use `<ImplicitUsings>enable</ImplicitUsings>` and project-level `GlobalUsings.cs` files for namespace imports. Namespace imports are maintained as organized `global using` directives in those files; ordinary `using` directives and `global using` directives are not placed in feature/source files. Razor `@using` and `@inject` directives are centralized in `_Imports.razor`; page-specific `@page`, `@attribute`, `@inherits`, and `@implements` remain on the individual component.

## Site and deployment configuration

`IConfiguration` represents deployment/host configuration: connection strings, infrastructure settings, environment variables, and references to deployment-managed secrets. It is not the persistence mechanism for values that an administrator edits through the application UI.

Persistent application/site settings are stored in the exclusive store database and are exposed through `ISiteSettingsService` and the typed `SiteSettingsModel`. The initial settings include site name, description, public URL, time zone, default culture, and locale. Defaults are safe and validated before persistence.

This separation keeps the deployment boundary independent from future database, media, payment, shipping, and federation provider configuration. Application settings may later be extended with provider selections without turning `IConfiguration` into an administrator-editable database.

## Identity and authorization

Authentication and user persistence use ASP.NET Core Identity only for its EF Core schema, `UserManager<TUser>`, `RoleManager<TRole>`, password hashing, lockout, security stamps, and related stores. RemoteCommerce does not map ASP.NET Core Identity API endpoints or Identity Account/Razor Pages.

RemoteCommerce exposes its own `IdentityController` and MudBlazor login, setup, recovery, registration, profile, and two-factor components. The controller dispatches identity requests such as `LoginCommand`, `BootstrapAdministratorCommand`, `RefreshTokenCommand`, `LogoutCommand`, and related requests through MediatR.

Successful authentication produces a signed, short-lived JWT access token together with a refresh token and their respective expiration metadata. Refresh requests validate the refresh token and its lifetime before issuing a new access/refresh pair. The browser administration session stores the access token only in an HTTP-only, secure, same-site cookie. The bearer handler also accepts a standard `Authorization: Bearer` token for API clients.

JWT validation checks signature, issuer, audience, lifetime, user existence, disabled state, and the Identity security stamp. JWT signing configuration is deployment-managed and is never persisted in SQL or exposed through the administration UI.

The first administrator is created through the RemoteCommerce `/api/identity/setup` boundary when the user store is empty. The setup creates the `Administrator` role and baseline permission claims and then establishes the authentication session. The Blazor route boundary checks the persisted setup state before rendering the application: while the Identity store has no users, the setup page is the only available application page; once setup exists, the setup page is unavailable and normal application/admin routes are available. If the user store is subsequently emptied, setup is required again.

No ASP.NET Core Identity API or Identity Account endpoint is part of the application contract.

Authorization is expressed through named policies and permission claims. The `Administrator` role is accepted by the baseline administrative policies, while individual permission claims provide an extension point for more granular roles and future plugin-declared permissions. No WooCommerce-style ACL is introduced in this stage.

The Blazor route boundary uses `AuthorizeRouteView`, and sensitive administration pages additionally declare their required policy. Plugin administration remains protected by the plugin-management policy and therefore continues to work without changing the plugin runtime lifecycle.

## Secrets

Application code consumes deployment-managed sensitive values through `ISecretProvider`. The initial `ConfigurationSecretProvider` delegates to ASP.NET Core configuration, which supports environment variables and other built-in configuration providers without introducing a proprietary secret store.

Secret values are never persisted in the application database and are never displayed by the administration UI. The security status page reports only configured/not-configured state. The contract is intentionally small so future Docker Secrets, Kubernetes Secrets, Azure Key Vault, or other providers can be introduced without changing consumers.

## Audit logging

Administrative security events use an `AuditLog` persistence model and `IAuditLogService` boundary. Site configuration changes and localization imports are persisted with non-secret context. The audit model records actor, operation, resource, result, context, and UTC timestamp and is deliberately not a SIEM or observability implementation.

## Database isolation and multi-store federation

The default deployment model is **one RemoteCommerce application instance per store with one exclusive database per store**. Multiple store databases may reside on the same SQL Server instance, but a store must never share its application database with another store.

A deployment may run several independent store stacks. A future federation plugin may connect independent stores and make them operate as a logical multi-store organization while preserving database isolation. Shared catalog, inventory, configuration, and other capabilities must be synchronized through explicit APIs/events/commands rather than direct cross-database writes.

RemoteCommerce should not introduce a mandatory `TenantId` into every domain entity merely to support this scenario. Database-per-store is the isolation boundary unless a later stage explicitly introduces another hosting model.

## Plugins

A plugin is distributed as a `.nupkg`. During development the generated plugin may reference `RemoteCommerce.Plugin.Abstractions` by project reference; released plugins consume the abstraction as a NuGet package.

The manifest is the package metadata source of truth. It declares identity, version, entry point, host compatibility, optional EF Core compatibility, required README/LICENSE files, and plugin dependencies. Static package metadata is not duplicated into the relational database; the database stores administrative state, integrity hashes, retained versions, dependencies, settings, and lifecycle diagnostics.

Package administration is separated into explicit validation and lifecycle boundaries. `IPluginManifestValidator` checks manifest semantics, `IPluginCompatibilityValidator` checks host/EF compatibility, and `IPluginPackageValidator` inspects the `.nupkg` structure and SHA-256 integrity before extraction. Validation never instantiates or activates the plugin entry point.

The persisted lifecycle distinguishes administrative intent from runtime reality. `PluginDesiredState` records whether the administrator wants a plugin enabled or disabled, while `PluginInstallationState` records states such as `Discovered`, `Validated`, `Installed`, `ActivationPending`, `Disabled`, `Loaded`, and `Failed`. Enable, disable, install, update, rollback, and uninstall operations persist their requested changes and use `IApplicationRestartService` to report that the current DI container remains unchanged until restart.

Plugin dependencies are version-ranged and persisted separately. Installation and update validation rejects missing, disabled, incompatible, duplicate, or circular dependencies. Uninstall rejects a plugin that is still required by another installed plugin. Previous package versions are retained as explicit `PluginVersion` records so rollback can be scheduled without deleting the previous artifact immediately.

The package administration UI is implemented with MudBlazor and provides plugin metadata, version/state, README/LICENSE content, dependency information, validation diagnostics, activation errors, install/update, enable/disable, and uninstall operations.

Plugin controllers are registered as MVC application parts. Plugin Razor components are registered with the Blazor router as additional assemblies.

The current activation model uses application restart because the root DI container and endpoint/routing model are immutable after application build. A later stage should investigate collectible `AssemblyLoadContext`, isolated plugin service scopes/registries, dynamic endpoint refresh, and safe unload/reload so compatible plugins can be installed, enabled, disabled, updated, or removed without restarting the host.

## Database providers

The transactional database is selected through a provider strategy while retaining SQL Server as the initial relational provider. Application and domain contracts do not expose `SqlConnection`, EF provider-specific commands, or other SQL Server APIs.

The Application persistence boundary contains `IDatabaseProvider`, `DatabaseTopology`, and `IDatabaseReplicationProvider`. Infrastructure currently provides `SqlServerDatabaseProvider` and `SqlServerReplicationProvider`. Provider selection is performed by `DatabaseProviderResolver` using deployment configuration and dependency injection. Unknown providers fail configuration resolution rather than silently falling back to an unrelated implementation.

The default configuration is SQL Server with `Single` topology. When `ConnectionStrings` is empty, the SQL Server strategy uses the development LocalDB fallback. When exactly one connection string exists, that endpoint is treated as the primary regardless of its name. When multiple connection strings exist, topology must be explicit; the strategy never assumes that a second connection such as `Reporting` is a replica. `PrimaryReplica` requires explicitly named primary and replica endpoints.

Connection string values are resolved through `ISecretProvider`; `IConfiguration` is used for non-secret provider and topology metadata. Connection strings are never returned by application/UI contracts, persisted in SQL, written to operation history, or intentionally logged.

`CommerceDbContextDesignTimeFactory` resolves the same provider strategy for EF Core design-time operations. This keeps future provider-aware migrations at one explicit boundary while the current SQL Server migration set remains unchanged.

### Database topology and replication

`DatabaseTopology.Single` represents one writable database endpoint. `DatabaseTopology.PrimaryReplica` represents one writable primary and provider-defined read replicas. Multi-primary, multi-master, federation, and cross-store synchronization are outside Stage 06.

Replication is deliberately separate from persistence. `IDatabaseReplicationProvider` represents provider-aware validation and initialization; it is not a generic table-copy service. The SQL Server implementation validates both endpoints and provides the initialization boundary that a later replication plugin can extend. Plugins must consume stable provider contracts rather than host `CommerceDbContext` internals.

A `PrimaryReplica` topology requires setup before normal application use. `DatabaseSetupService` persists only non-secret setup state and coordinates provider validation, replication validation, and initialization. The existing Stage 05 setup gate is extended rather than replaced: identity setup remains the first gate, and database setup is applied immediately after identity initialization. Required, in-progress, and failed setup states keep normal routes blocked; successful configuration releases the application. An interrupted in-progress state is treated as required on the next attempt so setup cannot become permanently stuck.

### Persistence invariants

The existing `CommerceDbContext` remains the authoritative SQL persistence boundary. `TransactionalBehavior` continues to own EF Core transactions; provider selection does not introduce a second unit-of-work or transaction abstraction.

Mutable SQL records continue to use the Stage 05 soft-delete behavior. `CommerceDbContext` converts deletes of `ISoftDeletable` entities into disabled state and normal query filters exclude those records. Administrative/history queries may explicitly use `IgnoreQueryFilters()`.

Operation history remains in the relational database. `CommerceDbContext.SaveChanges` captures serialized before/after state, entity identity/type, operation, UTC timestamp, actor, correlation and request context, and redacts sensitive property names. Mutation and history remain in the same EF Core transaction managed by `TransactionalBehavior`.

## Media storage

Media and large assets use `IMediaStorageProvider` rather than the transactional database provider. The default provider is `FileSystemMediaStorageProvider`, rooted under an application-owned directory. Provider-generated identifiers are opaque GUID values and clients never receive physical paths.

The filesystem provider validates file names and identifiers, stores metadata beside content, uses asynchronous I/O, and limits all access to generated provider identifiers. Directory traversal and arbitrary filesystem reads are rejected.

`MongoGridFsMediaStorageProvider` implements the same `IMediaStorageProvider` contract using MongoDB GridFS. MongoDB is optional and is not the RemoteCommerce transactional database. MongoDB connection credentials are resolved through `ISecretProvider`; database name and bucket are deployment configuration. MongoDB is not contacted merely because its package is installed or because the default filesystem provider is selected.

The MongoDB driver is isolated to Infrastructure. Domain and Application contracts do not reference `MongoDB.Driver`, `GridFSBucket`, BSON documents, or Mongo-specific identifiers. Media provider selection is performed by `MediaStorageProviderResolver` through DI.

## Localization

Localization is a first-class cross-cutting service. The RemoteCommerce `ILocalizer` wrapper is resource-type-aware and sits over the ASP.NET Core `IStringLocalizer<T>` infrastructure while also consulting administratively imported XML resources.

Initial cultures are `en-US` and `pt-BR`. Site configuration supplies the default culture, while ASP.NET Core request localization establishes `CurrentCulture`/`CurrentUICulture`. The configured site culture is used as the lowest-priority application provider so explicit query/cookie/browser preferences can still take precedence.

Administratively imported resources are validated as `.resx`-compatible XML, protected against DTD/external-entity processing, assigned a monotonically increasing version, stored as files under `App_Data/localization`, and represented in SQL Server only by metadata, hash, version, importer, and activation state. `en-US` is the final fallback when a localized key is unavailable. Invalid resources are never activated.

## API namespaces

- `/api/rp/v1/...` is the RemoteCommerce plugin API namespace.
- `/api/rc/v1/...` is reserved for APIs ported from WooCommerce.
- `/api/identity/...` is the host's explicit authentication/account boundary and is not an ASP.NET Core Identity endpoint.
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
