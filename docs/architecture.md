# Architecture

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

## Plugins

A plugin is distributed as a `.nupkg`. During development the generated plugin may reference `RemoteCommerce.Plugin.Abstractions` by project reference; released plugins consume the abstraction as a NuGet package.

The plugin manifest describes identity, package metadata, compatibility, entry point, README, and LICENSE information. Installation is persisted and the plugin becomes active after the host is restarted. The entry point receives the host `IConfiguration` so plugins can consume host defaults without duplicating configuration sources.

Plugin controllers are registered as MVC application parts. Plugin Razor components are registered with the Blazor router as additional assemblies.

## API namespaces

- `/api/rp/v1/...` is the RemoteCommerce plugin API namespace.
- `/api/rc/v1/...` is reserved for APIs ported from WooCommerce.
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

Plugins should extend these boundaries without requiring changes to the host for normal feature additions.
