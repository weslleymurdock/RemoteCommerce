# Implementation Stages

Stages are sequential. Only one stage may be active/open as a PR at a time. A new stage starts from the latest `main` after the previous stage is successfully integrated.

## Global implementation rules

- Public APIs must use complete XML documentation in en-US, including all applicable elements such as `<summary>`, `<param>`, `<returns>`, `<typeparam>`, `<exception>`, and `<remarks>`.
- All projects must use implicit namespaces through the project file (`<ImplicitUsings>enable</ImplicitUsings>` and explicit `<Using Include="..." />` items where required). Source files must not contain `using` directives. New code must follow this rule, and touched code must be migrated when practical.
- MediatR is mandatory for application commands, queries, notifications, and pipeline behaviors, fixed at version `12.5.0`.
- Controllers must dispatch application work through MediatR rather than embedding business/application orchestration directly in controller actions.
- FluentValidation is mandatory for command/query validation where validation is applicable, using `FluentValidation` and `FluentValidation.DependencyInjectionExtensions` with the repository-approved version.
- Required MediatR behaviors include logging and validation behaviors. Commands that execute transactional SQL/application persistence operations must use a transactional behavior with rollback on failure. Queries must not be wrapped in write transactions unless explicitly required by their consistency semantics.
- SQL persistence strategies must implement soft-delete for mutable persisted records. Hard deletion is exceptional and must be explicitly justified by the persistence contract.
- SQL persistence must provide an operation-history strategy for relevant mutations. Before an update/delete operation, the previous state must be captured as serialized metadata in a single history value/cell together with operation timestamp, user/actor, operation type, entity identity, and other relevant context available from the request/application context. The replacement/new state should likewise be serialized when applicable. History records must remain queryable independently from the current row state.
- Persistence history must not depend on controller-specific code; it belongs in application/persistence infrastructure and must work for MediatR-driven commands.
- New persistence code must preserve audit/history and soft-delete semantics consistently across supported SQL provider strategies.
- Exactly one PR may be open at a time. Do not create parallel PRs or parallel Stage branches.
- Do not merge PRs unless explicitly requested by the user. After a successful PR integration and passing required jobs, delete the Stage branch.

## Stage 01 — Foundation

Implemented foundation for the RemoteCommerce host and its plugin architecture.

- .NET 10 solution structure.
- Main server/UI application with Blazor and ASP.NET Core controllers.
- EF Core/SQL Server persistence foundation.
- MudBlazor UI foundation.
- Plugin abstractions and manifest model.
- Initial plugin administration/runtime concepts.
- XML documentation policy for public APIs.
- OpenAPI/Scalar setup was established during the early foundation work.
- Implicit namespace policy is part of the repository conventions.

**Exit condition:** host solution builds and provides the foundation required by the plugin runtime.

## Stage 02 — Plugin Runtime

Implemented the runtime plugin lifecycle on top of the foundation.

- Plugin installation/persistence flow.
- Plugin package/manifest discovery.
- Plugin loading after host restart.
- Dependency injection registration for plugin services/controllers.
- Plugin assembly registry.
- Blazor routing support for plugin Razor assemblies.
- Plugin administration/information display.
- Plugin README and LICENSE metadata support.
- Standard plugin health endpoint pattern.
- Runtime plugin API namespace `/api/rp/v1/...`.
- WooCommerce-compatible API namespace reserved as `/api/rc/v1/...`.

**Exit condition:** an installed plugin can be discovered, loaded on restart, participate in DI, expose controllers, and expose Razor pages.

## Stage 03 — NuGet Plugin Packaging and Template Tooling

Implemented distributable plugin packaging and generation tooling.

- Plugins are packaged as `.nupkg` artifacts.
- Plugin manifest includes package metadata and required README/LICENSE references.
- Plugin generator is a `dotnet tool` under `tools/`.
- Templates are stored as text resources (`.cs.txt`, `.razor.txt`, `.csproj.txt`) so placeholders never enter the template tool compilation.
- JSON and Markdown resources remain native documents.
- Generated plugins contain plugin information UI and health controller by default.
- Generated controller APIs follow `/api/rp/v1/<plugin_controller>`.
- Generated plugins reference `RemoteCommerce.Plugin.Abstractions` by project reference during repository development.
- Generated plugins use MudBlazor 9.8.0 and isolate package-version management where required by repository package props.
- Plugin entry point can consume host `IConfiguration`.
- Plugin Razor assemblies are registered through Blazor routing `AdditionalAssemblies`, while controllers are registered as MVC application parts.
- Plugin projects can be built independently from the main solution.
- Generated source projects must follow the repository-wide implicit-namespace/no-using-directive rule.

