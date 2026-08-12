using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpDev.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddToolboxModuleV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "toolbox_categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    icon = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_toolbox_categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "toolbox_tools",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    summary = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    description = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: true),
                    type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    input_schema = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    example_input = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    is_published = table.Column<bool>(type: "boolean", nullable: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    requires_authentication = table.Column<bool>(type: "boolean", nullable: false),
                    allow_history = table.Column<bool>(type: "boolean", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    published_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_toolbox_tools", x => x.Id);
                    table.ForeignKey(
                        name: "FK_toolbox_tools_toolbox_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "toolbox_categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "toolbox_execution_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    tool_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tool_type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    succeeded = table.Column<bool>(type: "boolean", nullable: false),
                    duration_milliseconds = table.Column<int>(type: "integer", nullable: false),
                    input_preview = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    output_preview = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    error_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    executed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_toolbox_execution_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_toolbox_execution_records_toolbox_tools_tool_id",
                        column: x => x.tool_id,
                        principalTable: "toolbox_tools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "toolbox_favorites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tool_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_toolbox_favorites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_toolbox_favorites_toolbox_tools_tool_id",
                        column: x => x.tool_id,
                        principalTable: "toolbox_tools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_toolbox_categories_display_order",
                table: "toolbox_categories",
                column: "display_order");

            migrationBuilder.CreateIndex(
                name: "ix_toolbox_categories_is_active",
                table: "toolbox_categories",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ux_toolbox_categories_slug",
                table: "toolbox_categories",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_toolbox_execution_records_executed_at_utc",
                table: "toolbox_execution_records",
                column: "executed_at_utc");

            migrationBuilder.CreateIndex(
                name: "ix_toolbox_execution_records_succeeded",
                table: "toolbox_execution_records",
                column: "succeeded");

            migrationBuilder.CreateIndex(
                name: "ix_toolbox_execution_records_tool_id",
                table: "toolbox_execution_records",
                column: "tool_id");

            migrationBuilder.CreateIndex(
                name: "ix_toolbox_execution_records_user_id",
                table: "toolbox_execution_records",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_toolbox_favorites_tool_id",
                table: "toolbox_favorites",
                column: "tool_id");

            migrationBuilder.CreateIndex(
                name: "ix_toolbox_favorites_user_id",
                table: "toolbox_favorites",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ux_toolbox_favorites_user_id_tool_id",
                table: "toolbox_favorites",
                columns: new[] { "user_id", "tool_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_toolbox_tools_category_id",
                table: "toolbox_tools",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_toolbox_tools_display_order",
                table: "toolbox_tools",
                column: "display_order");

            migrationBuilder.CreateIndex(
                name: "ix_toolbox_tools_is_enabled",
                table: "toolbox_tools",
                column: "is_enabled");

            migrationBuilder.CreateIndex(
                name: "ix_toolbox_tools_is_published",
                table: "toolbox_tools",
                column: "is_published");

            migrationBuilder.CreateIndex(
                name: "ix_toolbox_tools_type",
                table: "toolbox_tools",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "ux_toolbox_tools_slug",
                table: "toolbox_tools",
                column: "slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "toolbox_execution_records");

            migrationBuilder.DropTable(
                name: "toolbox_favorites");

            migrationBuilder.DropTable(
                name: "toolbox_tools");

            migrationBuilder.DropTable(
                name: "toolbox_categories");
        }
    }
}
