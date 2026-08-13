namespace RemoteCommerce.Application.Identity;

/// <summary>Defines authorization policy and permission names used by the administration surface.</summary>
public static class AuthorizationPolicies
{
    /// <summary>Requires the administrator role.</summary>
    public const string Administrator = "RemoteCommerce.Administrator";

    /// <summary>Permission claim used for administrative configuration.</summary>
    public const string ManageConfiguration = "configuration.manage";

    /// <summary>Permission claim used for user and role administration.</summary>
    public const string ManageUsers = "users.manage";

    /// <summary>Permission claim used for localization administration.</summary>
    public const string ManageLocalization = "localization.manage";

    /// <summary>Permission claim used for plugin administration.</summary>
    public const string ManagePlugins = "plugins.manage";
}
