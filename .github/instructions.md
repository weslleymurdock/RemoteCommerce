# AI implementation instructions

- Act as a senior .NET 10 engineer.
- Inspect `modules/woocommerce` when mapping WooCommerce concepts, but do not copy PHP implementation details into the .NET architecture.
- Preserve the single-project constraint for the host: Blazor + controllers + persistence + plugin runtime are in one ASP.NET Core project.
- The stable plugin contract is the deliberate exception to the host single-project rule and lives in `src/RemoteCommerce.Plugin.Abstractions` so independently built `.nupkg` plugins can consume a stable SDK without referencing host internals.
- Use primary constructors where they improve clarity.
- Use DI for all application services and plugin capabilities.
- Treat plugin installation and plugin activation as separate lifecycle phases.
- The only supported plugin distribution format is `.nupkg`.
- A successful installation writes the package payload and immutable/versioned installation state; activation occurs only after restart.
- Never attempt to mutate the running root service provider to activate a plugin.
- Avoid reflection in normal application paths; reflection is isolated to the plugin bootstrap boundary.
- Prefer async APIs for I/O and EF Core operations.
- Changes must be incremental, buildable, and testable.
- Every public API must include complete applicable XML documentation in en-US.
- Preserve existing OpenAPI/Scalar configuration when extending controllers.
