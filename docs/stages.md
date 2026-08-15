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

## Global architectural rules

- .NET 10 and repository-approved package versions are mandatory.
- Public APIs require complete en-US XML documentation.
- C# imports live in GlobalUsings.cs; Razor imports live in _Imports.razor.
- MediatR 12.5.0 is mandatory for application commands, queries, notifications, and behaviors.
- FluentValidation runs through the validation behavior.
- Mutating persistence uses the transactional behavior.
- Mutable persistence uses shared soft-delete and operation-history mechanisms.
- Exactly one active Stage branch and one open PR are allowed; PRs are not merged unless explicitly requested.
- Domain, Application, Infrastructure, Presentation, and Plugin Runtime remain logical boundaries inside the current host project.

## Future shared class library

The former plan to extract Domain, Application, and Infrastructure into three class libraries is retired.

The only future shared assembly is `RemoteCommerce.Abstractions`, located at `src/RemoteCommerce.Abstractions`, with `RootNamespace=RemoteCommerce`.

It contains only non-concrete contracts and models and preserves the logical namespaces already used by the host. It must not contain concrete persistence, storage, ASP.NET Core, Blazor, MudBlazor, or plugin runtime implementations.

Concrete Domain, Application, and Infrastructure implementations remain in the host unless a future explicit refactoring changes that decision.

## Global Application feature structure

Every Application feature uses:

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

Current host equivalent: `src/RemoteCommerce/Application/Feature/...`.

## Global data flow

`Controllers(Requests) -> MediatR Commands/Queries -> Behaviors -> Feature Services -> Repository<T> -> DbContext|StorageProvider`.

Controllers never receive Commands/Queries from transport binding. Commands/Queries receive their corresponding Request instance and map Request values into use-case data. Handlers return `Result` or `Result<T>`.

Applicable flow layers log exceptions and rethrow. The global exception handler owns HTTP translation to Problem Details and status codes.

## Global source formatting

- One C# instruction or method call per source line.
- One logical statement per line.
- One Razor directive per line.
- One HTML/Razor component invocation per line when attributes or child content exist.
- Keep executable Razor expressions and callbacks independently readable.

## Stage 07 — Plugin Persistence Compatibility

**Status: integrated in main.**

Stage 07 established provider-independent plugin persistence, restart-based activation, plugin-owned EF contexts/migrations, reusable soft-delete/history, and persistence-capable reference plugins. Plugin APIs remain under `/api/rp/v1`.

## Stage 08 — Product Catalog

**Status: implementation in Draft PR #11.**

Stage 08 introduces Product, ProductVariant, Category, Brand, Tag, ProductAttribute, ProductAttributeValue, ProductMetadata, product media references, catalog REST API, administration UI, dynamic menus, and theme contracts.

Catalog persistence uses the existing provider strategy and CommerceDbContext. Media remains provider-independent through IMediaStorageProvider. Catalog uses shared soft-delete and operation history.

RemoteCommerce catalog endpoints use `/api/rc/v1`. Plugin APIs remain `/api/rp/v1`.

The administration UI uses theme/presentation abstractions and dynamic menu contributions. MudBlazor is an internal component library, not the theming contract.

Stage 08 is architecturally complete only when the canonical feature layout, request/command/query/result flow, provider-independent repository boundary, shared abstractions direction, formatting rules, and plugin compatibility are satisfied.

Validation requires build, test, pack, and green CI. The PR remains draft until repository-owner validation.

## Stage 09 — Customers, Cart, and Checkout

Reserved.

## Stage 10 — Orders and Payments

Reserved.

## Stage 11 — Shipping, Taxes, and Store Operations

Reserved.

## Stage 12 — WooCommerce-Compatible REST API

Reserved for broader WooCommerce-compatible resources. `/api/rc/v1` is already established by Stage 08 for RemoteCommerce-owned catalog resources.

## Stage 13 — Storefront and Theme/Extension Model

Reserved for storefront rendering and broader theme/extension capabilities. Stage 08 only establishes reusable presentation contracts needed by the administration UI.

## Stage 14 — Multi-Store Federation

Reserved.

## Stage 15 — Runtime Plugin Hot Reload

Reserved. Plugin activation remains restart-based until this stage.

## Stage 16 — Production Readiness

Reserved.
