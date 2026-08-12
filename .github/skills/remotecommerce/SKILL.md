# RemoteCommerce architecture skill

Use for feature implementation and review.

- Inspect the single-project host structure before adding application files.
- Map WooCommerce concepts to explicit .NET contracts and domain services.
- Keep controllers thin and Razor components focused on UI.
- Use EF Core through DI and async I/O.
- Keep plugin manifest discovery, validation, package extraction, installation persistence and startup activation as separate lifecycle phases.
- Treat `.nupkg` as the only supported plugin distribution format.
- Keep stable plugin contracts in `RemoteCommerce.Plugin.Abstractions`; plugin implementations must not depend on host internals.
- Activate plugins only while building the host so plugin services participate in the final application DI container.
- Never mutate the running root service provider to activate, disable, or uninstall a plugin.
- Enable/disable/uninstall changes are persisted immediately and become effective after restart.
- Validate package paths against traversal, require `plugin.manifest.json` at package root, and require entry assemblies under `lib/net10.0`.
- Prefer a staging directory and atomic directory move for package installation.
- Use structured logging for plugin load failures without preventing unrelated plugins or the host from starting.
- Add focused tests or smoke endpoints for externally observable capabilities.
- Every public API must have complete en-US XML documentation appropriate to its members.

Plugins are trusted executable code. Package validation reduces accidental corruption and path traversal but is not a security boundary; future production hardening must add package signatures, publisher trust and authorization before accepting untrusted plugins.
