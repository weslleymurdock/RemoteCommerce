using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RemoteCommerce.Infrastructure.Persistence.Migrations;

/// <summary>Creates the Stage 05 site, identity, audit, and localization persistence.</summary>
public partial class Stage05SiteIdentityConfiguration : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AspNetRoles",
            schema: "commerce",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
            },
            constraints: table => table.PrimaryKey("PK_AspNetRoles", x => x.Id));

        migrationBuilder.CreateTable(
            name: "AspNetUsers",
            schema: "commerce",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
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
                AccessFailedCount = table.Column<int>(type: "int", nullable: false),
            },
            constraints: table => table.PrimaryKey("PK_AspNetUsers", x => x.Id));

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
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
            },
            constraints: table => table.PrimaryKey("PK_AuditLogs", x => x.Id));

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
            },
            constraints: table => table.PrimaryKey("PK_LocalizationResources", x => x.Id));

        migrationBuilder.CreateTable(
            name: "SiteSettings",
            schema: "commerce",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false),
                SiteName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                SiteDescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                PublicUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                TimeZone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Culture = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                Locale = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
            },
            constraints: table => table.PrimaryKey("PK_SiteSettings", x => x.Id));

        migrationBuilder.CreateTable(
            name: "AspNetRoleClaims",
            schema: "commerce",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                table.ForeignKey("FK_AspNetRoleClaims_AspNetRoles_RoleId", x => x.RoleId, "AspNetRoles", "Id", principalSchema: "commerce", onDelete: ReferentialAction.Cascade);
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
                ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                table.ForeignKey("FK_AspNetUserClaims_AspNetUsers_UserId", x => x.UserId, "AspNetUsers", "Id", principalSchema: "commerce", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserLogins",
            schema: "commerce",
            columns: table => new
            {
                LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                ProviderKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                table.ForeignKey("FK_AspNetUserLogins_AspNetUsers_UserId", x => x.UserId, "AspNetUsers", "Id", principalSchema: "commerce", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserRoles",
            schema: "commerce",
            columns: table => new
            {
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                table.ForeignKey("FK_AspNetUserRoles_AspNetRoles_RoleId", x => x.RoleId, "AspNetRoles", "Id", principalSchema: "commerce", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_AspNetUserRoles_AspNetUsers_UserId", x => x.UserId, "AspNetUsers", "Id", principalSchema: "commerce", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserTokens",
            schema: "commerce",
            columns: table => new
            {
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                table.ForeignKey("FK_AspNetUserTokens_AspNetUsers_UserId", x => x.UserId, "AspNetUsers", "Id", principalSchema: "commerce", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex("IX_AspNetRoleClaims_RoleId", "AspNetRoleClaims", "RoleId", "commerce");
        migrationBuilder.CreateIndex("RoleNameIndex", "AspNetRoles", "NormalizedName", "commerce", unique: true, filter: "[NormalizedName] IS NOT NULL");
        migrationBuilder.CreateIndex("IX_AspNetUserClaims_UserId", "AspNetUserClaims", "UserId", "commerce");
        migrationBuilder.CreateIndex("IX_AspNetUserLogins_UserId", "AspNetUserLogins", "UserId", "commerce");
        migrationBuilder.CreateIndex("IX_AspNetUserRoles_RoleId", "AspNetUserRoles", "RoleId", "commerce");
        migrationBuilder.CreateIndex("EmailIndex", "AspNetUsers", "NormalizedEmail", "commerce");
        migrationBuilder.CreateIndex("UserNameIndex", "AspNetUsers", "NormalizedUserName", "commerce", unique: true, filter: "[NormalizedUserName] IS NOT NULL");
        migrationBuilder.CreateIndex("IX_AuditLogs_CreatedAt", "AuditLogs", "CreatedAt", "commerce");
        migrationBuilder.CreateIndex("IX_AuditLogs_UserId", "AuditLogs", "UserId", "commerce");
        migrationBuilder.CreateIndex("IX_LocalizationResources_Culture_ResourceType_IsActive", "LocalizationResources", new[] { "Culture", "ResourceType", "IsActive" }, "commerce");
        migrationBuilder.CreateIndex("IX_LocalizationResources_Culture_ResourceType_Version", "LocalizationResources", new[] { "Culture", "ResourceType", "Version" }, "commerce", unique: true);

        migrationBuilder.InsertData(
            table: "SiteSettings",
            schema: "commerce",
            columns: new[] { "Id", "SiteName", "SiteDescription", "PublicUrl", "TimeZone", "Culture", "Locale", "UpdatedAt" },
            values: new object[] { 1, "RemoteCommerce", "", "https://localhost", "UTC", "en-US", "en-US", DateTime.UtcNow });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "AspNetRoleClaims", schema: "commerce");
        migrationBuilder.DropTable(name: "AspNetUserClaims", schema: "commerce");
        migrationBuilder.DropTable(name: "AspNetUserLogins", schema: "commerce");
        migrationBuilder.DropTable(name: "AspNetUserRoles", schema: "commerce");
        migrationBuilder.DropTable(name: "AspNetUserTokens", schema: "commerce");
        migrationBuilder.DropTable(name: "AuditLogs", schema: "commerce");
        migrationBuilder.DropTable(name: "LocalizationResources", schema: "commerce");
        migrationBuilder.DropTable(name: "SiteSettings", schema: "commerce");
        migrationBuilder.DropTable(name: "AspNetRoles", schema: "commerce");
        migrationBuilder.DropTable(name: "AspNetUsers", schema: "commerce");
    }
}
