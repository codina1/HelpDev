using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpDev.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAdministrationModuleV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "administration_announcements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    body = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    starts_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ends_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    published_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_administration_announcements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "administration_feature_flags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_administration_feature_flags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "administration_system_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    value = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    value_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_public = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_administration_system_settings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_administration_announcements_ends_at_utc",
                table: "administration_announcements",
                column: "ends_at_utc");

            migrationBuilder.CreateIndex(
                name: "ix_administration_announcements_starts_at_utc",
                table: "administration_announcements",
                column: "starts_at_utc");

            migrationBuilder.CreateIndex(
                name: "ix_administration_announcements_status",
                table: "administration_announcements",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_administration_announcements_type",
                table: "administration_announcements",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "ix_administration_announcements_updated_at_utc",
                table: "administration_announcements",
                column: "updated_at_utc");

            migrationBuilder.CreateIndex(
                name: "ix_administration_feature_flags_is_enabled",
                table: "administration_feature_flags",
                column: "is_enabled");

            migrationBuilder.CreateIndex(
                name: "ux_administration_feature_flags_key",
                table: "administration_feature_flags",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_administration_system_settings_is_public",
                table: "administration_system_settings",
                column: "is_public");

            migrationBuilder.CreateIndex(
                name: "ux_administration_system_settings_key",
                table: "administration_system_settings",
                column: "key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "administration_announcements");

            migrationBuilder.DropTable(
                name: "administration_feature_flags");

            migrationBuilder.DropTable(
                name: "administration_system_settings");
        }
    }
}
