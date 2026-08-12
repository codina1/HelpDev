using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpDev.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRoadmapEngineV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "roadmap_metadata",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    content_id = table.Column<Guid>(type: "uuid", nullable: false),
                    level = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    estimated_duration = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    goal = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    prerequisites = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roadmap_metadata", x => x.id);
                    table.ForeignKey(
                        name: "fk_roadmap_metadata_contents_content_id",
                        column: x => x.content_id,
                        principalTable: "contents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "roadmap_steps",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    roadmap_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    estimated_hours = table.Column<int>(type: "integer", nullable: false),
                    project_title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    project_description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roadmap_steps", x => x.id);
                    table.ForeignKey(
                        name: "FK_roadmap_steps_roadmap_metadata_roadmap_id",
                        column: x => x.roadmap_id,
                        principalTable: "roadmap_metadata",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "roadmap_resources",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    step_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    resource_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roadmap_resources", x => x.id);
                    table.ForeignKey(
                        name: "FK_roadmap_resources_roadmap_steps_step_id",
                        column: x => x.step_id,
                        principalTable: "roadmap_steps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "roadmap_topics",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    step_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roadmap_topics", x => x.id);
                    table.ForeignKey(
                        name: "FK_roadmap_topics_roadmap_steps_step_id",
                        column: x => x.step_id,
                        principalTable: "roadmap_steps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_roadmap_metadata_content_id",
                table: "roadmap_metadata",
                column: "content_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_roadmap_resources_step_id",
                table: "roadmap_resources",
                column: "step_id");

            migrationBuilder.CreateIndex(
                name: "ix_roadmap_resources_step_id_sort_order",
                table: "roadmap_resources",
                columns: new[] { "step_id", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "ix_roadmap_steps_roadmap_id",
                table: "roadmap_steps",
                column: "roadmap_id");

            migrationBuilder.CreateIndex(
                name: "ix_roadmap_steps_roadmap_id_sort_order",
                table: "roadmap_steps",
                columns: new[] { "roadmap_id", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "ix_roadmap_topics_step_id",
                table: "roadmap_topics",
                column: "step_id");

            migrationBuilder.CreateIndex(
                name: "ix_roadmap_topics_step_id_sort_order",
                table: "roadmap_topics",
                columns: new[] { "step_id", "sort_order" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "roadmap_resources");

            migrationBuilder.DropTable(
                name: "roadmap_topics");

            migrationBuilder.DropTable(
                name: "roadmap_steps");

            migrationBuilder.DropTable(
                name: "roadmap_metadata");
        }
    }
}
