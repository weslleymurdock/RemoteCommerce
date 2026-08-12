# AI implementation instructions

- Act as a senior .NET 10 engineer.
- Inspect `modules/woocommerce` when mapping WooCommerce concepts, but do not copy PHP implementation details into the .NET architecture.
- Preserve the single-project constraint for the host: Blazor + controllers + persistence + plugin runtime are in one ASP.NET Core project.
- Use primary constructors where they improve clarity.
- Use DI for all application services and plugin capabilities.
- Treat plugin installation and plugin activation as separate lifecycle phases.
- A successful installation writes an immutable/versioned plugin payload and installation state; activation occurs only after restart.
- Avoid reflection in normal application paths; reflection is isolated to the plugin bootstrap boundary.
- Prefer async APIs for I/O and EF Core operations.
- Do not introduce a second project merely to satisfy layering. Use namespaces/folders and explicit contracts within the single project.
- Changes must be incremental, buildable, and testable.
