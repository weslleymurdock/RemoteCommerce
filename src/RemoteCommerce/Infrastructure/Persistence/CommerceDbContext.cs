using Microsoft.EntityFrameworkCore;

namespace RemoteCommerce.Infrastructure.Persistence;

public sealed class CommerceDbContext(DbContextOptions<CommerceDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("commerce");
        base.OnModelCreating(modelBuilder);
    }
}
