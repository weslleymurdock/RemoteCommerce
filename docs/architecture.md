# Architecture

RemoteCommerce is a .NET 10 ASP.NET Core application with Interactive Server Blazor, EF Core persistence, MediatR application workflows, MudBlazor administration, and a restart-based runtime plugin model.

## Architectural boundaries

The current repository is a single host project, but Domain, Application, and Infrastructure are explicit architectural boundaries.

```text
RemoteCommerce
├── Components/                 Blazor UI and administration adapters
├── Application/                feature use cases and contracts
├── Domain/                     business model and domain rules
├── Infrastructure/             persistence, repositories, storage, integrations
├── Plugins/                    runtime loading and lifecycle orchestration
└── Plugin.Abstractions/        stable SDK contract consumed by plugins
```

The layout is intentionally migration-ready. Domain, Application, and Infrastructure may later become independent class library projects while retaining root namespace `RemoteCommerce` and the same dependency direction.

Domain has no dependency on Application or Infrastructure.

Application depends on Domain and abstractions, but not on Infrastructure implementations.

Infrastructure owns provider-specific implementations, EF Core, DbContexts, repositories, storage providers, and external integrations.

Presentation and Blazor UI are adapters over Application and must not access EF Core or storage providers directly.

## Application feature layout

Every Application feature is organized by feature and concern using this canonical structure when the corresponding concern exists:

```text
src/Application/Feature/
├── Abstractions/
├── Commands/
├── Handlers/
├── Queries/
├── Requests/
├── Resources/
├── Results/
└── Validators/
```

In the current host project the equivalent physical path is `src/RemoteCommerce/Application/Feature/...`.

Domain features belong under `src/RemoteCommerce/Domain/<Feature>`.

Infrastructure features belong under `src/RemoteCommerce/Infrastructure/<Feature>`.

Feature-specific application artifacts must not be placed in a global commands, queries, validators, or contracts folder.

## Application data flow

The canonical data flow for all new features is:

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

Controllers receive transport Requests and dispatch MediatR Commands or Queries.

MediatR Handlers execute use cases after configured Behaviors such as logging, validation, and transaction handling.

Feature Services coordinate Application and Infrastructure through explicit abstractions.

Repository contracts are database-agnostic and storage-provider-agnostic.

Repository implementations belong to Infrastructure and may use `DbContext` or a storage provider internally.

Provider-specific types never cross the Infrastructure boundary into Domain or Application contracts.

The host owns `CommerceDbContext`, `IDatabaseProvider`, `DatabaseTopology`, `ISecretProvider`, `TransactionalBehavior`, and operation history.

Plugins never receive `CommerceDbContext`, arbitrary connection strings, provider-specific database objects, or host implementation details.

## Application feature extraction rule

The future class library extraction must preserve the following conceptual projects and root namespace:

```text
RemoteCommerce.Domain
RemoteCommerce.Application
RemoteCommerce.Infrastructure
```

The project extraction itself is not implied by this rule. Until explicitly requested, the repository remains a single host project.

## Product Catalog

Stage 08 adds a host-owned catalog model without copying WooCommerce's internal PHP storage model. The core aggregate is `Product`; related entities represent variants, taxonomy, attributes, metadata, and media references.

`Product` supports name, URL slug, optional SKU, descriptions, lifecycle status, product type, prices, currency, optional brand, categories, tags, attributes, variants, metadata, and media references. Product types are `Simple`, `Variable`, `Virtual`, and `Downloadable`. Statuses are `Draft`, `Published`, and `Archived`.

`Category` is hierarchical through `ParentId` and uses restrictive parent deletion to prevent accidental tree corruption. `Brand` and `Tag` have unique slugs. Product and variant SKUs are database-unique. `ProductAttribute` and `ProductAttributeValue` provide extensible values such as Color, Size, and Material without hardcoding attributes into `Product`.

