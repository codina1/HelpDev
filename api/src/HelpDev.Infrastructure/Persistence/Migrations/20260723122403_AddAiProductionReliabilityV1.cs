using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpDev.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAiProductionReliabilityV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "user_id",
                table: "ai_usage_records",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<int>(
                name: "duration_ms",
                table: "ai_usage_records",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "error_code",
                table: "ai_usage_records",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "success",
                table: "ai_usage_records",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "ix_ai_usage_records_success_created",
                table: "ai_usage_records",
                columns: new[] { "success", "created_at_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_ai_usage_records_success_created",
                table: "ai_usage_records");

            migrationBuilder.DropColumn(
                name: "duration_ms",
                table: "ai_usage_records");

            migrationBuilder.DropColumn(
                name: "error_code",
                table: "ai_usage_records");

            migrationBuilder.DropColumn(
                name: "success",
                table: "ai_usage_records");

            migrationBuilder.AlterColumn<Guid>(
                name: "user_id",
                table: "ai_usage_records",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