**Exit condition:** a generated plugin can be built, packed, installed, and loaded by the host after restart.

## Stage 04 — Host Installation and Administration

Implemented and validated in the single Stage 04 PR.

- MudBlazor plugin administration list and details.
- `.nupkg` upload, package validation, install, update, enable, disable, uninstall, and retained-version rollback.
- Explicit desired state plus persisted lifecycle states including `ActivationPending`, `Disabled`, `Loaded`, and `Failed`.
- Manifest validation for identity, version, entry point, package paths, required README/LICENSE declarations, dependencies, and optional EF compatibility metadata.
- Package structure validation for `plugin.manifest.json`, `README.md`, `LICENSE.md`, target framework layout, and entry assembly metadata.
- SHA-256 package integrity recording.
- Host and EF Core compatibility validation without executing plugin code during package validation.
- Dependency version validation, disabled dependency protection, duplicate dependency detection, and circular dependency detection.
- SQL Server persistence for installation state, retained versions, dependencies, lifecycle errors, and plugin settings.
- Trusted local package source abstraction through `PluginAdministration:TrustedPackageDirectory`.
- Restart-required orchestration that persists desired lifecycle changes without attempting to mutate the root DI provider after host construction.
- Startup activation diagnostics persisted to the database.
- README.md and LICENSE.md remain package files and are read from the installed artifact rather than duplicated into database columns.
- Host administration API at `/api/v1/plugins` with OpenAPI/Scalar metadata.
- Automated tests covering package validation, missing required files, entry point/framework/host compatibility, integrity, dependency rules, lifecycle state transitions, uninstall dependency protection, and rollback.
- CI builds the main solution, runs tests, builds tooling/plugin solutions, and packs a generated sample plugin and template tool successfully.

Intentionally deferred from this stage:

- Package signing/trusted signature verification; the current boundary records and validates SHA-256 integrity and leaves signing/source trust extensible.
- A generic settings-schema/form generator UI; Stage 04 provides the persistence/service boundary for plugin settings.
- Plugin navigation/menu contributions; this remains part of Stage 13 storefront/admin extension work.
- Runtime hot reload; lifecycle changes continue to require host restart.

**Exit condition:** an administrator can install and manage plugins from the RemoteCommerce UI without manually editing application files, with build/test/package validation passing in CI.

## Stage 05 — Site, Identity, Configuration, Secrets, and Localization Foundation

Goal: establish the application-level foundation required by every subsequent store capability, while introducing the repository-wide application pipeline and persistence rules that future commerce features must follow.

### Site and application configuration

- Site identity, name, public URL/base URL, locale, timezone, culture, and general settings.
- Persistent application/store settings with a clear distinction between deployment configuration and editable application configuration.
- Typed configuration boundaries for settings consumed by application services.
- Configuration validation and safe defaults.
- A configuration model that can later host provider settings, media settings, payment settings, shipping settings, and federation settings without coupling those features into the current stage.

### Application request pipeline

- Introduce MediatR `12.5.0` as the application command/query/notification mediator.
- Controllers must remain thin and dispatch commands/queries through MediatR.
- Implement command/query handlers as the application boundary for use cases.
- Introduce MediatR pipeline behaviors for structured logging and FluentValidation.
- Use `FluentValidation` and `FluentValidation.DependencyInjectionExtensions` for validators and automatic validator registration.
- Validation behavior must prevent invalid commands/queries from reaching handlers and must expose predictable validation failures to the API/UI layers.
- Logging behavior must capture relevant request/handler context without logging secrets or sensitive credentials.
- Introduce transactional behavior for commands that perform SQL/persistence mutations. The behavior must commit on successful completion and rollback when the handler or persistence operation fails.
- Transactional behavior must not blindly wrap read-only queries in write transactions.
- Transaction boundaries must be compatible with EF Core execution semantics and the repository's persistence abstractions.
- MediatR notifications may be used for side effects that do not belong in the core command transaction, with explicit consistency semantics.

