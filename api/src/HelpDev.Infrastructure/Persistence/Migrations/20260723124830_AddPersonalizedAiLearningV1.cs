using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpDev.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonalizedAiLearningV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "learning_profiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    experience_level = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    learning_goals = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    current_skills = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_learning_profiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "learning_roadmaps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    goal = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    approved_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_learning_roadmaps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "learning_preferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    topic = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    interest_level = table.Column<int>(type: "integer", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_learning_preferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_learning_preferences_learning_profiles_profile_id",
                        column: x => x.profile_id,
                        principalTable: "learning_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "learning_roadmap_steps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    roadmap_id = table.Column<Guid>(type: "uuid", nullable: false),
                    step_order = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    related_course_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_learning_roadmap_steps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_learning_roadmap_steps_learning_roadmaps_roadmap_id",
                        column: x => x.roadmap_id,
                        principalTable: "learning_roadmaps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_learning_preferences_profile_topic",
                table: "learning_preferences",
                columns: new[] { "profile_id", "topic" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_learning_profiles_user_id",
                table: "learning_profiles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_learning_roadmap_steps_order",
                table: "learning_roadmap_steps",
                columns: new[] { "roadmap_id", "step_order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_learning_roadmaps_user_id",
                table: "learning_roadmaps",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "learning_preferences");

            migrationBuilder.DropTable(
                name: "learning_roadmap_steps");

            migrationBuilder.DropTable(
                name: "learning_profiles");

            migrationBuilder.DropTable(
                name: "learning_roadmaps");
        }
    }
}
