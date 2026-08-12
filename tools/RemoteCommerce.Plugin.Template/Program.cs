using System.Security;
using System.Text;
using System.Text.Json;

return await PluginTemplateGenerator.RunAsync(args);

internal static class PluginTemplateGenerator
{
    private static readonly string[] RequiredResources = ["README.md", "LICENSE.md", "PluginEntry.cs", "PluginInfo.razor", "PluginHealthController.cs"];

    public static Task<int> RunAsync(string[] args)
    {
        if (args.Length == 0 || !string.Equals(args[0], "new", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Usage: remotecommerce-plugin new <directory> <name>");
            return Task.FromResult(1);
        }

        var outputDirectory = args.Length > 1 ? Path.GetFullPath(args[1]) : PromptRequired("Output directory");
        var projectName = args.Length > 2 ? args[2] : PromptRequired("Plugin project name");
        if (Directory.Exists(outputDirectory) && Directory.EnumerateFileSystemEntries(outputDirectory).Any()) throw new InvalidOperationException("The output directory must be empty.");
        Directory.CreateDirectory(outputDirectory);

        var manifest = PromptManifest(projectName);
        var namespaceName = SanitizeIdentifier(projectName);
        manifest = manifest with { EntryAssembly = $"lib/net10.0/{manifest.PackageId}.dll", EntryType = $"{namespaceName}.PluginEntry" };
        var baseProject = FindBaseProject(Directory.GetCurrentDirectory());
        var baseReference = Path.GetRelativePath(outputDirectory, baseProject).Replace(Path.DirectorySeparatorChar, '/');
        var resources = FindResources();

        File.WriteAllText(Path.Combine(outputDirectory, $"{manifest.PackageId}.csproj"), BuildProjectFile(manifest, baseReference));
        File.WriteAllText(Path.Combine(outputDirectory, "plugin.manifest.json"), JsonSerializer.Serialize(manifest, new JsonSerializerOptions(JsonSerializerDefaults.Web) { WriteIndented = true }));
        foreach (var resourceName in RequiredResources)
        {
            var target = resourceName switch
            {
                "PluginEntry.cs" => Path.Combine(outputDirectory, resourceName),
                "PluginInfo.razor" => Path.Combine(outputDirectory, "Pages", resourceName),
                "PluginHealthController.cs" => Path.Combine(outputDirectory, "Controllers", resourceName),
                _ => Path.Combine(outputDirectory, resourceName)
            };
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.WriteAllText(target, Render(File.ReadAllText(Path.Combine(resources, resourceName)), manifest, namespaceName));
        }

        var mode = Prompt("Extension type (page/controller/both)", "both").ToLowerInvariant();
        if (mode is "page" or "both")
        {
            Directory.CreateDirectory(Path.Combine(outputDirectory, "Pages"));
            File.WriteAllText(Path.Combine(outputDirectory, "Pages", "PluginHome.razor"), Render("@page \"/plugins/{{PluginId}}/home\"\n\n<MudText Typo=\"Typo.h4\">{{Name}}</MudText>\n<MudText>{{Description}}</MudText>\n", manifest, namespaceName));
        }
        if (mode is "controller" or "both")
        {
            Directory.CreateDirectory(Path.Combine(outputDirectory, "Controllers"));
            File.WriteAllText(Path.Combine(outputDirectory, "Controllers", "PluginController.cs"), Render("using Microsoft.AspNetCore.Mvc;\n\nnamespace {{Namespace}}.Controllers;\n\n/// <summary>Provides the generated plugin API endpoint.</summary>\n[ApiController]\n[Route(\"api/rp/{{ApiVersion}}/{{ControllerName}}\")]\npublic sealed class PluginController : ControllerBase\n{\n    /// <summary>Returns basic plugin information.</summary>\n    /// <returns>The plugin identifier and version.</returns>\n    [HttpGet]\n    public object Get() => new { PluginId = \"{{PluginId}}\", Version = \"{{Version}}\" };\n}\n", manifest, namespaceName));
        }

        Console.WriteLine($"Created RemoteCommerce plugin '{manifest.Name}' at {outputDirectory}.");
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

    private static string FindResources()
    {
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "Resources"),
            Path.Combine(Directory.GetCurrentDirectory(), "Resources")
        };
        var resources = candidates.FirstOrDefault(path => Directory.Exists(path) && RequiredResources.All(file => File.Exists(Path.Combine(path, file))));
        return resources ?? throw new InvalidOperationException("The plugin template resources could not be found. The dotnet tool package must contain its Resources directory.");
    }

