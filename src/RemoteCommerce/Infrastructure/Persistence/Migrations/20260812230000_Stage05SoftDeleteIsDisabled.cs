namespace RemoteCommerce.Infrastructure.Persistence.Migrations;

/// <summary>Renames the Stage 05 soft-delete storage column to the canonical IsDisabled name.</summary>
[Migration("20260812230000_Stage05SoftDeleteIsDisabled")]
public partial class Stage05SoftDeleteIsDisabled : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PluginInstallations_PluginId' AND object_id = OBJECT_ID(N'commerce.PluginInstallations'))
                DROP INDEX [IX_PluginInstallations_PluginId] ON [commerce].[PluginInstallations];
            IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PluginVersions_PluginId_Version' AND object_id = OBJECT_ID(N'commerce.PluginVersions'))
                DROP INDEX [IX_PluginVersions_PluginId_Version] ON [commerce].[PluginVersions];
            IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PluginDependencies_PluginId_DependencyPluginId' AND object_id = OBJECT_ID(N'commerce.PluginDependencies'))
                DROP INDEX [IX_PluginDependencies_PluginId_DependencyPluginId] ON [commerce].[PluginDependencies];
            IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PluginSettings_PluginId_Key' AND object_id = OBJECT_ID(N'commerce.PluginSettings'))
                DROP INDEX [IX_PluginSettings_PluginId_Key] ON [commerce].[PluginSettings];

            DECLARE @table sysname;
            DECLARE tables_cursor CURSOR LOCAL FAST_FORWARD FOR
                SELECT value FROM (VALUES
                    (N'PluginInstallations'),
                    (N'PluginVersions'),
                    (N'PluginDependencies'),
                    (N'PluginSettings'),
                    (N'AspNetUsers'),
                    (N'AspNetRoles'),
                    (N'SiteSettings'),
                    (N'LocalizationResources')) AS tables(value);
            OPEN tables_cursor;
            FETCH NEXT FROM tables_cursor INTO @table;
            WHILE @@FETCH_STATUS = 0
            BEGIN
                IF COL_LENGTH(N'commerce.' + @table, N'IsDeleted') IS NOT NULL
                   AND COL_LENGTH(N'commerce.' + @table, N'IsDisabled') IS NULL
                    EXEC sys.sp_rename N'commerce.' + @table + N'.IsDeleted', N'IsDisabled', N'COLUMN';

                IF COL_LENGTH(N'commerce.' + @table, N'DeletedAt') IS NOT NULL
                    EXEC (N'ALTER TABLE commerce.' + QUOTENAME(@table) + N' DROP COLUMN DeletedAt');

                FETCH NEXT FROM tables_cursor INTO @table;
            END;
            CLOSE tables_cursor;
            DEALLOCATE tables_cursor;
            """);

        migrationBuilder.Sql("""
            CREATE UNIQUE INDEX [IX_PluginInstallations_PluginId]
                ON [commerce].[PluginInstallations] ([PluginId])
                WHERE [IsDisabled] = 0;
            CREATE UNIQUE INDEX [IX_PluginVersions_PluginId_Version]
                ON [commerce].[PluginVersions] ([PluginId], [Version])
                WHERE [IsDisabled] = 0;
            CREATE UNIQUE INDEX [IX_PluginDependencies_PluginId_DependencyPluginId]
                ON [commerce].[PluginDependencies] ([PluginId], [DependencyPluginId])
                WHERE [IsDisabled] = 0;
            CREATE UNIQUE INDEX [IX_PluginSettings_PluginId_Key]
                ON [commerce].[PluginSettings] ([PluginId], [Key])
                WHERE [IsDisabled] = 0;
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP INDEX IF EXISTS [IX_PluginInstallations_PluginId] ON [commerce].[PluginInstallations];
            DROP INDEX IF EXISTS [IX_PluginVersions_PluginId_Version] ON [commerce].[PluginVersions];
            DROP INDEX IF EXISTS [IX_PluginDependencies_PluginId_DependencyPluginId] ON [commerce].[PluginDependencies];
            DROP INDEX IF EXISTS [IX_PluginSettings_PluginId_Key] ON [commerce].[PluginSettings];

            DECLARE @table sysname;
            DECLARE tables_cursor CURSOR LOCAL FAST_FORWARD FOR
                SELECT value FROM (VALUES
                    (N'PluginInstallations'),
                    (N'PluginVersions'),
                    (N'PluginDependencies'),
                    (N'PluginSettings'),
                    (N'AspNetUsers'),
                    (N'AspNetRoles'),
                    (N'SiteSettings'),
                    (N'LocalizationResources')) AS tables(value);
            OPEN tables_cursor;
            FETCH NEXT FROM tables_cursor INTO @table;
            WHILE @@FETCH_STATUS = 0
            BEGIN
                IF COL_LENGTH(N'commerce.' + @table, N'IsDisabled') IS NOT NULL
                   AND COL_LENGTH(N'commerce.' + @table, N'IsDeleted') IS NULL
                    EXEC sys.sp_rename N'commerce.' + @table + N'.IsDisabled', N'IsDeleted', N'COLUMN';

                IF COL_LENGTH(N'commerce.' + @table, N'DeletedAt') IS NULL
                    EXEC (N'ALTER TABLE commerce.' + QUOTENAME(@table) + N' ADD DeletedAt datetimeoffset NULL');

                FETCH NEXT FROM tables_cursor INTO @table;
            END;
            CLOSE tables_cursor;
            DEALLOCATE tables_cursor;

            CREATE UNIQUE INDEX [IX_PluginInstallations_PluginId]
                ON [commerce].[PluginInstallations] ([PluginId])
                WHERE [IsDeleted] = 0;
            CREATE UNIQUE INDEX [IX_PluginVersions_PluginId_Version]
                ON [commerce].[PluginVersions] ([PluginId], [Version])
                WHERE [IsDeleted] = 0;
            CREATE UNIQUE INDEX [IX_PluginDependencies_PluginId_DependencyPluginId]
                ON [commerce].[PluginDependencies] ([PluginId], [DependencyPluginId])
                WHERE [IsDeleted] = 0;
            CREATE UNIQUE INDEX [IX_PluginSettings_PluginId_Key]
                ON [commerce].[PluginSettings] ([PluginId], [Key])
                WHERE [IsDeleted] = 0;
            """);
    }
}
