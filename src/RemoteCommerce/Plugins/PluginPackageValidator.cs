using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using RemoteCommerce.Plugins.Abstractions;

namespace RemoteCommerce.Plugins;

/// <summary>Inspects plugin packages without extracting or activating plugin code.</summary>
/// <param name="manifestValidator">The validator for manifest semantics.</param>
/// <param name="compatibilityValidator">The validator for host compatibility.</param>
public sealed class PluginPackageValidator(
    IPluginManifestValidator manifestValidator,
    IPluginCompatibilityValidator compatibilityValidator) : IPluginPackageValidator
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <inheritdoc />
    public async Task<PluginPackageValidationResult> ValidateAsync(string packagePath, CancellationToken cancellationToken = default)
    {
        var issues = new List<PluginValidationIssue>();
        if (!File.Exists(packagePath))
        {
            issues.Add(new("PACKAGE_NOT_FOUND", "The plugin package file was not found.", PluginValidationSeverity.Error));
            return new(null, string.Empty, issues);
        }

        if (!string.Equals(Path.GetExtension(packagePath), ".nupkg", StringComparison.OrdinalIgnoreCase))
        {
            issues.Add(new("PACKAGE_EXTENSION_INVALID", "RemoteCommerce plugins must be distributed as .nupkg files.", PluginValidationSeverity.Error));
            return new(null, string.Empty, issues);
        }

        var hash = await ComputeHashAsync(packagePath, cancellationToken);
        try
        {
            using var archive = ZipFile.OpenRead(packagePath);
            var manifestEntry = archive.GetEntry("plugin.manifest.json");
            if (manifestEntry is null)
            {
                issues.Add(new("MANIFEST_MISSING", "The package must contain plugin.manifest.json at its root.", PluginValidationSeverity.Error));
                return new(null, hash, issues);
            }

            PluginManifest? manifest;
            try
            {
                await using var stream = manifestEntry.Open();
                manifest = await JsonSerializer.DeserializeAsync<PluginManifest>(stream, JsonOptions, cancellationToken);
            }
            catch (JsonException exception)
            {
                issues.Add(new("MANIFEST_INVALID_JSON", $"The plugin manifest is not valid JSON: {exception.Message}", PluginValidationSeverity.Error));
                return new(null, hash, issues);
            }

            if (manifest is null)
            {
                issues.Add(new("MANIFEST_EMPTY", "The plugin manifest is empty.", PluginValidationSeverity.Error));
                return new(null, hash, issues);
            }

            issues.AddRange(manifestValidator.Validate(manifest));
            issues.AddRange(compatibilityValidator.Validate(manifest));
            ValidateRequiredPackageEntries(archive, manifest, issues);
            ValidateEntryAssemblyMetadata(archive, manifest, issues);

            return new(manifest, hash, issues);
        }
        catch (InvalidDataException exception)
        {
            issues.Add(new("PACKAGE_INVALID", $"The .nupkg archive is invalid: {exception.Message}", PluginValidationSeverity.Error));
            return new(null, hash, issues);
        }
    }

    private static void ValidateRequiredPackageEntries(ZipArchive archive, PluginManifest manifest, List<PluginValidationIssue> issues)
    {
        foreach (var required in new[] { "README.md", "LICENSE.md" })
        {
            var entry = archive.GetEntry(required);
            if (entry is null || entry.Length == 0)
                issues.Add(new($"{required[..^3].ToUpperInvariant()}_MISSING", $"The package must contain a non-empty {required} at its root.", PluginValidationSeverity.Error));
        }

        var normalizedAssemblyPath = manifest.EntryAssembly.Replace('\\', '/');
        var assemblyEntry = archive.GetEntry(normalizedAssemblyPath);
        if (assemblyEntry is null)
            issues.Add(new("ENTRY_ASSEMBLY_MISSING", $"The entry assembly '{manifest.EntryAssembly}' is not present in the package.", PluginValidationSeverity.Error));
    }

    private static void ValidateEntryAssemblyMetadata(ZipArchive archive, PluginManifest manifest, List<PluginValidationIssue> issues)
    {
        var entry = archive.GetEntry(manifest.EntryAssembly.Replace('\\', '/'));
        if (entry is null || entry.Length == 0)
            return;

        var temporaryPath = Path.Combine(Path.GetTempPath(), "RemoteCommerce", "validation", $"{Guid.NewGuid():N}.dll");
        Directory.CreateDirectory(Path.GetDirectoryName(temporaryPath)!);
        try
        {
            entry.ExtractToFile(temporaryPath, true);
            var assemblyName = AssemblyName.GetAssemblyName(temporaryPath);
            if (string.IsNullOrWhiteSpace(assemblyName.Name))
                issues.Add(new("ENTRY_ASSEMBLY_INVALID", "The plugin entry assembly does not contain valid assembly metadata.", PluginValidationSeverity.Error));
        }
        catch (BadImageFormatException exception)
        {
            issues.Add(new("ENTRY_ASSEMBLY_INVALID", $"The plugin entry assembly is not a valid .NET assembly: {exception.Message}", PluginValidationSeverity.Error));
        }
        finally
        {
            try { if (File.Exists(temporaryPath)) File.Delete(temporaryPath); } catch { }
        }
    }

    private static async Task<string> ComputeHashAsync(string path, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(path);
        var hash = await SHA256.HashDataAsync(stream, cancellationToken);
        return Convert.ToHexString(hash);
    }
}
