# Writing Styles

## Code

- Prefer clear, explicit .NET 10 APIs over clever abstractions.
- Prefer file-scoped namespaces.
- Public types, members, interfaces, controllers, services, extension methods, and DI contracts must have XML documentation in en-US.
- XML documentation should include all applicable tags such as `<summary>`, `<param>`, `<returns>`, `<typeparam>`, `<exception>`, and `<remarks>`.
- Use domain terminology consistently: RemoteCommerce, plugin, manifest, host, catalog, order, customer.

## Documentation

- Write technical documentation in concise English unless the user requests another language.
- Explain intent, invariants, extension points, and operational consequences.
- Prefer examples that compile or map directly to implemented contracts.
- Never document an implementation as available before it exists.

## Architecture decisions

Document decisions as constraints and rationale, not as transient implementation details. When an API or file layout is part of a public contract, include the exact route or type name.
