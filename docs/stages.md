# Implementation Stages

Stages are sequential. Only one stage may be active/open as a PR at a time, and stages start from the latest integrated `main`.

## Roadmap

1. Foundation
2. Plugin Runtime
3. NuGet Plugin Packaging and Template Tooling
4. Host Installation and Administration
5. Site, Identity, Configuration, Secrets, and Localization Foundation
6. Database Provider Strategy and Media Storage
7. Plugin Persistence Compatibility
8. Product Catalog
9. Customers, Cart, and Checkout
10. Orders and Payments
11. Shipping, Taxes, and Store Operations
12. WooCommerce-Compatible REST API
13. Storefront and Theme/Extension Model
14. Multi-Store Federation
15. Runtime Plugin Hot Reload
16. Production Readiness

## Global rules

- .NET 10 and repository-approved package versions are used.
- Public APIs receive complete en-US XML documentation.
- C# imports live in `GlobalUsings.cs`; Razor imports live in `_Imports.razor`.
- MediatR 12.5.0 is mandatory for application commands, queries, notifications, and behaviors.
- FluentValidation runs through the validation behavior.
- Mutating persistence uses the transactional behavior.
- Mutable persistence uses soft-delete and operation history.
- Exactly one Stage branch and one PR may be open; PRs are not merged unless explicitly requested.

## Stage 01 — Foundation

Established the .NET 10 host, Blazor Server UI, ASP.NET Core controllers, EF Core foundation, MudBlazor, plugin abstractions, and repository conventions.

**Exit condition:** host foundation builds and supports later stages.

## Stage 02 — Plugin Runtime

Implemented plugin discovery, restart-based activation, DI registration, controller application parts, plugin Razor routing, dependencies, lifecycle state, and `/api/rp/v1` plugin APIs.

**Exit condition:** installed plugins load after restart and can expose services, controllers, and Razor pages.

## Stage 03 — NuGet Plugin Packaging and Template Tooling

Implemented the `dotnet` plugin template tool, resource-based templates, `.nupkg` layout, README/LICENSE/manifest packaging, and independently buildable generated plugins.

**Exit condition:** generated plugins build and pack as installable packages.

## Stage 04 — Host Installation and Administration

Implemented package upload/validation, installation/update/enable/disable/uninstall, dependency validation, retained versions, lifecycle diagnostics, integrity hashing, restart orchestration, and the plugin administration UI.

**Exit condition:** administrators can manage plugins without mutating the root DI container after build.

## Stage 05 — Site, Identity, Configuration, Secrets, and Localization Foundation

Implemented site configuration, Identity-backed authentication, JWT/refresh sessions, authorization policies, secret-provider boundary, localization resources, MediatR 12.5.0 pipeline behaviors, soft-delete, and operation history.

**Exit condition:** the application can be configured and administered through authenticated UI/API flows with transactional persistence and reusable security/persistence boundaries.

## Stage 06 — Database Provider Strategy and Media Storage

Implemented `IDatabaseProvider`, `DatabaseTopology`, SQL Server provider selection, primary/replica setup boundaries, `ISecretProvider` connection-string resolution, provider-aware design-time persistence, filesystem media storage, and MongoDB/GridFS media storage.

**Exit condition:** the host selects database/media technology through stable contracts without leaking provider-specific APIs into Application/domain boundaries.

## Stage 07 — Plugin Persistence Compatibility

**Status: implementation in Draft PR #9; build and automated tests have passed after CI fixes. Final packaging/runtime validation is being completed in this PR.**

### Persistence contract

- Added `IRemoteCommercePluginPersistence` and `IPluginPersistenceBuilder`.
- Plugins may own a `DbContext`, entities, EF configurations, and migrations in the plugin assembly.
- Plugins never receive `CommerceDbContext` or arbitrary connection strings.
- Provider selection remains host-owned through `IDatabaseProvider` and `DatabaseTopology`.
- Plugin schema names are deterministic and derived from stable plugin IDs.
- Plugin migration history is isolated by plugin schema.
- Plugin contexts can participate in the active relational transaction when the current store database supports it.
- Plugin entities can participate in the reusable soft-delete and operation-history infrastructure.
- EF compatibility continues to use the existing manifest `efCoreVersion` field.
- Package validation remains metadata-only and does not execute migrations or plugin code.

### Lifecycle and migration behavior

```text
install/update
    ↓
package + manifest + EF compatibility validation
    ↓
persist installation
    ↓
restart
    ↓
plugin activation
    ↓
discover PluginDbContext + migrations
    ↓
apply pending migrations
    ↓
Loaded
```

Migration failure leaves the plugin inactive, persists lifecycle diagnostics, and can be retried after restart. Uninstall does not delete plugin data; purge remains a separate future administrative operation.

### Reference plugins used for validation

- **RemoteSEO** — deterministic SEO analysis for rendered page/product representations, persisted per store, with `/api/rp/v1/remote-seo` and `/remote-seo`.
- **RemoteAdSense** — public AdSense placement metadata, markup contract, storefront integration, `/api/rp/v1/remote-adsense`, and `/remote-adsense`.
- **RemoteVisitors** — anonymous visitor identity, thirty-minute visit sessions, individual access tracking, aggregate statistics, `/api/rp/v1/remote-visitors`, and `/remote-visitors`.

All three reference plugins own their EF context and migration assembly and declare `efCoreVersion: 10.0`.

### Operational validation surface

Added an administrator-only `/admin/logs` viewer backed by structured file logging. Every application record begins with:

`[DATETIME][LEVEL][NAMESPACE.CLASS][MESSAGE]`

Visitor tracking is failure-tolerant and does not block storefront navigation. AdSense integration uses public placement metadata and never stores secrets.

### CI/package validation

The plugin solution builds Sample, RemoteSEO, RemoteAdSense, and RemoteVisitors. CI packs all four plugin packages and validates their `.nupkg` artifacts while preserving the existing test reporter and coverage summary steps.

**Exit condition:** persistence-capable and non-persistent plugins remain installable, provider-independent, migration-capable, transaction-compatible, soft-delete/history-compatible, and package-valid, with the three reference plugins exercising the persistence boundary and the application log viewer providing an operational validation surface.

## Stage 08 — Product Catalog

Reserved for product/catalog domain implementation. Not implemented by Stage 07.

## Stage 09 — Customers, Cart, and Checkout

Reserved. Not implemented by Stage 07.

## Stage 10 — Orders and Payments

Reserved. Not implemented by Stage 07.

## Stage 11 — Shipping, Taxes, and Store Operations

Reserved. Not implemented by Stage 07.

## Stage 12 — WooCommerce-Compatible REST API

Reserved. `/api/rc/v1` remains unused by Stage 07.

## Stage 13 — Storefront and Theme/Extension Model

Reserved. Stage 07 does not implement storefront themes or extension contracts.

## Stage 14 — Multi-Store Federation

Reserved. Stage 07 preserves one application instance = one store = one exclusive database.

## Stage 15 — Runtime Plugin Hot Reload

Reserved. Stage 07 continues to require restart-based activation.

## Stage 16 — Production Readiness

Reserved for future production hardening and operational requirements.
