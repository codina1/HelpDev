using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpDev.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSecurityAuditV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    occurred_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    action = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    outcome = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    actor_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    actor_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    subject_id = table.Column<Guid>(type: "uuid", nullable: true),
                    subject_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    subject_display = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    reason_code = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    correlation_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    request_method = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    request_path_template = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    metadata = table.Column<IReadOnlyDictionary<string, string>>(type: "jsonb", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_records", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_audit_records_action_occurred_at_utc",
                table: "audit_records",
                columns: new[] { "action", "occurred_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_records_actor_user_id_occurred_at_utc",
                table: "audit_records",
                columns: new[] { "actor_user_id", "occurred_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_records_category_occurred_at_utc",
                table: "audit_records",
                columns: new[] { "category", "occurred_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_records_correlation_id",
                table: "audit_records",
                column: "correlation_id");

            migrationBuilder.CreateIndex(
                name: "ix_audit_records_occurred_at_utc",
                table: "audit_records",
                column: "occurred_at_utc");

            migrationBuilder.CreateIndex(
                name: "ix_audit_records_outcome_occurred_at_utc",
                table: "audit_records",
                columns: new[] { "outcome", "occurred_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_records_subject_occurred_at_utc",
                table: "audit_records",
                columns: new[] { "subject_type", "subject_id", "occurred_at_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_records");
        }
    }
}
