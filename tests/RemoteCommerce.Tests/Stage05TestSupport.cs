namespace RemoteCommerce.Tests;

internal sealed class TestDbContextFactory(CommerceDbContext db) : IDbContextFactory<CommerceDbContext>
{
    public CommerceDbContext CreateDbContext()
    {
        return db;
    }

    public Task<CommerceDbContext> CreateDbContextAsync(
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(db);
    }
}

internal sealed class TestApplicationContext : IApplicationContext
{
    public Guid? UserId =>
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    public string Actor =>
        "test-user";

    public string CorrelationId =>
        "test-correlation";

    public string? IpAddress =>
        "127.0.0.1";
}

public sealed record PingQuery(string Value) : IQuery<string>;

public sealed class PingQueryValidator : AbstractValidator<PingQuery>
{
    public PingQueryValidator()
    {
        RuleFor(x => x.Value)
            .NotEmpty();
    }
}

public sealed class PingQueryHandler : IRequestHandler<PingQuery, string>
{
    public Task<string> Handle(
        PingQuery request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(request.Value);
    }
}

internal sealed record FailingCommand : ITransactionalCommand;
