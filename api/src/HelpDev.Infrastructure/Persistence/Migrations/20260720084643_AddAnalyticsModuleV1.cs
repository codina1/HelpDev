using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpDev.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAnalyticsModuleV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "analytics_daily_active_users",
                columns: table => new
                {
                    date_utc = table.Column<DateOnly>(type: "date", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_seen_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_analytics_daily_active_users", x => new { x.date_utc, x.user_id });
                });

            migrationBuilder.CreateTable(
                name: "analytics_daily_metrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_utc = table.Column<DateOnly>(type: "date", nullable: false),
                    metric_key = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    subject_id = table.Column<Guid>(type: "uuid", nullable: true),
                    subject_type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    dimension1_key = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    dimension1_value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    dimension2_key = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    dimension2_value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    count = table.Column<long>(type: "bigint", nullable: false),
                    success_count = table.Column<long>(type: "bigint", nullable: false),
                    failure_count = table.Column<long>(type: "bigint", nullable: false),
                    total_duration_milliseconds = table.Column<long>(type: "bigint", nullable: false),
                    min_duration_milliseconds = table.Column<long>(type: "bigint", nullable: false),
                    max_duration_milliseconds = table.Column<long>(type: "bigint", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_analytics_daily_metrics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "analytics_event_receipts",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    occurred_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processing_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    error_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    metric_date_utc = table.Column<DateOnly>(type: "date", nullable: false),
                    schema_version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_analytics_event_receipts", x => x.EventId);
                });

            migrationBuilder.CreateTable(
                name: "analytics_subject_snapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    subject_id = table.Column<Guid>(type: "uuid", nullable: false),
                    display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    slug = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_analytics_subject_snapshots", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_analytics_daily_active_users_date_utc",
                table: "analytics_daily_active_users",
                column: "date_utc");

            migrationBuilder.CreateIndex(
                name: "ix_analytics_daily_active_users_user_id",
                table: "analytics_daily_active_users",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_analytics_daily_metrics_date_utc",
                table: "analytics_daily_metrics",
                column: "date_utc");

            migrationBuilder.CreateIndex(
                name: "ix_analytics_daily_metrics_metric_key_date_utc",
                table: "analytics_daily_metrics",
                columns: new[] { "metric_key", "date_utc" });

            migrationBuilder.CreateIndex(
                name: "ux_analytics_daily_metrics_identity",
                table: "analytics_daily_metrics",
                columns: new[] { "date_utc", "metric_key", "subject_id", "subject_type", "dimension1_key", "dimension1_value", "dimension2_key", "dimension2_value" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "ix_analytics_event_receipts_event_type",
                table: "analytics_event_receipts",
                column: "event_type");

            migrationBuilder.CreateIndex(
                name: "ix_analytics_event_receipts_metric_date_utc",
                table: "analytics_event_receipts",
                column: "metric_date_utc");

            migrationBuilder.CreateIndex(
                name: "ix_analytics_event_receipts_processed_at_utc",
                table: "analytics_event_receipts",
                column: "processed_at_utc");

            migrationBuilder.CreateIndex(
                name: "ix_analytics_subject_snapshots_subject_type",
                table: "analytics_subject_snapshots",
                column: "subject_type");

            migrationBuilder.CreateIndex(
                name: "ux_analytics_subject_snapshots_subject_type_subject_id",
                table: "analytics_subject_snapshots",
                columns: new[] { "subject_type", "subject_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "analytics_daily_active_users");

            migrationBuilder.DropTable(
                name: "analytics_daily_metrics");

            migrationBuilder.DropTable(
                name: "analytics_event_receipts");

            migrationBuilder.DropTable(
                name: "analytics_subject_snapshots");
        }
    }
}
