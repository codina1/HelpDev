using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpDev.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddToolLibraryV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tool_metadata",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    content_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tool_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    official_website_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    github_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    logo_media_id = table.Column<Guid>(type: "uuid", nullable: true),
                    company_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    pricing_model = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    tool_category = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    platform_support = table.Column<int>(type: "integer", nullable: false),
                    license_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tool_metadata", x => x.id);
                    table.ForeignKey(
                        name: "fk_tool_metadata_contents_content_id",
                        column: x => x.content_id,
                        principalTable: "contents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tool_alternatives",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tool_id = table.Column<Guid>(type: "uuid", nullable: false),
                    alternative_tool_content_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tool_alternatives", x => x.id);
                    table.ForeignKey(
                        name: "FK_tool_alternatives_tool_metadata_tool_id",
                        column: x => x.tool_id,
                        principalTable: "tool_metadata",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tool_features",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tool_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tool_features", x => x.id);
                    table.ForeignKey(
                        name: "FK_tool_features_tool_metadata_tool_id",
                        column: x => x.tool_id,
                        principalTable: "tool_metadata",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_tool_alternatives_tool_id_alternative_content_id",
                table: "tool_alternatives",
                columns: new[] { "tool_id", "alternative_tool_content_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tool_alternatives_tool_id_sort_order",
                table: "tool_alternatives",
                columns: new[] { "tool_id", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "ix_tool_features_tool_id_sort_order",
                table: "tool_features",
                columns: new[] { "tool_id", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "ix_tool_metadata_content_id",
                table: "tool_metadata",
                column: "content_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tool_metadata_tool_name",
                table: "tool_metadata",
                column: "tool_name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tool_alternatives");

            migrationBuilder.DropTable(
                name: "tool_features");

            migrationBuilder.DropTable(
                name: "tool_metadata");
        }
    }
}
