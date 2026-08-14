# Architecture

RemoteCommerce is a .NET 10 ASP.NET Core application with Interactive Server Blazor, EF Core persistence, MediatR application workflows, MudBlazor administration, and a restart-based runtime plugin model.

## Host boundaries

```text
RemoteCommerce
├── Components/                 Blazor UI and administration
├── Application/                commands, queries, validators, contracts
├── Domain/                     host-owned domain models
├── Infrastructure/
│   ├── Persistence/            EF Core, provider strategy, history, soft-delete
│   ├── Logging/                structured application file logging
│   ├── Media/                  filesystem and MongoDB/GridFS providers
│   └── Security/               JWT/secret boundaries
├── Plugins/                    runtime loading, lifecycle, persistence orchestration
└── Plugin.Abstractions/        stable SDK contract consumed by plugins
```

The host owns `CommerceDbContext`, `IDatabaseProvider`, `DatabaseTopology`, `ISecretProvider`, `TransactionalBehavior`, and host operation history. A plugin never receives `CommerceDbContext`, arbitrary connection strings, `SqlConnection`, `SqlCommand`, MongoDB types, or provider-specific infrastructure contracts.

## Application pipeline

Commands and queries use MediatR 12.5.0. Controllers are HTTP adapters and dispatch application work through MediatR. FluentValidation executes through `ValidationBehavior`. `LoggingBehavior` supplies application request diagnostics and `TransactionalBehavior` owns write transactions. A command that mutates persistence either commits all participating relational changes or rolls them back.

## Database provider strategy

`IDatabaseProvider` is the stable Application persistence contract. Infrastructure selects the implementation through `DatabaseProviderResolver`. SQL Server is the initial provider. `DatabaseTopology.Single` represents one writable store database and `PrimaryReplica` represents one writable primary plus provider-defined replicas.

Connection strings are deployment secrets resolved through `ISecretProvider`. Plugins consume provider configuration indirectly through `IPluginPersistenceBuilder`; they cannot select another store database or supply arbitrary connection strings.

## Plugin persistence

Plugin persistence is deliberately separate from host persistence:

```text
PluginEntry
    │
    └── IRemoteCommercePluginPersistence
             │
             ▼
      IPluginPersistenceBuilder
             │
             ▼
        PluginDbContext
             │
             ▼
      IDatabaseProvider
             │
             ▼
   current store database
```

A persistence-capable plugin owns its `DbContext`, entities, EF configurations, and migrations in its own assembly. The host only receives the context type and migration assembly through the provider-independent abstraction.

The stable plugin identifier determines the relational schema. For example `remote_seo` maps to `rc_plugin_remote_seo`. Display names are never used to derive database identifiers. Plugin migration history uses the same deterministic schema and the host-selected provider.

The host applies provider configuration when creating a plugin context. SQL Server configuration is not copied into plugin projects. Future relational providers can implement the same `IDatabaseProvider` contract without changing plugin code.

When a host `CommerceDbContext` transaction is active, the runtime can attach the plugin context to the same relational transaction. This is limited to the current store database and does not create distributed transactions. Cross-database atomicity is not promised.

Plugin mutation entities can implement `IPluginSoftDeletable`. The runtime persistence infrastructure applies the same soft-delete policy used by the host, and plugin contexts use query filters to exclude disabled records.

Plugin changes participate in the reusable operation-history interceptor. History is associated with the plugin identity and records entity, operation, previous/new state, actor, correlation/request context, and UTC timestamp with sensitive values redacted.

## Plugin lifecycle and migrations

Installation follows:

```text
upload → package validation → manifest validation → compatibility validation
       → persist installation → restart → activation
       → persistence initialization/migration → Loaded
```

Package validation is metadata-only. It never instantiates plugin code, creates a `DbContext`, or executes migrations.

During activation, the runtime discovers `IRemoteCommercePluginPersistence`, configures the declared context, discovers migrations from the plugin assembly, and applies pending migrations. Migration failure persists lifecycle diagnostics and prevents the plugin from reaching `Loaded`; restart provides a retry path. Uninstall removes runtime/package state but does not purge plugin data. Destructive purge is intentionally outside normal uninstall.

Plugin EF compatibility uses the existing `plugin.manifest.json` `efCoreVersion` field. Plugins without persistence leave this field null and continue to work. Persisted plugins declare the supported EF Core major/minor version and are rejected when incompatible.

## Plugin packaging

A plugin package contains `plugin.manifest.json`, README/LICENSE, the `lib/net10.0` entry assembly, and required dependencies. Database files, connection strings, and secrets are never packaged.

`RemoteCommerce.Plugins.slnx` builds the sample plugin plus the three Stage 07 reference plugins. CI also packs all four and validates their `.nupkg` artifacts. The template continues to support both non-persistent and persistence-capable generated plugins.

## Stage 07 reference plugins

### RemoteSEO

`RemoteSEO` analyzes rendered page/product representations using route, title, meta description, canonical URL, and content. It calculates a deterministic score, records recommendations, and persists each analysis in `rc_plugin_remote_seo`. Its API is `/api/rp/v1/remote-seo/analyze` and its interactive plugin page is `/remote-seo`.

### RemoteAdSense

`RemoteAdSense` stores public Google AdSense placement metadata, never publisher secrets. It provides placement management and render-markup endpoints under `/api/rp/v1/remote-adsense`. The storefront integration script loads configured public placement metadata and Google AdSense client script only when placements exist. Its interactive plugin page is `/remote-adsense`.

### RemoteVisitors

`RemoteVisitors` distinguishes three concepts: a **visitor** is a long-lived anonymous browser identity, a **visit** is a session separated by thirty minutes of inactivity, and an **access** is an individual tracked request/page access. Network information is hashed before persistence. It exposes tracking and aggregate statistics under `/api/rp/v1/remote-visitors`, persists data in `rc_plugin_remote_visitors`, and provides `/remote-visitors` for plugin validation.

The visitor integration script is intentionally failure-tolerant: telemetry failures never block storefront navigation.

## Internal application log viewer

The host writes application logs to `App_Data/logs/application.log` using the mandatory format:

```text
[DATETIME][LEVEL][NAMESPACE.CLASS][MESSAGE]
```

The file logger uses UTC timestamps and the logger category as the namespace/class segment. Exceptions append type/message metadata without changing the required prefix. `/admin/logs` is administrator-only and displays recent formatted records through `ApplicationLogReader`.

The viewer is an operational validation surface, not a replacement for external observability/SIEM tooling. Secrets are not intentionally written by the logging infrastructure.

## Soft-delete and operation history

Host and plugin mutable persistence use the same soft-delete contract. Operation history is infrastructure-owned and independent of controllers. Plugin history is recorded in the host operation-history boundary with plugin identity and redaction.

## API namespaces

- `/api/rp/v1/...` is reserved for plugin APIs.
- `/api/rc/v1/...` remains reserved for future WooCommerce-compatible APIs.
- `/api/identity/...` is the RemoteCommerce-owned authentication boundary.

No Stage 07 implementation introduces Product Catalog, Customers, Cart, Checkout, Orders, Payments, Shipping, Taxes, WooCommerce REST, storefront themes, federation, or hot reload.
