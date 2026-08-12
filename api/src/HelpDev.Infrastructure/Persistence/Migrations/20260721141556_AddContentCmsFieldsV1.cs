using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpDev.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddContentCmsFieldsV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cover_image",
                table: "contents",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "excerpt",
                table: "contents",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "published_at_utc",
                table: "contents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "contents",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            // Backfill new timestamps for pre-existing rows so ordering/read models stay sensible.
            migrationBuilder.Sql("UPDATE contents SET updated_at = created_at;");
            migrationBuilder.Sql("UPDATE contents SET published_at_utc = created_at WHERE status = 'Published';");

            migrationBuilder.CreateIndex(
                name: "IX_contents_updated_at",
                table: "contents",
                column: "updated_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_contents_updated_at",
                table: "contents");

            migrationBuilder.DropColumn(
                name: "cover_image",
                table: "contents");

            migrationBuilder.DropColumn(
                name: "excerpt",
                table: "contents");

            migrationBuilder.DropColumn(
                name: "published_at_utc",
                table: "contents");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "contents");
        }
    }
}
