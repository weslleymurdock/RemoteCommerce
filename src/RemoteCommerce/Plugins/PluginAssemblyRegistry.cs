namespace RemoteCommerce.Plugins;

/// <summary>Provides the startup-only registry of plugin assemblies used by host routing infrastructure.</summary>
public static class PluginAssemblyRegistry
{
    private static readonly List<Assembly> AssembliesInternal = [];

    /// <summary>Gets the plugin assemblies loaded during the current application startup.</summary>
    public static IReadOnlyList<Assembly> Assemblies => AssembliesInternal;

    /// <summary>Registers a plugin assembly for host component discovery.</summary>
    /// <param name="assembly">The plugin assembly to register.</param>
    internal static void Add(Assembly assembly)
    {
        if (!AssembliesInternal.Contains(assembly)) AssembliesInternal.Add(assembly);
    }
}