### Identity and authorization

- Users, authentication/session foundation, roles, claims, and permissions.
- Administrative authorization policies.
- Seed/bootstrap path for the initial administrator.
- Explicit separation between authentication, authorization, and application/site settings.
- Identity mutations must use commands and validation through the MediatR pipeline.

### Administration foundation

- Admin dashboard foundation.
- Consistent admin navigation and settings organization.
- Configuration status/validation feedback.
- Audit logging foundation for administrative configuration/security changes.
- Administrative mutation operations must participate in the common command, validation, logging, transaction, soft-delete, and history policies where applicable.

### Secrets boundary

- Introduce an application-level `ISecretProvider` abstraction (or an equivalent existing abstraction if the repository already provides one).
- Initial implementation must integrate with ASP.NET Core configuration/environment mechanisms rather than inventing a proprietary secret store.
- Do not persist plaintext secrets in the application database.
- Keep the abstraction compatible with future Docker secrets, Swarm/Kubernetes secrets, Azure Key Vault, environment variables, or other external providers.
- Clearly distinguish secrets from normal editable application settings.
- Secret access must not be logged by MediatR logging behavior.

### Localization

- Introduce the RemoteCommerce `ILocalizer` abstraction/wrapper over the ASP.NET Core `IStringLocalizer<T>` infrastructure.
- Resource resolution must support resource-type-aware localization.
- Initial cultures: `en-US` and `pt-BR`.
- Define culture fallback behavior.
- Support resource XML files as an administrable resource format.
- Resource XML upload/import through the UI.
- Validate resource structure, culture, keys, duplicates, and malformed files before activation.
- Version/track imported resources and provide safe replacement semantics.
- Do not require code changes to add or replace localized resource content.
- Resource mutation commands must use FluentValidation and transactional behavior where database state is changed.

### SQL persistence, soft-delete, and history

- Every mutable SQL persistence strategy introduced by this stage or later must support soft-delete.
- Soft-deleted records must not appear in normal application queries unless an explicit administrative/history query requests them.
- Deletion commands must translate to soft-delete by default.
- Hard-delete operations are exceptional and require an explicit persistence contract and justification.
- Persisted mutations must have an operation-history strategy independent from controller/UI code.
- Before an update or delete, capture the previous state as serialized metadata in a single history value/cell, together with the operation timestamp, actor/user identity when available, operation type, entity identity, correlation/request information when available, and other relevant context.
- When applicable, capture the replacement/new state in serialized form as part of the history record.
- History records must remain queryable independently of the current entity row.
- The history implementation must work with MediatR command handlers and transactional behavior so the mutation and its history are committed or rolled back together.
- Avoid relying on controller-specific code to populate audit metadata; obtain context through application/infrastructure abstractions.
- Sensitive values must be excluded or protected according to the repository's security policy.

### Architectural constraints

- Do not split the application into Domain/Application/Infrastructure projects merely for organizational purposes in this stage.
- Keep boundaries explicit through contracts and services so a later DDD/modular refactoring can be performed when actual bounded contexts emerge.
- Do not introduce `TenantId` into all entities.
- Do not implement multi-store federation in this stage.
- Do not implement the database strategy/provider pattern in this stage; it remains Stage 06.
- Do not implement MongoDB/GridFS in this stage; it remains Stage 06.
- Do not make Docker/Swarm/Kubernetes the application's deployment abstraction in this stage. The secret abstraction must merely remain compatible with those environments.

**Exit condition:** a fresh installation can be configured and administered through the UI, an initial administrator can authenticate and use authorized administration features, commands/queries flow through MediatR `12.5.0` with FluentValidation and logging behaviors, mutating commands use transactional rollback semantics, SQL persistence uses soft-delete and atomic operation history, secrets are consumed through an abstraction without plaintext persistence, and application UI/services resolve localized strings through `ILocalizer` for `en-US` and `pt-BR` with validated resource XML imports.

## Stage 06 — Database Provider Strategy and Media Storage

Goal: establish provider boundaries before commerce features create hard dependencies on a single persistence technology.

