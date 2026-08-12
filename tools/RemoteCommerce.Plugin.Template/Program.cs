 return await PluginTemplateGenerator.RunAsync(args);

internal static class PluginTemplateGenerator
{
    private static readonly string[] RequiredTextResources =
    [
        "Plugin.csproj.txt", "PluginEntry.cs.txt", "_Imports.razor.txt", "PluginInfo.razor.txt",
        "PluginHealthController.cs.txt", "PluginHome.razor.txt", "PluginController.cs.txt"
    ];

    private static readonly string[] RequiredDocumentResources = ["plugin.manifest.json", "README.md", "LICENSE.md"];

    public static Task<int> RunAsync(string[] args)
    {
        if (args.Length != 3 || !string.Equals(args[0], "new", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Usage: rc-plugin new <directory> <name>");
            return Task.FromResult(1);
        }

        var outputDirectory = args.Length > 1 ? Path.GetFullPath(args[1]) : PromptRequired("Output directory");
        var projectName = args.Length > 2 ? args[2] : PromptRequired("Plugin project name");
        if (Directory.Exists(outputDirectory) && Directory.EnumerateFileSystemEntries(outputDirectory).Any())
            throw new InvalidOperationException("The output directory must be empty.");
        Directory.CreateDirectory(outputDirectory);

        var manifest = PromptManifest(projectName);
        var namespaceName = SanitizeIdentifier(projectName);
        manifest = manifest with
        {
            EntryAssembly = $"lib/net10.0/{manifest.PackageId}.dll",
            EntryType = $"{namespaceName}.PluginEntry"
        };

        var baseProject = FindBaseProject(Directory.GetCurrentDirectory());
        var baseReference = Path.GetRelativePath(outputDirectory, baseProject).Replace(Path.DirectorySeparatorChar, '/');
        var resources = FindResources();
        var mode = Prompt("Extension type (page/controller/both)", "both").ToLowerInvariant();
        if (mode is not ("page" or "controller" or "both"))
            throw new InvalidOperationException("Extension type must be page, controller, or both.");

        WriteResource(resources, "Plugin.csproj.txt", outputDirectory, "Plugin.csproj", manifest, namespaceName, baseReference);
        WriteResource(resources, "plugin.manifest.json", outputDirectory, "plugin.manifest.json", manifest, namespaceName, baseReference);
        WriteResource(resources, "README.md", outputDirectory, "README.md", manifest, namespaceName, baseReference);
        WriteResource(resources, "LICENSE.md", outputDirectory, "LICENSE.md", manifest, namespaceName, baseReference);
        WriteResource(resources, "PluginEntry.cs.txt", outputDirectory, "PluginEntry.cs", manifest, namespaceName, baseReference);
        WriteResource(resources, "_Imports.razor.txt", outputDirectory, "_Imports.razor", manifest, namespaceName, baseReference);
        WriteResource(resources, "PluginInfo.razor.txt", outputDirectory, "Pages/PluginInfo.razor", manifest, namespaceName, baseReference);
        WriteResource(resources, "PluginHealthController.cs.txt", outputDirectory, "Controllers/PluginHealthController.cs", manifest, namespaceName, baseReference);

        if (mode is "page" or "both")
            WriteResource(resources, "PluginHome.razor.txt", outputDirectory, "Pages/PluginHome.razor", manifest, namespaceName, baseReference);
        if (mode is "controller" or "both")
            WriteResource(resources, "PluginController.cs.txt", outputDirectory, "Controllers/PluginController.cs", manifest, namespaceName, baseReference);

        Console.WriteLine($"Created RemoteCommerce plugin '{manifest.Name}' at {outputDirectory}.");
        Console.WriteLine($"Default plugin API prefix: /api/rp/{manifest.ApiVersion}");
        Console.WriteLine("The generated project references RemoteCommerce.Plugin.Abstractions by project reference.");
        return Task.FromResult(0);
    }

    private static PluginManifestModel PromptManifest(string projectName)
    {
        var packageId = Prompt("PackageId", SanitizeIdentifier(projectName));
        var name = PromptRequired("Plugin name");
        var id = Prompt("Plugin id", packageId.ToLowerInvariant().Replace('.', '-'));
        var version = Prompt("Version", "1.0.0");
        var description = PromptRequired("Package description");
        var tags = PromptRequired("Package tags (semicolon separated)");
        var title = Prompt("Package title", name);
        var authors = PromptRequired("Authors");
        var company = PromptRequired("Company");
        var repositoryUrl = PromptRequired("Repository URL");
        var repositoryType = Prompt("Repository type", "git");
        var projectUrl = PromptRequired("Project URL");
        var requireLicenseAcceptance = Prompt("Require license acceptance (true/false)", "true").Equals("true", StringComparison.OrdinalIgnoreCase);
        var minHostVersion = Prompt("Minimum RemoteCommerce host version", "1.0.0");
        var apiVersion = Prompt("RemoteCommerce plugin API version", "v1");
        var controllerName = Prompt("Default plugin controller name", "plugin");
        return new PluginManifestModel(id, name, "LICENSE.md", "README.md", version, "", "", minHostVersion, description, packageId, tags, title, authors, company, repositoryUrl, repositoryType, requireLicenseAcceptance, projectUrl, apiVersion, controllerName);
    }

    private static void WriteResource(string resources, string resourceName, string outputDirectory, string targetRelativePath, PluginManifestModel manifest, string ns, string baseReference)
    {
        var target = Path.Combine(outputDirectory, targetRelativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        var template = File.ReadAllText(Path.Combine(resources, resourceName));
        File.WriteAllText(target, Render(template, manifest, ns, baseReference));
    }

    private static string FindResources()
    {
        var candidates = new[] { Path.Combine(AppContext.BaseDirectory, "Resources"), Path.Combine(Directory.GetCurrentDirectory(), "Resources") };
        return candidates.FirstOrDefault(path => Directory.Exists(path) && RequiredTextResources.Concat(RequiredDocumentResources).All(file => File.Exists(Path.Combine(path, file))))
            ?? throw new InvalidOperationException("The plugin template resources could not be found. The dotnet tool package must contain its Resources directory.");
    }

    private static string Render(string template, PluginManifestModel manifest, string ns, string baseReference) => template
        .Replace("{{Namespace}}", ns, StringComparison.Ordinal)
        .Replace("{{Name}}", manifest.Name, StringComparison.Ordinal)
        .Replace("{{Description}}", manifest.Description, StringComparison.Ordinal)
        .Replace("{{PluginId}}", manifest.Id, StringComparison.Ordinal)
        .Replace("{{Version}}", manifest.Version, StringComparison.Ordinal)
        .Replace("{{Company}}", manifest.Company, StringComparison.Ordinal)
        .Replace("{{Year}}", DateTime.UtcNow.Year.ToString(), StringComparison.Ordinal)
        .Replace("{{ApiVersion}}", manifest.ApiVersion, StringComparison.Ordinal)
        .Replace("{{ControllerName}}", manifest.ControllerName, StringComparison.Ordinal)
        .Replace("{{PackageId}}", manifest.PackageId, StringComparison.Ordinal)
        .Replace("{{PackageTags}}", manifest.PackageTags, StringComparison.Ordinal)
        .Replace("{{Title}}", manifest.Title, StringComparison.Ordinal)
        .Replace("{{Authors}}", manifest.Authors, StringComparison.Ordinal)
        .Replace("{{RepositoryUrl}}", manifest.RepositoryUrl, StringComparison.Ordinal)
        .Replace("{{RepositoryType}}", manifest.RepositoryType, StringComparison.Ordinal)
        .Replace("{{PackageRequireLicenseAcceptance}}", manifest.PackageRequireLicenseAcceptance.ToString().ToLowerInvariant(), StringComparison.Ordinal)
        .Replace("{{PackageProjectUrl}}", manifest.PackageProjectUrl, StringComparison.Ordinal)
        .Replace("{{MinHostVersion}}", manifest.MinHostVersion, StringComparison.Ordinal)
        .Replace("{{BaseReference}}", baseReference, StringComparison.Ordinal);

    private static string FindBaseProject(string currentDirectory)
    {
        var directory = new DirectoryInfo(currentDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "src", "RemoteCommerce.Plugin.Abstractions", "RemoteCommerce.Plugin.Abstractions.csproj");
            if (File.Exists(candidate)) return candidate;
            directory = directory.Parent;
        }
        throw new InvalidOperationException("Could not locate src/RemoteCommerce.Plugin.Abstractions/RemoteCommerce.Plugin.Abstractions.csproj. Run the tool from the RemoteCommerce repository.");
    }

    private static string PromptRequired(string label) => Prompt(label, null, true);
    private static string Prompt(string label, string? defaultValue = null, bool required = false)
    {
        while (true)
        {
            Console.Write(defaultValue is null ? $"{label}: " : $"{label} [{defaultValue}]: ");
            var value = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(value)) value = defaultValue;
            if (!string.IsNullOrWhiteSpace(value) || !required) return value ?? string.Empty;
            Console.WriteLine("A value is required.");
        }
    }

    private static string SanitizeIdentifier(string value)
    {
        var builder = new StringBuilder();
        foreach (var c in value) builder.Append(char.IsLetterOrDigit(c) || c == '_' ? c : '_');
        if (builder.Length == 0 || char.IsDigit(builder[0])) builder.Insert(0, '_');
        return builder.ToString();
    }

    private sealed record PluginManifestModel(string Id, string Name, string License, string Readme, string Version, string EntryAssembly, string EntryType, string MinHostVersion, string Description, string PackageId, string PackageTags, string Title, string Authors, string Company, string RepositoryUrl, string RepositoryType, bool PackageRequireLicenseAcceptance, string PackageProjectUrl, string ApiVersion, string ControllerName);
}
