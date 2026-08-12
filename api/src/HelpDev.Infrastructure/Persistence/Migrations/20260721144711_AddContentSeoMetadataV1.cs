using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpDev.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddContentSeoMetadataV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "canonical_url",
                table: "contents",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "focus_keyword",
                table: "contents",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "og_image",
                table: "contents",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "seo_description",
                table: "contents",
                type: "character varying(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "seo_title",
                table: "contents",
                type: "character varying(70)",
                maxLength: 70,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "canonical_url",
                table: "contents");

            migrationBuilder.DropColumn(
                name: "focus_keyword",
                table: "contents");

            migrationBuilder.DropColumn(
                name: "og_image",
                table: "contents");

            migrationBuilder.DropColumn(
                name: "seo_description",
                table: "contents");

            migrationBuilder.DropColumn(
                name: "seo_title",
                table: "contents");
        }
    }
}
