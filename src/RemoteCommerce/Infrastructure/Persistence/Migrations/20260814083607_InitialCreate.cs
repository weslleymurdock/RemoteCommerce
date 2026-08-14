using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RemoteCommerce.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "commerce");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                schema: "commerce",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsDisabled = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                schema: "commerce",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsDisabled = table.Column<bool>(type: "bit", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                schema: "commerce",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Actor = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Operation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Resource = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Result = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Context = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LocalizationResources",
                schema: "commerce",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Culture = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ResourceType = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ContentHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ImportedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ImportedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDisabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalizationResources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OperationHistories",
                schema: "commerce",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityType = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    OperationType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Actor = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CorrelationId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    PreviousState = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NewState = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PluginDependencies",
                schema: "commerce",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PluginId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DependencyPluginId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MinimumVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaximumVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsDisabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PluginDependencies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PluginInstallations",
                schema: "commerce",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PluginId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PackagePath = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    State = table.Column<int>(type: "int", nullable: false),
                    DesiredState = table.Column<int>(type: "int", nullable: false),
                    PackageHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    PendingVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastError = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    InstalledAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsDisabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PluginInstallations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PluginLifecycleErrors",
                schema: "commerce",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PluginId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Operation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    ExceptionType = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PluginLifecycleErrors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PluginSettings",
                schema: "commerce",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PluginId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Key = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDisabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PluginSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PluginVersions",
                schema: "commerce",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PluginId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PackagePath = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    PackageHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    InstalledAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    IsDisabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PluginVersions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SiteSettings",
                schema: "commerce",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SiteName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SiteDescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    PublicUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    TimeZone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Culture = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Locale = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDisabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                schema: "commerce",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "commerce",
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                schema: "commerce",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "commerce",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                schema: "commerce",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "commerce",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                schema: "commerce",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "commerce",
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "commerce",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                schema: "commerce",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "commerce",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                schema: "commerce",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "commerce",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                schema: "commerce",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                schema: "commerce",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                schema: "commerce",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "commerce",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "commerce",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CreatedAt",
                schema: "commerce",
                table: "AuditLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                schema: "commerce",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalizationResources_Culture_ResourceType_IsActive",
                schema: "commerce",
                table: "LocalizationResources",
                columns: new[] { "Culture", "ResourceType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_LocalizationResources_Culture_ResourceType_Version",
                schema: "commerce",
                table: "LocalizationResources",
                columns: new[] { "Culture", "ResourceType", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OperationHistories_EntityType_EntityId_OccurredAt",
                schema: "commerce",
                table: "OperationHistories",
                columns: new[] { "EntityType", "EntityId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PluginDependencies_PluginId_DependencyPluginId",
                schema: "commerce",
                table: "PluginDependencies",
                columns: new[] { "PluginId", "DependencyPluginId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PluginInstallations_PluginId",
                schema: "commerce",
                table: "PluginInstallations",
                column: "PluginId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PluginLifecycleErrors_PluginId_CreatedAt",
                schema: "commerce",
                table: "PluginLifecycleErrors",
                columns: new[] { "PluginId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PluginSettings_PluginId_Key",
                schema: "commerce",
                table: "PluginSettings",
                columns: new[] { "PluginId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PluginVersions_PluginId_Version",
                schema: "commerce",
                table: "PluginVersions",
                columns: new[] { "PluginId", "Version" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims",
                schema: "commerce");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims",
                schema: "commerce");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins",
                schema: "commerce");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles",
                schema: "commerce");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens",
                schema: "commerce");

            migrationBuilder.DropTable(
                name: "AuditLogs",
                schema: "commerce");

            migrationBuilder.DropTable(
                name: "LocalizationResources",
                schema: "commerce");

            migrationBuilder.DropTable(
                name: "OperationHistories",
                schema: "commerce");

            migrationBuilder.DropTable(
                name: "PluginDependencies",
                schema: "commerce");

            migrationBuilder.DropTable(
                name: "PluginInstallations",
                schema: "commerce");

            migrationBuilder.DropTable(
                name: "PluginLifecycleErrors",
                schema: "commerce");

            migrationBuilder.DropTable(
                name: "PluginSettings",
                schema: "commerce");

            migrationBuilder.DropTable(
                name: "PluginVersions",
                schema: "commerce");

            migrationBuilder.DropTable(
                name: "SiteSettings",
                schema: "commerce");

            migrationBuilder.DropTable(
                name: "AspNetRoles",
                schema: "commerce");

            migrationBuilder.DropTable(
                name: "AspNetUsers",
                schema: "commerce");
        }
    }
}
