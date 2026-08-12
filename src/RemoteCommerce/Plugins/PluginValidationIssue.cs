namespace RemoteCommerce.Plugins;

/// <summary>Describes a plugin package validation problem that can be presented to an administrator.</summary>
/// <param name="Code">The stable diagnostic code.</param>
/// <param name="Message">The human-readable diagnostic message.</param>
/// <param name="Severity">The diagnostic severity.</param>
public sealed record PluginValidationIssue(
    string Code,
    string Message,
    PluginValidationSeverity Severity);

/// <summary>Defines the severity of a plugin validation diagnostic.</summary>
public enum PluginValidationSeverity
{
    /// <summary>The diagnostic prevents installation or activation.</summary>
    Error = 0,

    /// <summary>The diagnostic does not block installation but should be reviewed by an administrator.</summary>
    Warning = 1
}
