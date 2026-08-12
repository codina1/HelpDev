using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpDev.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSearchDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "search_documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    source_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    slug = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    summary = table.Column<string>(type: "text", nullable: false),
                    url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_published = table.Column<bool>(type: "boolean", nullable: false),
                    source_published_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    source_updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    indexed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_event_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_search_documents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_search_documents_is_published",
                table: "search_documents",
                column: "is_published");

            migrationBuilder.CreateIndex(
                name: "ix_search_documents_published_type_title",
                table: "search_documents",
                columns: new[] { "is_published", "source_type", "title" });

            migrationBuilder.CreateIndex(
                name: "ux_search_documents_source",
                table: "search_documents",
                columns: new[] { "source_type", "source_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "search_documents");
        }
    }
}
