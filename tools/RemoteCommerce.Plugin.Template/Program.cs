using System.Security;
using System.Text;
using System.Text.Json;

return await PluginTemplateGenerator.RunAsync(args);

internal static class PluginTemplateGenerator
{
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

        File.WriteAllText(Path.Combine(outputDirectory, $"{manifest.PackageId}.csproj"), BuildProjectFile(manifest, baseReference));
        File.WriteAllText(Path.Combine(outputDirectory, "plugin.manifest.json"), JsonSerializer.Serialize(manifest, new JsonSerializerOptions(JsonSerializerDefaults.Web) { WriteIndented = true }));
        File.WriteAllText(Path.Combine(outputDirectory, "README.md"), $"# {manifest.Name}\n\n{manifest.Description}\n");
        File.WriteAllText(Path.Combine(outputDirectory, "LICENSE.md"), "Mozilla Public License Version 2.0\n\nReplace this file with the license terms applicable to your plugin.\n");
        File.WriteAllText(Path.Combine(outputDirectory, "PluginEntry.cs"), BuildEntry(namespaceName));
        File.WriteAllText(Path.Combine(outputDirectory, "_Imports.razor"), "@using Microsoft.AspNetCore.Components\n@using Microsoft.AspNetCore.Components.Routing\n@using Microsoft.AspNetCore.Components.Web\n");

        var mode = Prompt("Extension type (page/controller/both)", "both").ToLowerInvariant();
        if (mode is "page" or "both")
        {
            Directory.CreateDirectory(Path.Combine(outputDirectory, "Pages"));
            File.WriteAllText(Path.Combine(outputDirectory, "Pages", "PluginHome.razor"), $"@page \"/plugins/{manifest.Id}\"\n\n<h1>{EscapeRazor(manifest.Name)}</h1>\n<p>{EscapeRazor(manifest.Description)}</p>\n");
        }
        if (mode is "controller" or "both")
        {
            Directory.CreateDirectory(Path.Combine(outputDirectory, "Controllers"));
            File.WriteAllText(Path.Combine(outputDirectory, "Controllers", "PluginController.cs"), BuildController(manifest, namespaceName));
        }

        Console.WriteLine($"Created RemoteCommerce plugin '{manifest.Name}' at {outputDirectory}.");
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
        return new PluginManifestModel(id, name, "LICENSE.md", "README.md", version, "", "", minHostVersion, description, packageId, tags, title, authors, company, repositoryUrl, repositoryType, requireLicenseAcceptance, projectUrl);
    }

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

    private static string BuildEntry(string ns) => $"""using Microsoft.Extensions.DependencyInjection;
using RemoteCommerce.Plugins.Abstractions;

namespace {ns};

/// <summary>Registers the generated RemoteCommerce plugin.</summary>
public sealed class PluginEntry : IRemoteCommercePlugin
{{
    /// <summary>Registers plugin services and discovers plugin MVC and Razor components.</summary>
    /// <param name=\"services\">The host service collection.</param>
    /// <param name=\"manifest\">The installed plugin manifest.</param>
    public void ConfigureServices(IServiceCollection services, PluginManifest manifest)
    {{
        services.AddControllers().AddApplicationPart(typeof(PluginEntry).Assembly);
        services.AddRazorComponents().AddAdditionalAssemblies(typeof(PluginEntry).Assembly);
    }}
}}
""";

    private static string BuildController(PluginManifestModel m, string ns) => $"""using Microsoft.AspNetCore.Mvc;
namespace {ns}.Controllers;
/// <summary>Provides the generated plugin controller endpoint.</summary>
[ApiController]
[Route(\"api/plugins/{m.Id}\")]
public sealed class PluginController : ControllerBase
{{
    /// <summary>Returns basic information about the plugin.</summary>
    /// <returns>The plugin identifier and version.</returns>
    [HttpGet]
    public object Get() => new {{ PluginId = \"{m.Id}\", Version = \"{m.Version}\" }};
}}
""";

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

    private static string Xml(string value) => SecurityElement.Escape(value) ?? string.Empty;
    private static string EscapeRazor(string value) => value.Replace("\"", "&quot;", StringComparison.Ordinal);
    private sealed record PluginManifestModel(string Id, string Name, string License, string Readme, string Version, string EntryAssembly, string EntryType, string MinHostVersion, string Description, string PackageId, string PackageTags, string Title, string Authors, string Company, string RepositoryUrl, string RepositoryType, bool PackageRequireLicenseAcceptance, string PackageProjectUrl);
}
