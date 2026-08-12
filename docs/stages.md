# Implementation Stages

Stages are sequential. Only one stage may be active/open as a PR at a time. A new stage starts from the latest `main` after the previous stage is successfully integrated.

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

**Exit condition:** a generated plugin can be built, packed, installed, and loaded by the host after restart.

## Stage 04 — Host Installation and Administration

Goal: make plugin management operationally equivalent to WordPress plugin administration.

- Admin plugin list, install, enable, disable, update, uninstall, and rollback states.
- Upload/local `.nupkg` installation plus trusted package source support.
- Dependency and compatibility validation.
- Package integrity/signature validation.
- Restart/reload orchestration and installation status.
- Plugin settings pages and configuration persistence.
- Navigation/menu contributions from plugins.

**Exit condition:** an administrator can install and manage plugins from the RemoteCommerce UI without manually editing application files.

## Stage 05 — Site and User Administration

Goal: establish the WordPress-like application administration core.

- Site identity, URL, locale, timezone, culture, and general settings.
- Users, roles, claims, permissions, sessions, and authorization.
- Admin navigation and dashboard.
- Media library foundation.
- Audit logging and configuration validation.

**Exit condition:** a fresh installation can be configured and administered without code changes.

## Stage 06 — Product Catalog

Goal: reach the WooCommerce catalog baseline.

- Products and product lifecycle.
- Simple and variable products.
- Categories, tags, attributes, and variations.
- SKU, pricing, sale pricing, tax class, stock status, and inventory quantities.
- Product media/gallery.
- Catalog search/filtering and admin CRUD.

**Exit condition:** administrators can create and manage a usable store catalog.

## Stage 07 — Customers, Cart, and Checkout

Goal: implement the core commerce transaction flow.

- Customers and customer addresses.
- Anonymous and authenticated carts.
- Cart items and pricing snapshots.
- Checkout sessions.
- Billing/shipping addresses.
- Tax calculation boundary.
- Shipping method boundary.
- Coupon/discount boundary.

**Exit condition:** a customer can progress from catalog to a valid checkout/order request.

## Stage 08 — Orders and Payments

Goal: implement WooCommerce-equivalent order management.

- Orders, order items, totals, taxes, discounts, shipping, and fees.
- Order status state machine.
- Payment intent abstraction.
- Payment provider plugin contracts.
- Refunds and payment reconciliation.
- Customer order history and admin order management.

**Exit condition:** a complete order can be created, paid through a provider, fulfilled, refunded, and audited.

## Stage 09 — Shipping, Taxes, and Store Operations

Goal: complete the operational commerce layer.

- Shipping zones, methods, rates, and fulfillment states.
- Tax zones/classes/rates.
- Inventory reservation and stock adjustments.
- Order fulfillment/shipping tracking.
- Transactional background processing.

**Exit condition:** the system supports real store operations rather than only checkout simulation.

## Stage 10 — WooCommerce-Compatible REST API

Goal: provide the API surface required for WooCommerce-compatible integrations.

- `/api/rc/v1` resource model.
- Products, variations, categories, customers, orders, coupons, and reports as appropriate.
- Authentication and authorization.
- Pagination, filtering, sorting, error contracts, and versioning.
- OpenAPI/Scalar documentation.

**Exit condition:** supported WooCommerce integrations can communicate with RemoteCommerce through the documented compatibility API.

## Stage 11 — Storefront and Theme/Extension Model

Goal: provide a complete customer-facing store experience.

- Storefront navigation.
- Product listing/detail pages.
- Cart and checkout pages.
- Customer account pages.
- Theme/layout extension points.
- Plugin page/menu/widget contributions.

**Exit condition:** a fresh RemoteCommerce installation can present a functional store without custom development.

## Stage 12 — Production Readiness

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

**Final exit condition:** a fresh RemoteCommerce installation can be configured and operated as a functional WordPress + WooCommerce-equivalent application, with plugins installed and managed through the application UI and supported commerce workflows available end to end.