Product metadata is represented by explicit `ProductMetadata` records containing a validated key, scalar/JSON type, and value. Secrets are not a catalog metadata capability. Product media contains only a `MediaId`, role, order, and alternative text; binary content remains owned by `IMediaStorageProvider`.

The catalog uses the existing host `CommerceDbContext` and provider strategy. It does not introduce a second context or provider-specific database API. Catalog entities participate in the shared soft-delete contract and EF query filtering. The Stage 08 migration creates catalog tables under the existing `commerce` schema.

## Catalog API

RemoteCommerce-owned resources use `/api/rc/v1`. Plugin resources remain under `/api/rp/v1`.

Stage 08 provides product collection/detail and administrative mutation endpoints plus category, brand, tag, and attribute resources. Product collections are paged with a safe maximum page size of 100 and support search/status/brand/SKU/product-type filters. Administrative mutations require the existing Administrator policy. Controllers dispatch through MediatR and return application Results rather than EF entities.

OpenAPI and Scalar continue using the host configuration. The catalog controller is in the host MVC application and therefore participates in the existing API discovery pipeline.

## Administration UI and theming

The administration UI is layered as:

```text
Application use case
    ↓
Request / Result or Page ViewModel
    ↓
UI Components
    ↓
Theme / presentation contracts
    ↓
Component library implementation
```

`IThemeProvider` and `ThemeDefinition` define theme identity, version, author, layouts, assets, stylesheets, scripts, component override metadata, and other presentation metadata. The Application/Domain catalog model has no reference to MudBlazor or the theme implementation.

MudBlazor remains a component library used by the host UI. It is not the RemoteCommerce theme contract.

## Dynamic menu system

Administration navigation is composed through `IMenuProvider`, `IMenuContributor`, and `MenuItemDefinition`. Core navigation registers the catalog tree and existing administration destinations as contributions instead of making a page implementation the navigation contract.

The stable plugin SDK exposes `IRemoteCommercePluginMenuContributor` and `PluginMenuItem`. A plugin can register its contributor without referencing host Razor components.

Menu filtering is presentation visibility only. Every sensitive route and API mutation continues to use ASP.NET Core authorization policies. A hidden menu item is never considered authorization.

## Source formatting

C# instructions and method calls must be vertically readable with one logical statement per line.

Razor directives must be one per line.

HTML and Razor component invocations with attributes or child content must remain independently readable and must not be compressed onto shared lines.

These rules apply to production code, tests, generated templates, and Razor UI.

## Localization

Catalog UI labels use the existing `ILocalizer` boundary and a `CatalogResources` resource marker. Initial supported cultures remain `en-US` and `pt-BR`.

## Plugin extension points

Stage 08 establishes narrow plugin extension points rather than arbitrary component injection:

- administration menu contributions through the stable SDK;
- catalog metadata through explicit `ProductMetadata` records;
- future product UI extensions through typed Application/Presentation contracts without coupling Domain to Razor or MudBlazor.

The plugin runtime, manifest, package validator, generator, persistence contract, and restart lifecycle remain unchanged.

## Soft-delete and operation history

Catalog entities implement the same `ISoftDeletable` contract used by the host. Normal EF queries exclude disabled records. Deletions flow through the existing persistence preparation path and are represented as soft-delete state changes. Operation history remains infrastructure-owned and uses the existing redaction rules; catalog code does not create a second audit system.

## Provider and media boundaries

Catalog persistence uses the Stage 06/07 `IDatabaseProvider` strategy and the host `CommerceDbContext`. Media references do not know whether content is stored in the filesystem or MongoDB/GridFS. No catalog code creates `SqlConnection`, `SqlCommand`, or MongoDB driver objects.

## Plugin persistence

Plugin persistence remains separate from host catalog persistence. Plugin-owned contexts, migrations, schema names, transactions, soft-delete, and operation history continue to follow the Stage 07 contract. The host catalog does not expose its EF context to plugins.
