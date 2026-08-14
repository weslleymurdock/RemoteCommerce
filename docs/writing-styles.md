# Writing Styles

## Code

- Prefer clear, explicit .NET 10 APIs over clever abstractions.
- Prefer file-scoped namespaces.
- Public types, members, interfaces, controllers, services, methods, extension methods, and DI contracts must have XML documentation in en-US.
- XML documentation should include all applicable tags such as `<summary>`, `<param>`, `<returns>`, `<typeparam>`, `<exception>`, and `<remarks>`.
- Use domain terminology consistently: RemoteCommerce, plugin, manifest, host, catalog, order, customer.

## Feature organization

- Organize Application code by feature rather than by technical concern across the whole application.
- Each Application feature uses `Abstractions`, `Commands`, `Handlers`, `Queries`, `Requests`, `Resources`, `Results`, and `Validators` folders when those concerns exist.
- Keep Domain and Infrastructure feature folders aligned with the same feature name.
- Keep the physical layout compatible with future `RemoteCommerce` class library projects.

## Source formatting

- One C# instruction per line.
- One C# method call per line when it is an executable operation.
- One logical C# statement per line.
- Do not compress multiple statements onto one line for brevity.
- One Razor directive per line.
- One HTML or Razor component invocation per line when it has attributes or child content.
- Keep executable Razor expressions, callbacks, and method calls independently readable.
- Apply the rule to production code, tests, templates, and generated source.

## Data-flow documentation

- Document architecture using the canonical flow `Requests → MediatR Handlers → Behaviors → Feature Services → Repository<T> → DbContext|Storage`.
- Explicitly distinguish Application abstractions from Infrastructure implementations.
- Do not document provider-specific types as Application or Domain contracts.

## Documentation

- Write technical documentation in concise English unless the user requests another language.
- Explain intent, invariants, extension points, and operational consequences.
- Prefer examples that compile or map directly to implemented contracts.
- Never document an implementation as available before it exists.

## Architecture decisions

Document decisions as constraints and rationale, not as transient implementation details. When an API or file layout is part of a public contract, include the exact route or type name.
