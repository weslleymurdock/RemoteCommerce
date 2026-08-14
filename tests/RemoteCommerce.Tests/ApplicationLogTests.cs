namespace RemoteCommerce.Tests;

public sealed class ApplicationLogTests
{
    [Fact]
    public void FileLoggerUsesRequiredStructuredFormat()
    {
        var directory = Path.Combine(Path.GetTempPath(), "remotecommerce-tests", Guid.NewGuid().ToString("N"));
        var path = Path.Combine(directory, "application.log");
        try
        {
            using var provider = new FileLoggerProvider(path);
            var logger = provider.CreateLogger("RemoteCommerce.Tests.ApplicationLogTests");
            logger.LogInformation("Stage 07 validation message");

            var line = File.ReadAllLines(path).Single();
            Assert.Matches("^\\[[^\\]]+\\]\\[Information\\]\\[RemoteCommerce\\.Tests\\.ApplicationLogTests\\]\\[Stage 07 validation message\\]$", line);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void ApplicationLogReaderReturnsRecentLines()
    {
        var directory = Path.Combine(Path.GetTempPath(), "remotecommerce-tests", Guid.NewGuid().ToString("N"));
        var path = Path.Combine(directory, "application.log");
        Directory.CreateDirectory(directory);
        try
        {
            File.WriteAllLines(path, ["one", "two", "three"]);
            var lines = new ApplicationLogReader(path).ReadRecent(2);
            Assert.Equal(["two", "three"], lines);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
    }
}
