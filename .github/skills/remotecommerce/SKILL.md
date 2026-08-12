# RemoteCommerce architecture skill

Use for feature implementation and review.

- Inspect the single-project structure before adding files.
- Map WooCommerce concepts to explicit .NET contracts and domain services.
- Keep controllers thin and Razor components focused on UI.
- Use EF Core through DI and async I/O.
- Keep plugin manifest discovery, validation, installation, persistence and activation as separate lifecycle phases.
- Activate plugins only while building the host so plugin services participate in the application DI container.
- Never mutate the running root service provider to activate a plugin.
- Add focused tests or smoke endpoints for externally observable capabilities.

Plugins are trusted executable code. Installation must validate package integrity, manifest compatibility and safe paths before marking an installation successful.
