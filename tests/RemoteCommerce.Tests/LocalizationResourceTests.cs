using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using RemoteCommerce.Application.Localization;
using RemoteCommerce.Infrastructure.Persistence;

namespace RemoteCommerce.Tests;

public sealed class LocalizationResourceTests
{
    [Fact]
    public async Task ImportAsync_StoresValidResourceAndVersion()
    {
        var root = Path.Combine(Path.GetTempPath(), "RemoteCommerceTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var service = new LocalizationResourceService(CreateFactory(), new TestEnvironment(root));
            await using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("<root><data name=\"Hello\"><value>Olá</value></data></root>"));

            var result = await service.ImportAsync(stream, "pt-BR", "RemoteCommerce.Tests.SharedResource", null);

            Assert.Equal(1, result.Version);
            Assert.Equal(1, result.EntryCount);
            Assert.True(File.Exists(Path.Combine(root, "App_Data", "localization", "RemoteCommerce.Tests.SharedResource", "pt-BR", "v1.xml")));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ImportAsync_RejectsDuplicateKeys()
    {
        var root = Path.Combine(Path.GetTempPath(), "RemoteCommerceTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var service = new LocalizationResourceService(CreateFactory(), new TestEnvironment(root));
            await using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("<root><data name=\"Hello\"><value>A</value></data><data name=\"Hello\"><value>B</value></data></root>"));

            await Assert.ThrowsAsync<InvalidDataException>(() => service.ImportAsync(stream, "en-US", "SharedResource", null));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ImportAsync_RejectsExternalEntities()
    {
        var root = Path.Combine(Path.GetTempPath(), "RemoteCommerceTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var service = new LocalizationResourceService(CreateFactory(), new TestEnvironment(root));
            var xml = "<!DOCTYPE root [<!ENTITY xxe SYSTEM 'file:///etc/passwd'>]><root><data name=\"Hello\"><value>&xxe;</value></data></root>";
            await using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xml));

            await Assert.ThrowsAnyAsync<Exception>(() => service.ImportAsync(stream, "en-US", "SharedResource", null));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ImportAsync_RejectsUnsupportedCulture()
    {
        var service = new LocalizationResourceService(CreateFactory(), new TestEnvironment(Path.GetTempPath()));
        await using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("<root />"));

        await Assert.ThrowsAsync<ArgumentException>(() => service.ImportAsync(stream, "fr-FR", "SharedResource", null));
    }

    private static IDbContextFactory<CommerceDbContext> CreateFactory()
    {
        var options = new DbContextOptionsBuilder<CommerceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new PooledDbContextFactory<CommerceDbContext>(options);
    }

    private sealed class TestEnvironment(string contentRootPath) : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "RemoteCommerce.Tests";
        public string WebRootPath { get; set; } = contentRootPath;
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = contentRootPath;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