- Database provider strategy selected from `IConfiguration` with a documented default.
- Provider registration and startup validation.
- SQL Server as the initial/default relational provider.
- Provider-aware EF Core migrations and schema management.
- Application contracts that prevent provider-specific APIs from leaking into domain services.
- Every SQL provider strategy must implement the mandatory soft-delete contract.
- Every SQL provider strategy must implement the mandatory serialized operation-history strategy, preserving before/after metadata and operation context atomically with mutations.
- Database provider implementations must integrate with the MediatR transactional behavior rather than bypassing application transaction boundaries.
- Document/blob storage abstraction for media and other large assets.
- MongoDB/GridFS provider for media and optional virtual-product payloads, consumed through application services rather than MongoDB-specific domain types.

**Exit condition:** the host can select a supported database provider through configuration, relational persistence remains isolated behind contracts, every supported SQL provider satisfies soft-delete/history/transaction rules, and media can be stored through a provider abstraction with SQL Server and MongoDB/GridFS support where applicable.

## Stage 07 — Plugin Persistence Compatibility

Goal: make generated and installed plugins safe to build against the host persistence stack and common application pipeline.

- Pin the EF Core version used by plugin templates to the backend-compatible version.
- Add `Microsoft.EntityFrameworkCore` and related required abstractions/packages explicitly to generated plugins where required.
- Generate a placeholder plugin `DbContext` and registration boundary without imposing a database schema on every plugin.
- Define plugin migration ownership and database naming/isolation rules.
- Validate plugin EF versions against host compatibility metadata.
- Permit plugins to use other third-party SQL/EF libraries when their feature requires them, while preventing incompatible runtime dependency graphs.
- Plugin commands/queries must use the host MediatR `12.5.0` contract when participating in host application workflows.
- Plugin validators should use the host FluentValidation pipeline where the plugin exposes command/query validation.
- Plugin SQL persistence must obey the mandatory soft-delete and operation-history strategy or explicitly use a host-provided persistence service that guarantees those semantics.
- Plugin mutations must participate in transaction boundaries and rollback semantics.

**Exit condition:** a generated plugin with persistence compiles independently and can opt into a supported plugin `DbContext` without taking ownership of the host's domain DbContext, while plugin application operations remain compatible with the host command/query, validation, transaction, soft-delete, and history rules.

## Stage 08 — Product Catalog

Goal: reach the WooCommerce catalog baseline.

- Products and product lifecycle.
- Simple and variable products.
- Categories, tags, attributes, and variations.
- SKU, pricing, sale pricing, tax class, stock status, and inventory quantities.
- Product media/gallery using the configured storage provider.
- Catalog search/filtering and admin CRUD.
- Catalog commands and queries through MediatR.
- FluentValidation for catalog mutations.
- Transactional behavior for catalog commands.
- Soft-delete for mutable catalog records.
- Operation history for catalog mutations.

**Exit condition:** administrators can create and manage a usable store catalog with validated, transactional, soft-deletable, and historically traceable mutations.

## Stage 09 — Customers, Cart, and Checkout

Goal: implement the core commerce transaction flow.

- Customers and customer addresses.
- Anonymous and authenticated carts.
- Cart items and pricing snapshots.
- Checkout sessions.
- Billing/shipping addresses.
- Tax calculation boundary.
- Shipping method boundary.
- Coupon/discount boundary.
- Commands/queries through MediatR with validation and transactional behavior where applicable.
- Soft-delete and operation history for mutable SQL records.

**Exit condition:** a customer can progress from catalog to a valid checkout/order request with consistent transaction and audit semantics.

## Stage 10 — Orders and Payments

Goal: implement WooCommerce-equivalent order management.

- Orders, order items, totals, taxes, discounts, shipping, and fees.
- Order status state machine.
- Payment intent abstraction.
- Payment provider plugin contracts.
- Refunds and payment reconciliation.
- Customer order history and admin order management.
- MediatR commands/queries/notifications and required behaviors.
- Transactional order mutations with rollback.
- Soft-delete where semantically applicable.
- Immutable/order-history requirements must complement, not weaken, the mandatory persistence operation history.

**Exit condition:** a complete order can be created, paid through a provider, fulfilled, refunded, and audited.

## Stage 11 — Shipping, Taxes, and Store Operations

Goal: complete the operational commerce layer.

