#nullable disable

namespace RemoteCommerce.Infrastructure.Persistence.Migrations;

/// <summary>Creates and upgrades the persistence schema required by plugin administration.</summary>
[DbContext(typeof(CommerceDbContext))]
[Migration("20260812160000_Stage04PluginAdministration")]
public sealed class Stage04PluginAdministration : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF SCHEMA_ID(N'commerce') IS NULL EXEC(N'CREATE SCHEMA commerce');
            IF OBJECT_ID(N'commerce.PluginInstallations', N'U') IS NULL
            BEGIN
                CREATE TABLE commerce.PluginInstallations
                (Id uniqueidentifier NOT NULL, PluginId nvarchar(200) NOT NULL, Version nvarchar(50) NOT NULL, PackagePath nvarchar(2048) NOT NULL, State int NOT NULL, DesiredState int NOT NULL, PackageHash nvarchar(64) NOT NULL, PendingVersion nvarchar(50) NULL, LastError nvarchar(4000) NULL, InstalledAt datetimeoffset NOT NULL, UpdatedAt datetimeoffset NOT NULL, CONSTRAINT PK_PluginInstallations PRIMARY KEY (Id));
                CREATE UNIQUE INDEX IX_PluginInstallations_PluginId ON commerce.PluginInstallations(PluginId);
            END
            ELSE
            BEGIN
                IF COL_LENGTH(N'commerce.PluginInstallations', N'PackagePath') IS NOT NULL ALTER TABLE commerce.PluginInstallations ALTER COLUMN PackagePath nvarchar(2048) NOT NULL;
                IF COL_LENGTH(N'commerce.PluginInstallations', N'DesiredState') IS NULL ALTER TABLE commerce.PluginInstallations ADD DesiredState int NOT NULL CONSTRAINT DF_PluginInstallations_DesiredState DEFAULT 0;
                IF COL_LENGTH(N'commerce.PluginInstallations', N'PackageHash') IS NULL ALTER TABLE commerce.PluginInstallations ADD PackageHash nvarchar(64) NOT NULL CONSTRAINT DF_PluginInstallations_PackageHash DEFAULT N'';
                IF COL_LENGTH(N'commerce.PluginInstallations', N'PendingVersion') IS NULL ALTER TABLE commerce.PluginInstallations ADD PendingVersion nvarchar(50) NULL;
                IF COL_LENGTH(N'commerce.PluginInstallations', N'LastError') IS NULL ALTER TABLE commerce.PluginInstallations ADD LastError nvarchar(4000) NULL;
                IF COL_LENGTH(N'commerce.PluginInstallations', N'UpdatedAt') IS NULL ALTER TABLE commerce.PluginInstallations ADD UpdatedAt datetimeoffset NOT NULL CONSTRAINT DF_PluginInstallations_UpdatedAt DEFAULT SYSUTCDATETIME();
                UPDATE commerce.PluginInstallations SET State = CASE State WHEN 0 THEN 2 WHEN 1 THEN 4 ELSE State END;
                UPDATE commerce.PluginInstallations SET DesiredState = CASE State WHEN 4 THEN 1 ELSE 0 END;
            END;
            IF OBJECT_ID(N'commerce.PluginVersions', N'U') IS NULL
            BEGIN
                CREATE TABLE commerce.PluginVersions
                (Id uniqueidentifier NOT NULL, PluginId nvarchar(200) NOT NULL, Version nvarchar(50) NOT NULL, PackagePath nvarchar(2048) NOT NULL, PackageHash nvarchar(64) NOT NULL, InstalledAt datetimeoffset NOT NULL, IsCurrent bit NOT NULL, CONSTRAINT PK_PluginVersions PRIMARY KEY (Id));
                CREATE UNIQUE INDEX IX_PluginVersions_PluginId_Version ON commerce.PluginVersions(PluginId, Version);
            END;
            IF OBJECT_ID(N'commerce.PluginDependencies', N'U') IS NULL
            BEGIN
                CREATE TABLE commerce.PluginDependencies
                (Id uniqueidentifier NOT NULL, PluginId nvarchar(200) NOT NULL, DependencyPluginId nvarchar(200) NOT NULL, MinimumVersion nvarchar(50) NOT NULL, MaximumVersion nvarchar(50) NULL, CONSTRAINT PK_PluginDependencies PRIMARY KEY (Id));
                CREATE UNIQUE INDEX IX_PluginDependencies_PluginId_DependencyPluginId ON commerce.PluginDependencies(PluginId, DependencyPluginId);
            END;
            IF OBJECT_ID(N'commerce.PluginLifecycleErrors', N'U') IS NULL
            BEGIN
                CREATE TABLE commerce.PluginLifecycleErrors
                (Id uniqueidentifier NOT NULL, PluginId nvarchar(200) NOT NULL, Operation nvarchar(100) NOT NULL, Category nvarchar(100) NOT NULL, Message nvarchar(4000) NOT NULL, ExceptionType nvarchar(500) NULL, CreatedAt datetimeoffset NOT NULL, CONSTRAINT PK_PluginLifecycleErrors PRIMARY KEY (Id));
                CREATE INDEX IX_PluginLifecycleErrors_PluginId_CreatedAt ON commerce.PluginLifecycleErrors(PluginId, CreatedAt);
            END;
            IF OBJECT_ID(N'commerce.PluginSettings', N'U') IS NULL
            BEGIN
                CREATE TABLE commerce.PluginSettings
                (Id uniqueidentifier NOT NULL, PluginId nvarchar(200) NOT NULL, [Key] nvarchar(200) NOT NULL, [Value] nvarchar(max) NOT NULL, Metadata nvarchar(max) NULL, CONSTRAINT PK_PluginSettings PRIMARY KEY (Id));
                CREATE UNIQUE INDEX IX_PluginSettings_PluginId_Key ON commerce.PluginSettings(PluginId, [Key]);
            END;
            INSERT INTO commerce.PluginVersions (Id, PluginId, Version, PackagePath, PackageHash, InstalledAt, IsCurrent)
            SELECT NEWID(), PluginId, Version, PackagePath, PackageHash, InstalledAt, 1 FROM commerce.PluginInstallations installation
            WHERE NOT EXISTS (SELECT 1 FROM commerce.PluginVersions version WHERE version.PluginId = installation.PluginId AND version.Version = installation.Version);
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF OBJECT_ID(N'commerce.PluginSettings', N'U') IS NOT NULL DROP TABLE commerce.PluginSettings;
            IF OBJECT_ID(N'commerce.PluginLifecycleErrors', N'U') IS NOT NULL DROP TABLE commerce.PluginLifecycleErrors;
            IF OBJECT_ID(N'commerce.PluginDependencies', N'U') IS NOT NULL DROP TABLE commerce.PluginDependencies;
            IF OBJECT_ID(N'commerce.PluginVersions', N'U') IS NOT NULL DROP TABLE commerce.PluginVersions;
            IF OBJECT_ID(N'commerce.PluginInstallations', N'U') IS NOT NULL
            BEGIN
                IF COL_LENGTH(N'commerce.PluginInstallations', N'UpdatedAt') IS NOT NULL ALTER TABLE commerce.PluginInstallations DROP CONSTRAINT IF EXISTS DF_PluginInstallations_UpdatedAt;
                IF COL_LENGTH(N'commerce.PluginInstallations', N'LastError') IS NOT NULL ALTER TABLE commerce.PluginInstallations DROP COLUMN LastError;
                IF COL_LENGTH(N'commerce.PluginInstallations', N'PendingVersion') IS NOT NULL ALTER TABLE commerce.PluginInstallations DROP COLUMN PendingVersion;
                IF COL_LENGTH(N'commerce.PluginInstallations', N'PackageHash') IS NOT NULL ALTER TABLE commerce.PluginInstallations DROP CONSTRAINT IF EXISTS DF_PluginInstallations_PackageHash;
                IF COL_LENGTH(N'commerce.PluginInstallations', N'PackageHash') IS NOT NULL ALTER TABLE commerce.PluginInstallations DROP COLUMN PackageHash;
                IF COL_LENGTH(N'commerce.PluginInstallations', N'DesiredState') IS NOT NULL ALTER TABLE commerce.PluginInstallations DROP CONSTRAINT IF EXISTS DF_PluginInstallations_DesiredState;
                IF COL_LENGTH(N'commerce.PluginInstallations', N'DesiredState') IS NOT NULL ALTER TABLE commerce.PluginInstallations DROP COLUMN DesiredState;
                IF COL_LENGTH(N'commerce.PluginInstallations', N'UpdatedAt') IS NOT NULL ALTER TABLE commerce.PluginInstallations DROP COLUMN UpdatedAt;
            END;
            """);
    }

    /// <inheritdoc />
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
        // The Stage 04 migration uses idempotent SQL so it can upgrade the Stage 03 installation table.
    }
}
