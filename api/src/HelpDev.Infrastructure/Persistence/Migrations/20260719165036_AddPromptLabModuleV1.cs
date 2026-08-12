using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpDev.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPromptLabModuleV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "promptlab_categories",
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
                    table.PrimaryKey("PK_promptlab_categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "promptlab_prompts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    summary = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    description = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: true),
                    purpose = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    visibility = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    is_published = table.Column<bool>(type: "boolean", nullable: false),
                    requires_authentication = table.Column<bool>(type: "boolean", nullable: false),
                    allow_history = table.Column<bool>(type: "boolean", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    latest_version_number = table.Column<int>(type: "integer", nullable: false),
                    published_version_number = table.Column<int>(type: "integer", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    published_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promptlab_prompts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_promptlab_prompts_promptlab_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "promptlab_categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "promptlab_favorites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    prompt_definition_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promptlab_favorites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_promptlab_favorites_promptlab_prompts_prompt_definition_id",
                        column: x => x.prompt_definition_id,
                        principalTable: "promptlab_prompts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "promptlab_versions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    prompt_definition_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version_number = table.Column<int>(type: "integer", nullable: false),
                    template = table.Column<string>(type: "character varying(20000)", maxLength: 20000, nullable: false),
                    change_notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promptlab_versions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_promptlab_versions_promptlab_prompts_prompt_definition_id",
                        column: x => x.prompt_definition_id,
                        principalTable: "promptlab_prompts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "promptlab_render_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    prompt_definition_id = table.Column<Guid>(type: "uuid", nullable: false),
                    prompt_version_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version_number = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    succeeded = table.Column<bool>(type: "boolean", nullable: false),
                    duration_milliseconds = table.Column<int>(type: "integer", nullable: false),
                    input_preview = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    rendered_preview = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    error_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    rendered_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promptlab_render_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_promptlab_render_records_promptlab_prompts_prompt_definitio~",
                        column: x => x.prompt_definition_id,
                        principalTable: "promptlab_prompts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_promptlab_render_records_promptlab_versions_prompt_version_~",
                        column: x => x.prompt_version_id,
                        principalTable: "promptlab_versions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "promptlab_variables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    prompt_version_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    label = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    is_required = table.Column<bool>(type: "boolean", nullable: false),
                    default_value = table.Column<string>(type: "character varying(20000)", maxLength: 20000, nullable: true),
                    min_length = table.Column<int>(type: "integer", nullable: true),
                    max_length = table.Column<int>(type: "integer", nullable: true),
                    min_value = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    max_value = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    validation_pattern = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    allowed_values_json = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promptlab_variables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_promptlab_variables_promptlab_versions_prompt_version_id",
                        column: x => x.prompt_version_id,
                        principalTable: "promptlab_versions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_promptlab_categories_display_order",
                table: "promptlab_categories",
                column: "display_order");

            migrationBuilder.CreateIndex(
                name: "ix_promptlab_categories_is_active",
                table: "promptlab_categories",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ux_promptlab_categories_slug",
                table: "promptlab_categories",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_promptlab_favorites_prompt_definition_id",
                table: "promptlab_favorites",
                column: "prompt_definition_id");

            migrationBuilder.CreateIndex(
                name: "ix_promptlab_favorites_user_id",
                table: "promptlab_favorites",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ux_promptlab_favorites_user_id_prompt_definition_id",
                table: "promptlab_favorites",
                columns: new[] { "user_id", "prompt_definition_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_promptlab_prompts_category_id",
                table: "promptlab_prompts",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_promptlab_prompts_display_order",
                table: "promptlab_prompts",
                column: "display_order");

            migrationBuilder.CreateIndex(
                name: "ix_promptlab_prompts_is_enabled",
                table: "promptlab_prompts",
                column: "is_enabled");

            migrationBuilder.CreateIndex(
                name: "ix_promptlab_prompts_is_published",
                table: "promptlab_prompts",
                column: "is_published");

            migrationBuilder.CreateIndex(
                name: "ix_promptlab_prompts_published_version_number",
                table: "promptlab_prompts",
                column: "published_version_number");

            migrationBuilder.CreateIndex(
                name: "ix_promptlab_prompts_purpose",
                table: "promptlab_prompts",
                column: "purpose");

            migrationBuilder.CreateIndex(
                name: "ix_promptlab_prompts_visibility",
                table: "promptlab_prompts",
                column: "visibility");

            migrationBuilder.CreateIndex(
                name: "ux_promptlab_prompts_slug",
                table: "promptlab_prompts",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_promptlab_render_records_prompt_definition_id",
                table: "promptlab_render_records",
                column: "prompt_definition_id");

            migrationBuilder.CreateIndex(
                name: "ix_promptlab_render_records_prompt_version_id",
                table: "promptlab_render_records",
                column: "prompt_version_id");

            migrationBuilder.CreateIndex(
                name: "ix_promptlab_render_records_rendered_at_utc",
                table: "promptlab_render_records",
                column: "rendered_at_utc");

            migrationBuilder.CreateIndex(
                name: "ix_promptlab_render_records_succeeded",
                table: "promptlab_render_records",
                column: "succeeded");

            migrationBuilder.CreateIndex(
                name: "ix_promptlab_render_records_user_id",
                table: "promptlab_render_records",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_promptlab_variables_display_order",
                table: "promptlab_variables",
                column: "display_order");

            migrationBuilder.CreateIndex(
                name: "ix_promptlab_variables_prompt_version_id",
                table: "promptlab_variables",
                column: "prompt_version_id");

            migrationBuilder.CreateIndex(
                name: "ux_promptlab_variables_prompt_version_id_name",
                table: "promptlab_variables",
                columns: new[] { "prompt_version_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_promptlab_versions_created_at_utc",
                table: "promptlab_versions",
                column: "created_at_utc");

            migrationBuilder.CreateIndex(
                name: "ix_promptlab_versions_prompt_definition_id",
                table: "promptlab_versions",
                column: "prompt_definition_id");

            migrationBuilder.CreateIndex(
                name: "ux_promptlab_versions_prompt_definition_id_version_number",
                table: "promptlab_versions",
                columns: new[] { "prompt_definition_id", "version_number" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "promptlab_favorites");

            migrationBuilder.DropTable(
                name: "promptlab_render_records");

            migrationBuilder.DropTable(
                name: "promptlab_variables");

            migrationBuilder.DropTable(
                name: "promptlab_versions");

            migrationBuilder.DropTable(
                name: "promptlab_prompts");

            migrationBuilder.DropTable(
                name: "promptlab_categories");
        }
    }
}