- Shipping zones, methods, rates, and fulfillment states.
- Tax zones/classes/rates.
- Inventory reservation and stock adjustments.
- Order fulfillment/shipping tracking.
- Transactional background processing.
- MediatR commands/queries/notifications for application workflows.
- Validation, transaction, soft-delete, and operation-history rules for applicable SQL mutations.

**Exit condition:** the system supports real store operations rather than only checkout simulation.

## Stage 12 — WooCommerce-Compatible REST API

Goal: provide the API surface required for WooCommerce-compatible integrations.

- `/api/rc/v1` resource model.
- Products, variations, categories, customers, orders, coupons, and reports as appropriate.
- Authentication and authorization.
- Pagination, filtering, sorting, error contracts, and versioning.
- OpenAPI/Scalar documentation.
- Controllers remain thin and dispatch application commands/queries through MediatR.
- API validation uses the common FluentValidation pipeline.

**Exit condition:** supported WooCommerce integrations can communicate with RemoteCommerce through the documented compatibility API.

## Stage 13 — Storefront and Theme/Extension Model

Goal: provide a complete customer-facing store experience.

- Storefront navigation.
- Product listing/detail pages.
- Cart and checkout pages.
- Customer account pages.
- Theme/layout extension points.
- Plugin page/menu/widget contributions.
- Application workflows use the common command/query, validation, transaction, soft-delete, and history policies where applicable.

**Exit condition:** a fresh RemoteCommerce installation can present a functional store without custom development.

## Stage 14 — Multi-Store Federation

Goal: allow independently deployed RemoteCommerce stores to operate as one logical multi-store organization without sacrificing database isolation.

- Organization/store identity and federation contracts.
- Federation plugin distributed independently from the host.
- Explicit API/event/command synchronization between stores.
- Shared catalog policies.
- Shared inventory reservations, transfers, and stock synchronization.
- Shared configuration with clear ownership and override rules.
- Idempotency, conflict detection, retries, ordering, and eventual-consistency semantics.
- Secure store-to-store authentication and authorization.
- Optional control-plane metadata that does not contain transactional store data.
- Docker Compose/Swarm deployment guidance for multiple independent store stacks.
- Federation mutations must preserve local soft-delete/history/transaction semantics; synchronization must never bypass local persistence policies.

**Exit condition:** two or more stores with exclusive databases can be operated independently while exposing a controlled logical multi-store experience for supported shared capabilities.

## Stage 15 — Runtime Plugin Hot Reload

Goal: investigate and, where technically safe, enable plugin installation/activation/deactivation/update without restarting the host process.

- Collectible `AssemblyLoadContext` for plugin assemblies.
- Plugin service registry and isolated service scopes.
- Dynamic controller/application-part refresh.
- Dynamic Blazor additional-assembly routing strategy.
- Background service lifecycle cancellation.
- Endpoint and cache invalidation.
- Safe unload verification and assembly leak detection.
- Atomic activation/update and rollback.
- Explicit capability matrix for plugins that cannot be hot reloaded.
- Hot reload must respect active MediatR handlers, notification subscriptions, pipeline behaviors, EF contexts, transactions, and persistence history services.

The restart-based model remains the safe fallback. Hot reload must not compromise DI lifetime correctness, routing, memory safety, security, or in-flight requests.

**Exit condition:** compatible plugins can be installed, enabled, disabled, updated, and unloaded without process restart; unsupported plugins clearly require restart.

## Stage 16 — Production Readiness

Goal: make the platform suitable for production deployment.

- Database migrations and upgrade strategy.
- Secrets/configuration guidance.
- Structured logging, metrics, health checks, and tracing.
- Caching and performance baselines.
- Concurrency/idempotency safeguards.
- Security hardening.
- Backup/restore guidance.
- Automated integration/end-to-end tests.
- Upgrade compatibility and plugin API compatibility policy.
- Disaster recovery and multi-store federation operational guidance.
- Production validation of soft-delete retention, operation-history growth, archival, and privacy/redaction policies.
- Production validation of MediatR behavior ordering and transactional rollback guarantees.

**Final exit condition:** a fresh RemoteCommerce installation can be configured and operated as a functional WordPress + WooCommerce-equivalent application, with plugins installed and managed through the application UI and supported commerce workflows available end to end.
