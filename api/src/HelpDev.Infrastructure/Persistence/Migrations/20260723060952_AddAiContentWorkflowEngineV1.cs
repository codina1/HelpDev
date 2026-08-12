using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpDev.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAiContentWorkflowEngineV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ai_content_workflow_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    idea_id = table.Column<Guid>(type: "uuid", nullable: false),
                    current_step = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    linked_content_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ai_content_workflow_sessions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "content_ideas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    target_type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_content_ideas", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_ai_content_workflow_sessions_created_by",
                table: "ai_content_workflow_sessions",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_ai_content_workflow_sessions_idea_id",
                table: "ai_content_workflow_sessions",
                column: "idea_id");

            migrationBuilder.CreateIndex(
                name: "ix_ai_content_workflow_sessions_updated",
                table: "ai_content_workflow_sessions",
                column: "updated_at_utc");

            migrationBuilder.CreateIndex(
                name: "ix_content_ideas_created_by",
                table: "content_ideas",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_content_ideas_status",
                table: "content_ideas",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ai_content_workflow_sessions");

            migrationBuilder.DropTable(
                name: "content_ideas");
        }
    }
}
