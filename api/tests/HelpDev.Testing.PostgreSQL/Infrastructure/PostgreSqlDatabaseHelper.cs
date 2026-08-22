using Npgsql;

namespace HelpDev.Testing.PostgreSQL.Infrastructure;

public static class PostgreSqlDatabaseHelper
{
    public const int ExpectedMigrationCount = 27;

    private static readonly HashSet<(string SourceTable, string TargetTable)> AllowedCrossModuleForeignKeys =
    [
        ("contents", "users"),
    ];

    public static IReadOnlyList<string> ExpectedModuleTables { get; } =
    [
        "users",
        "contents",
        "content_revisions",
        "content_workflow_history",
        "content_ideas",
        "ai_content_workflow_sessions",
        "courses",
        "course_sections",
        "course_lessons",
        "enrollments",
        "lesson_progresses",
        "learning_profiles",
        "learning_preferences",
        "learning_roadmaps",
        "learning_roadmap_steps",
        "outbox_messages",
        "search_documents",
        "search_chunks",
        "search_vectors",
        "search_semantic_index_states",
        "administration_announcements",
        "administration_feature_flags",
        "administration_system_settings",
        "toolbox_categories",
        "toolbox_tools",
        "toolbox_favorites",
        "toolbox_execution_records",
        "promptlab_categories",
        "promptlab_prompts",
        "promptlab_library_prompts",
        "promptlab_ai_models",
        "promptlab_packs",
        "promptlab_pack_items",
        "promptlab_versions",
        "promptlab_variables",
        "promptlab_favorites",
        "promptlab_render_records",
        "analytics_event_receipts",
        "analytics_subject_snapshots",
        "analytics_daily_active_users",
        "analytics_daily_metrics",
        "ai_usage_records",
        "audit_records",
        "media_assets",
    ];

    public static async Task TruncateAllModuleTablesAsync(
        string connectionString,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var tableList = string.Join(
            ", ",
            ExpectedModuleTables.Select(table => $"\"{table}\""));

        await using var command = new NpgsqlCommand(
            $"TRUNCATE TABLE {tableList} RESTART IDENTITY CASCADE",
            connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public static async Task<IReadOnlyList<string>> GetExistingModuleTablesAsync(
        string connectionString,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(
            """
            SELECT table_name
            FROM information_schema.tables
            WHERE table_schema = 'public'
              AND table_type = 'BASE TABLE'
              AND table_name = ANY(@tables)
            ORDER BY table_name
            """,
            connection);
        command.Parameters.AddWithValue("tables", ExpectedModuleTables.ToArray());

        var tables = new List<string>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            tables.Add(reader.GetString(0));
        }

        return tables;
    }

    public static async Task<int> GetAppliedMigrationCountAsync(
        string connectionString,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(
            """
            SELECT COUNT(*)
            FROM "__EFMigrationsHistory"
            """,
            connection);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result);
    }

    public static async Task<IReadOnlyList<CrossModuleForeignKey>> GetCrossModuleForeignKeysAsync(
        string connectionString,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(
            """
            SELECT
                tc.table_name AS source_table,
                ccu.table_name AS target_table,
                tc.constraint_name
            FROM information_schema.table_constraints tc
            JOIN information_schema.constraint_column_usage ccu
                ON ccu.constraint_name = tc.constraint_name
               AND ccu.table_schema = tc.table_schema
            WHERE tc.table_schema = 'public'
              AND tc.constraint_type = 'FOREIGN KEY'
            ORDER BY tc.constraint_name
            """,
            connection);

        var foreignKeys = new List<CrossModuleForeignKey>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var sourceTable = reader.GetString(0);
            var targetTable = reader.GetString(1);
            var constraintName = reader.GetString(2);
            var sourceModule = ResolveModule(sourceTable);
            var targetModule = ResolveModule(targetTable);

            if (sourceModule is null || targetModule is null || sourceModule == targetModule)
            {
                continue;
            }

            if (AllowedCrossModuleForeignKeys.Contains((sourceTable, targetTable)))
            {
                continue;
            }

            foreignKeys.Add(new CrossModuleForeignKey(
                constraintName,
                sourceTable,
                targetTable,
                sourceModule,
                targetModule));
        }

        return foreignKeys;
    }

    private static string? ResolveModule(string tableName) =>
        tableName switch
        {
            "users" => "identity",
            "contents" => "content",
            "content_revisions" or "content_workflow_history" or "content_ideas" or "ai_content_workflow_sessions" => "content",
            "courses" or "enrollments" or "course_sections" or "course_lessons" or "lesson_progresses"
                or "learning_profiles" or "learning_preferences" or "learning_roadmaps" or "learning_roadmap_steps" => "learning",
            "outbox_messages" => "infrastructure",
            "search_documents" => "search",
            "search_chunks" or "search_vectors" or "search_semantic_index_states" => "search",
            "audit_records" => "auditing",
            _ when tableName.StartsWith("administration_", StringComparison.Ordinal) => "administration",
            _ when tableName.StartsWith("toolbox_", StringComparison.Ordinal) => "toolbox",
            _ when tableName.StartsWith("promptlab_", StringComparison.Ordinal) => "promptlab",
            _ when tableName.StartsWith("analytics_", StringComparison.Ordinal) => "analytics",
            "ai_usage_records" => "analytics",
            _ when tableName.StartsWith("media_", StringComparison.Ordinal) => "media",
            _ => null,
        };

    public sealed record CrossModuleForeignKey(
        string ConstraintName,
        string SourceTable,
        string TargetTable,
        string SourceModule,
        string TargetModule);
}
