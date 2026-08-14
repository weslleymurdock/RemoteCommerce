# Architecture

RemoteCommerce is a .NET 10 ASP.NET Core application with Interactive Server Blazor, EF Core persistence, MediatR application workflows, MudBlazor administration, and a restart-based runtime plugin model.

## Host boundaries

```text
RemoteCommerce
├── Components/                 Blazor UI and administration
├── Application/                commands, queries, validators, contracts
│   ├── Catalog/                product catalog use cases and models
│   └── Presentation/           theme and dynamic menu contracts
├── Domain/                     host-owned domain models
│   ├── Catalog/                products, taxonomy, variants, metadata, media refs
│   └── Shared/                 reusable soft-delete contract
├── Infrastructure/
│   ├── Persistence/            EF Core, provider strategy, history, soft-delete
│   ├── Catalog/                catalog persistence application service
│   ├── Media/                  filesystem and MongoDB/GridFS providers
│   └── Security/               JWT/secret boundaries
├── Plugins/                    runtime loading, lifecycle, persistence orchestration
└── Plugin.Abstractions/        stable SDK contract consumed by plugins
```

The host owns `CommerceDbContext`, `IDatabaseProvider`, `DatabaseTopology`, `ISecretProvider`, `TransactionalBehavior`, and operation history. Plugins never receive `CommerceDbContext`, arbitrary connection strings, provider-specific database objects, or host implementation details.

## Application request pipeline

```text
Controller / Razor page
    ↓
IMediator.Send
    ↓
LoggingBehavior
    ↓
ValidationBehavior
    ↓
TransactionalBehavior
    ↓
Catalog handler
    ↓
ICatalogService
    ↓
CommerceDbContext / provider boundary
```

Controllers remain HTTP adapters. Catalog Razor components dispatch application requests through MediatR and do not access EF Core directly.

## Product Catalog

Stage 08 adds a host-owned catalog model without copying WooCommerce's internal PHP storage model. The core aggregate is `Product`; related entities represent variants, taxonomy, attributes, metadata, and media references.

`Product` supports name, URL slug, optional SKU, descriptions, lifecycle status, product type, prices, currency, optional brand, categories, tags, attributes, variants, metadata, and media references. Product types are `Simple`, `Variable`, `Virtual`, and `Downloadable`. Statuses are `Draft`, `Published`, and `Archived`.

`Category` is hierarchical through `ParentId` and uses restrictive parent deletion to prevent accidental tree corruption. `Brand` and `Tag` have unique slugs. Product and variant SKUs are database-unique. `ProductAttribute` and `ProductAttributeValue` provide extensible values such as Color, Size, and Material without hardcoding attributes into `Product`.

Product metadata is represented by explicit `ProductMetadata` records containing a validated key, scalar/JSON type, and value. Secrets are not a catalog metadata capability. Product media contains only a `MediaId`, role, order, and alternative text; binary content remains owned by `IMediaStorageProvider`.

The catalog uses the existing host `CommerceDbContext` and provider strategy. It does not introduce a second context or provider-specific database API. Catalog entities participate in the shared soft-delete contract and EF query filtering. The Stage 08 migration creates catalog tables under the existing `commerce` schema.

## Catalog API

RemoteCommerce-owned resources use `/api/rc/v1`. Plugin resources remain under `/api/rp/v1`.

Stage 08 provides product collection/detail and administrative mutation endpoints plus category, brand, tag, and attribute resources. Product collections are paged with a safe maximum page size of 100 and support search/status/brand/SKU/product-type filters. Administrative mutations require the existing Administrator policy. Controllers dispatch through MediatR and return application models rather than EF entities.

OpenAPI and Scalar continue using the host configuration. The catalog controller is in the host MVC application and therefore participates in the existing API discovery pipeline.

## Administration UI and theming

The administration UI is layered as:

```text
Application use case
    ↓
Page/View model
    ↓
UI components
    ↓
Theme / presentation contracts
    ↓
MudBlazor implementation
```

`IThemeProvider` and `ThemeDefinition` define theme identity, version, author, layouts, assets, stylesheets, scripts, component override metadata, and other presentation metadata. The Application/Domain catalog model has no reference to MudBlazor or the theme implementation. Stage 08 does not implement remote theme downloads or arbitrary code execution.

MudBlazor remains a component library used by the host UI. It is not the RemoteCommerce theme contract, allowing a future theme implementation to replace layouts and presentation assets without changing catalog use cases.

## Dynamic menu system

Administration navigation is composed through `IMenuProvider`, `IMenuContributor`, and `MenuItemDefinition`. Core navigation registers the catalog tree and existing administration destinations as contributions instead of making the catalog page itself the navigation contract.

The stable plugin SDK exposes `IRemoteCommercePluginMenuContributor` and `PluginMenuItem`. A plugin can register its contributor during normal startup without referencing host Razor components. Runtime plugin loading only activates enabled/valid plugins, so disabled, uninstalled, or failed plugins do not leave an active menu contribution in the service collection.

Menu filtering is presentation visibility only. Every sensitive route and API mutation continues to use ASP.NET Core authorization policies. A hidden menu item is never considered authorization.

## Localization

Catalog UI labels use the existing `ILocalizer` boundary and a `CatalogResources` resource marker. Initial supported cultures remain `en-US` and `pt-BR`. No catalog page treats Portuguese text as an authorization or domain contract.

## Plugin extension points

Stage 08 establishes narrow plugin extension points rather than arbitrary component injection:

- administration menu contributions through the stable SDK;
- catalog metadata through explicit `ProductMetadata` records;
- future product UI extensions can be added as typed application/presentation contracts without coupling Domain to Razor or MudBlazor.

The plugin runtime, manifest, package validator, generator, persistence contract, and restart lifecycle remain unchanged.

## Soft-delete and operation history

Catalog entities implement the same `ISoftDeletable` contract used by the host. Normal EF queries exclude disabled records. Deletions flow through the existing persistence preparation path and are represented as soft-delete state changes. Operation history remains infrastructure-owned and uses the existing redaction rules; catalog code does not create a second audit system.

## Provider and media boundaries

Catalog persistence uses the Stage 06/07 `IDatabaseProvider` strategy and the host `CommerceDbContext`. Media references do not know whether content is stored in the filesystem or MongoDB/GridFS. No catalog code creates `SqlConnection`, `SqlCommand`, or MongoDB driver objects.

## Plugin persistence

Plugin persistence remains separate from host catalog persistence. Plugin-owned contexts, migrations, schema names, transactions, soft-delete, and operation history continue to follow the Stage 07 contract. The host catalog does not expose its EF context to plugins.