    private static string Render(string template, PluginManifestModel manifest, string ns) => template
        .Replace("{{Namespace}}", ns, StringComparison.Ordinal)
        .Replace("{{Name}}", manifest.Name, StringComparison.Ordinal)
        .Replace("{{Description}}", manifest.Description, StringComparison.Ordinal)
        .Replace("{{PluginId}}", manifest.Id, StringComparison.Ordinal)
        .Replace("{{Version}}", manifest.Version, StringComparison.Ordinal)
        .Replace("{{Company}}", manifest.Company, StringComparison.Ordinal)
        .Replace("{{Year}}", DateTime.UtcNow.Year.ToString(), StringComparison.Ordinal)
        .Replace("{{ApiVersion}}", manifest.ApiVersion, StringComparison.Ordinal)
        .Replace("{{ControllerName}}", manifest.ControllerName, StringComparison.Ordinal);

    private static string BuildProjectFile(PluginManifestModel m, string baseReference) => $"""<Project Sdk=\"Microsoft.NET.Sdk.Razor\">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <AssemblyName>{Xml(m.PackageId)}</AssemblyName>
    <RootNamespace>{Xml(SanitizeIdentifier(m.PackageId))}</RootNamespace>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
    <PackageId>{Xml(m.PackageId)}</PackageId>
    <Version>{Xml(m.Version)}</Version>
    <PackageDescription>{Xml(m.Description)}</PackageDescription>
    <PackageTags>{Xml(m.PackageTags)}</PackageTags>
    <Title>{Xml(m.Title)}</Title>
    <Authors>{Xml(m.Authors)}</Authors>
    <Company>{Xml(m.Company)}</Company>
    <RepositoryUrl>{Xml(m.RepositoryUrl)}</RepositoryUrl>
    <RepositoryType>{Xml(m.RepositoryType)}</RepositoryType>
    <PackageLicenseFile>LICENSE.md</PackageLicenseFile>
    <PackageRequireLicenseAcceptance>{m.PackageRequireLicenseAcceptance}</PackageRequireLicenseAcceptance>
    <PackageReadmeFile>README.md</PackageReadmeFile>
    <PackageProjectUrl>{Xml(m.PackageProjectUrl)}</PackageProjectUrl>
  </PropertyGroup>
  <ItemGroup>
    <FrameworkReference Include=\"Microsoft.AspNetCore.App\" />
    <ProjectReference Include=\"{Xml(baseReference)}\" />
    <None Include=\"plugin.manifest.json\" Pack=\"true\" PackagePath=\"\" CopyToOutputDirectory=\"PreserveNewest\" />
    <None Update=\"LICENSE.md\" Pack=\"true\" PackagePath=\"\" />
    <None Update=\"README.md\" Pack=\"true\" PackagePath=\"\" />
  </ItemGroup>
</Project>
""";

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

    private static string Xml(string value) => SecurityElement.Escape(value) ?? string.Empty;
    private sealed record PluginManifestModel(string Id, string Name, string License, string Readme, string Version, string EntryAssembly, string EntryType, string MinHostVersion, string Description, string PackageId, string PackageTags, string Title, string Authors, string Company, string RepositoryUrl, string RepositoryType, bool PackageRequireLicenseAcceptance, string PackageProjectUrl, string ApiVersion, string ControllerName);
}
