using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpDev.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPromptLabLibraryPersistenceV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "promptlab_ai_models",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    provider = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    logo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promptlab_ai_models", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "promptlab_packs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    description = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: true),
                    cover_image = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    author_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, defaultValue: "Draft"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    published_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promptlab_packs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "promptlab_library_prompts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    description = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: true),
                    content = table.Column<string>(type: "character varying(20000)", maxLength: 20000, nullable: false),
                    cover_image = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    media_type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ai_model_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, defaultValue: "Draft"),
                    author_id = table.Column<Guid>(type: "uuid", nullable: false),
                    views = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    copy_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    published_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promptlab_library_prompts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_promptlab_library_prompts_promptlab_ai_models_ai_model_id",
                        column: x => x.ai_model_id,
                        principalTable: "promptlab_ai_models",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_promptlab_library_prompts_promptlab_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "promptlab_categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "promptlab_pack_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    pack_id = table.Column<Guid>(type: "uuid", nullable: false),
                    prompt_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promptlab_pack_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_promptlab_pack_items_promptlab_library_prompts_prompt_id",
                        column: x => x.prompt_id,
                        principalTable: "promptlab_library_prompts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_promptlab_pack_items_promptlab_packs_pack_id",
                        column: x => x.pack_id,
                        principalTable: "promptlab_packs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_promptlab_ai_models_is_active",
                table: "promptlab_ai_models",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_promptlab_ai_models_provider",
                table: "promptlab_ai_models",
                column: "provider");

            migrationBuilder.CreateIndex(
                name: "ux_promptlab_ai_models_slug",
                table: "promptlab_ai_models",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_promptlab_library_prompts_ai_model_id",
                table: "promptlab_library_prompts",
                column: "ai_model_id");

            migrationBuilder.CreateIndex(
                name: "ix_promptlab_library_prompts_category_id",
                table: "promptlab_library_prompts",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_promptlab_library_prompts_status",
                table: "promptlab_library_prompts",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ux_promptlab_library_prompts_slug",
                table: "promptlab_library_prompts",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_promptlab_pack_items_prompt_id",
                table: "promptlab_pack_items",
                column: "prompt_id");

            migrationBuilder.CreateIndex(
                name: "ux_promptlab_pack_items_pack_id_item_order",
                table: "promptlab_pack_items",
                columns: new[] { "pack_id", "item_order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_promptlab_pack_items_pack_id_prompt_id",
                table: "promptlab_pack_items",
                columns: new[] { "pack_id", "prompt_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_promptlab_packs_author_id",
                table: "promptlab_packs",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "ix_promptlab_packs_status",
                table: "promptlab_packs",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ux_promptlab_packs_slug",
                table: "promptlab_packs",
                column: "slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "promptlab_pack_items");

            migrationBuilder.DropTable(
                name: "promptlab_library_prompts");

            migrationBuilder.DropTable(
                name: "promptlab_packs");

            migrationBuilder.DropTable(
                name: "promptlab_ai_models");
        }
    }
}
