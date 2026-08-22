using HelpDev.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpDev.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260822090000_AddContentBlockEditorFields")]
    public partial class AddContentBlockEditorFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "content_json",
                table: "contents",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "content_html",
                table: "contents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "content_format",
                table: "contents",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "editor_version",
                table: "contents",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "word_count",
                table: "contents",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "reading_time_minutes",
                table: "contents",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_autosaved_at_utc",
                table: "contents",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "content_json", table: "contents");
            migrationBuilder.DropColumn(name: "content_html", table: "contents");
            migrationBuilder.DropColumn(name: "content_format", table: "contents");
            migrationBuilder.DropColumn(name: "editor_version", table: "contents");
            migrationBuilder.DropColumn(name: "word_count", table: "contents");
            migrationBuilder.DropColumn(name: "reading_time_minutes", table: "contents");
            migrationBuilder.DropColumn(name: "last_autosaved_at_utc", table: "contents");
        }
    }
}
