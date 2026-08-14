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
- Domain, Application, and Infrastructure are explicit architectural boundaries even while they remain in the current host project.
- The physical layout must remain ready for future class library extraction with root namespace `RemoteCommerce`.
- Domain must not depend on Application or Infrastructure.
- Application must not depend directly on Infrastructure implementations.
- Infrastructure owns repository implementations, DbContexts, storage providers, and provider-specific persistence.

## Global Application feature structure

Every Application feature must use this canonical structure when the concern exists:

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

In the current host project, the equivalent is `src/RemoteCommerce/Application/Feature/...`.

Domain features belong under `src/RemoteCommerce/Domain/<Feature>`.

Infrastructure features belong under `src/RemoteCommerce/Infrastructure/<Feature>`.

Do not place feature-specific application artifacts in global folders.

## Global data flow

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

Controllers receive Requests and dispatch MediatR Commands or Queries.

Handlers execute use cases after configured Behaviors.

Feature Services coordinate Application and Infrastructure through abstractions.

Repository contracts are database-agnostic and storage-provider-agnostic.

Repository implementations are Infrastructure-only.

## Global source formatting

- One C# instruction or method call per source line.
- One logical statement per source line.
- One Razor directive per line.
- One HTML or Razor component invocation per line when it has attributes or child content.
- Keep executable Razor expressions and event callbacks independently readable.
- Apply these rules to production code, tests, generated templates, and Razor UI.

## Stage 07 — Plugin Persistence Compatibility

**Status: integrated in main.**

Stage 07 established provider-independent plugin persistence, restart-based activation, plugin-owned EF contexts/migrations, reusable soft-delete/history, and the three persistence-capable reference plugins RemoteSEO, RemoteAdSense, and RemoteVisitors. Plugin APIs remain under `/api/rp/v1`.

## Stage 08 — Product Catalog

**Status: implementation in Draft PR #11.**

Stage 08 introduces the first host-owned commercial domain while preserving the provider and plugin boundaries established previously.

### Catalog domain

The catalog owns `Product`, `ProductVariant`, `Category`, `Brand`, `Tag`, `ProductAttribute`, `ProductAttributeValue`, metadata, product media references, and relationship entities. Product types are `Simple`, `Variable`, `Virtual`, and `Downloadable`; lifecycle states include `Draft`, `Published`, and `Archived`.

### Application and persistence

Catalog mutations and queries use MediatR 12.5.0 and FluentValidation. Queries are projected into application Results and product collections are bounded to a maximum page size of 100. The catalog uses the existing `CommerceDbContext`, database provider strategy, soft-delete contract, and operation-history boundary; no catalog-specific provider is introduced.

### REST API

RemoteCommerce-owned catalog resources use `/api/rc/v1`. Product listing supports pagination and the implemented product filters. Administrative mutations require the existing Administrator authorization policy. Plugin APIs continue using `/api/rp/v1`.

### Administration UI

Catalog administration is available under `/admin/catalog/products`, with taxonomy pages for categories, brands, tags, and attributes. Product editing is application-command based and does not access EF Core from Razor components.

### Theme and menu contracts

The administration surface has a presentation theme boundary and a dynamic menu composition boundary. MudBlazor remains an internal component library, not the theme contract. The stable plugin SDK can contribute menu items without changing host navigation code.

### Architectural exit condition

Stage 08 is not considered architecturally complete until the implementation follows the global Domain/Application/Infrastructure boundaries, canonical Application feature layout, canonical data flow, and global source formatting rules.

### Validation exit condition

The Stage 08 PR must pass repository build, test, pack, and CI validation before being marked ready. It remains draft until manually validated by the repository owner.

## Stage 09 — Customers, Cart, and Checkout

Reserved.

## Stage 10 — Orders and Payments

Reserved.

## Stage 11 — Shipping, Taxes, and Store Operations

Reserved.

## Stage 12 — WooCommerce-Compatible REST API

Reserved for broader WooCommerce-compatible resources. `/api/rc/v1` is already established by Stage 08 for RemoteCommerce-owned catalog resources.

## Stage 13 — Storefront and Theme/Extension Model

Reserved for storefront rendering. Stage 08 only establishes the reusable presentation/theme contracts needed by the administration UI.

## Stage 14 — Multi-Store Federation

Reserved.

## Stage 15 — Runtime Plugin Hot Reload

Reserved. Plugin activation remains restart-based.

## Stage 16 — Production Readiness

Reserved.
