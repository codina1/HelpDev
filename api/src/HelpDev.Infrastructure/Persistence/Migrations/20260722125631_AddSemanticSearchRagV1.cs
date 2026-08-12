using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace HelpDev.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSemanticSearchRagV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.CreateTable(
                name: "search_chunks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    source_id = table.Column<Guid>(type: "uuid", nullable: false),
                    chunk_index = table.Column<int>(type: "integer", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    metadata = table.Column<string>(type: "text", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_event_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_search_chunks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "search_semantic_index_states",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    source_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    chunk_count = table.Column<int>(type: "integer", nullable: false),
                    last_event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_indexed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    failure_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_search_semantic_index_states", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "search_vectors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    chunk_id = table.Column<Guid>(type: "uuid", nullable: false),
                    embedding = table.Column<Vector>(type: "vector(384)", nullable: false),
                    dimensions = table.Column<int>(type: "integer", nullable: false),
                    model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_search_vectors", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_search_chunks_source",
                table: "search_chunks",
                columns: new[] { "source_type", "source_id" });

            migrationBuilder.CreateIndex(
                name: "ux_search_chunks_source_index",
                table: "search_chunks",
                columns: new[] { "source_type", "source_id", "chunk_index" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_search_semantic_index_states_status",
                table: "search_semantic_index_states",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ux_search_semantic_index_states_source",
                table: "search_semantic_index_states",
                columns: new[] { "source_type", "source_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_search_vectors_chunk",
                table: "search_vectors",
                column: "chunk_id",
                unique: true);

            migrationBuilder.Sql("""
                CREATE INDEX IF NOT EXISTS ix_search_vectors_embedding_hnsw
                ON search_vectors
                USING hnsw (embedding vector_cosine_ops);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS ix_search_vectors_embedding_hnsw;");

            migrationBuilder.DropTable(
                name: "search_chunks");

            migrationBuilder.DropTable(
                name: "search_semantic_index_states");

            migrationBuilder.DropTable(
                name: "search_vectors");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:vector", ",,");
        }
    }
}
