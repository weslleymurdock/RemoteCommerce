namespace RemoteCommerce.Infrastructure.Persistence.Migrations;

/// <summary>Adds the Stage 05 soft-delete and serialized operation-history persistence contract.</summary>
[Migration("20260812223000_Stage05PipelinePersistence")]
public partial class Stage05PipelinePersistence : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        foreach (var table in new[] { "PluginInstallations", "PluginVersions", "PluginDependencies", "PluginSettings" })
        {
            migrationBuilder.AddColumn<bool>(table: table, name: "IsDeleted", schema: "commerce", type: "bit", nullable: false, defaultValue: false);
            migrationBuilder.AddColumn<DateTimeOffset>(table: table, name: "DeletedAt", schema: "commerce", type: "datetimeoffset", nullable: true);
        }

        foreach (var table in new[] { "AspNetUsers", "AspNetRoles" })
        {
            migrationBuilder.AddColumn<bool>(table: table, name: "IsDeleted", schema: "commerce", type: "bit", nullable: false, defaultValue: false);
            migrationBuilder.AddColumn<DateTimeOffset>(table: table, name: "DeletedAt", schema: "commerce", type: "datetimeoffset", nullable: true);
        }

        migrationBuilder.AddColumn<bool>(table: "SiteSettings", name: "IsDeleted", schema: "commerce", type: "bit", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<DateTimeOffset>(table: "SiteSettings", name: "DeletedAt", schema: "commerce", type: "datetimeoffset", nullable: true);
        migrationBuilder.AddColumn<bool>(table: "LocalizationResources", name: "IsDeleted", schema: "commerce", type: "bit", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<DateTimeOffset>(table: "LocalizationResources", name: "DeletedAt", schema: "commerce", type: "datetimeoffset", nullable: true);

        migrationBuilder.CreateTable(
            name: "OperationHistories",
            schema: "commerce",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                EntityType = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                EntityId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                OperationType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                OccurredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Actor = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                CorrelationId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                IpAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                PreviousState = table.Column<string>(type: "nvarchar(max)", nullable: false),
                NewState = table.Column<string>(type: "nvarchar(max)", nullable: true),
            },
            constraints: table => table.PrimaryKey("PK_OperationHistories", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_OperationHistories_EntityType_EntityId_OccurredAt",
            schema: "commerce",
            table: "OperationHistories",
            columns: new[] { "EntityType", "EntityId", "OccurredAt" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "OperationHistories", schema: "commerce");

        migrationBuilder.DropColumn(name: "IsDeleted", table: "LocalizationResources", schema: "commerce");
        migrationBuilder.DropColumn(name: "DeletedAt", table: "LocalizationResources", schema: "commerce");
        migrationBuilder.DropColumn(name: "IsDeleted", table: "SiteSettings", schema: "commerce");
        migrationBuilder.DropColumn(name: "DeletedAt", table: "SiteSettings", schema: "commerce");

        foreach (var table in new[] { "AspNetUsers", "AspNetRoles", "PluginInstallations", "PluginVersions", "PluginDependencies", "PluginSettings" })
        {
            migrationBuilder.DropColumn(name: "IsDeleted", table: table, schema: "commerce");
            migrationBuilder.DropColumn(name: "DeletedAt", table: table, schema: "commerce");
        }
    }
}
