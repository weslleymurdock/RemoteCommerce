namespace RemoteCommerce.Infrastructure.Persistence.Migrations;

/// <summary>Changes mutable plugin uniqueness indexes to exclude soft-deleted records.</summary>
[Migration("20260812224000_Stage05SoftDeleteIndexes")]
public partial class Stage05SoftDeleteIndexes : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_PluginInstallations_PluginId", schema: "commerce", table: "PluginInstallations");
        migrationBuilder.CreateIndex(name: "IX_PluginInstallations_PluginId", schema: "commerce", table: "PluginInstallations", column: "PluginId", unique: true, filter: "[IsDeleted] = 0");

        migrationBuilder.DropIndex(name: "IX_PluginVersions_PluginId_Version", schema: "commerce", table: "PluginVersions");
        migrationBuilder.CreateIndex(name: "IX_PluginVersions_PluginId_Version", schema: "commerce", table: "PluginVersions", columns: new[] { "PluginId", "Version" }, unique: true, filter: "[IsDeleted] = 0");

        migrationBuilder.DropIndex(name: "IX_PluginDependencies_PluginId_DependencyPluginId", schema: "commerce", table: "PluginDependencies");
        migrationBuilder.CreateIndex(name: "IX_PluginDependencies_PluginId_DependencyPluginId", schema: "commerce", table: "PluginDependencies", columns: new[] { "PluginId", "DependencyPluginId" }, unique: true, filter: "[IsDeleted] = 0");

        migrationBuilder.DropIndex(name: "IX_PluginSettings_PluginId_Key", schema: "commerce", table: "PluginSettings");
        migrationBuilder.CreateIndex(name: "IX_PluginSettings_PluginId_Key", schema: "commerce", table: "PluginSettings", columns: new[] { "PluginId", "Key" }, unique: true, filter: "[IsDeleted] = 0");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_PluginInstallations_PluginId", schema: "commerce", table: "PluginInstallations");
        migrationBuilder.CreateIndex(name: "IX_PluginInstallations_PluginId", schema: "commerce", table: "PluginInstallations", column: "PluginId", unique: true);
        migrationBuilder.DropIndex(name: "IX_PluginVersions_PluginId_Version", schema: "commerce", table: "PluginVersions");
        migrationBuilder.CreateIndex(name: "IX_PluginVersions_PluginId_Version", schema: "commerce", table: "PluginVersions", columns: new[] { "PluginId", "Version" }, unique: true);
        migrationBuilder.DropIndex(name: "IX_PluginDependencies_PluginId_DependencyPluginId", schema: "commerce", table: "PluginDependencies");
        migrationBuilder.CreateIndex(name: "IX_PluginDependencies_PluginId_DependencyPluginId", schema: "commerce", table: "PluginDependencies", columns: new[] { "PluginId", "DependencyPluginId" }, unique: true);
        migrationBuilder.DropIndex(name: "IX_PluginSettings_PluginId_Key", schema: "commerce", table: "PluginSettings");
        migrationBuilder.CreateIndex(name: "IX_PluginSettings_PluginId_Key", schema: "commerce", table: "PluginSettings", columns: new[] { "PluginId", "Key" }, unique: true);
    }
}
