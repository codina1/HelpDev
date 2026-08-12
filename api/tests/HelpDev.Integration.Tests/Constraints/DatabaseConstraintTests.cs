using HelpDev.Testing.PostgreSQL;
using HelpDev.Testing.PostgreSQL.Infrastructure;
using Npgsql;

namespace HelpDev.Integration.Tests.Constraints;

[Collection(PostgreSqlCollection.Name)]
[Trait("Category", "Integration")]
[Trait("Category", "PostgreSQL")]
public sealed class DatabaseConstraintTests : PostgreSqlIntegrationTestBase
{
    public DatabaseConstraintTests(PostgreSqlFixture fixture)
        : base(fixture)
    {
    }

    [PostgreSqlFact]
    public async Task Users_mobile_unique_constraint_is_enforced()
    {
        var connectionString = await CreateDatabaseAndMigrateAsync();
        var userId = Guid.NewGuid();

        await ExecuteAsync(connectionString, """
            INSERT INTO users (
                "Id", mobile, full_name, first_name, last_name, email, profile_image_url,
                expertise, interests, role, stack, created_at)
            VALUES (
                @id, @mobile, 'Test User', 'Test', 'User', '', '', '', '', 'User', '', NOW())
            """,
            [
                new NpgsqlParameter("id", userId),
                new NpgsqlParameter("mobile", "09121112233"),
            ]);

        var exception = await Assert.ThrowsAsync<PostgresException>(() =>
            ExecuteAsync(connectionString, """
                INSERT INTO users (
                    "Id", mobile, full_name, first_name, last_name, email, profile_image_url,
                    expertise, interests, role, stack, created_at)
                VALUES (
                    @id, @mobile, 'Duplicate', 'Dup', 'User', '', '', '', '', 'User', '', NOW())
                """,
                [
                    new NpgsqlParameter("id", Guid.NewGuid()),
                    new NpgsqlParameter("mobile", "09121112233"),
                ]));

        Assert.Equal(PostgresErrorCodes.UniqueViolation, exception.SqlState);
    }

    [PostgreSqlFact]
    public async Task Contents_slug_unique_constraint_is_enforced()
    {
        var connectionString = await CreateDatabaseAndMigrateAsync();
        var userId = Guid.NewGuid();
        var contentId = Guid.NewGuid();

        await ExecuteAsync(connectionString, """
            INSERT INTO users (
                "Id", mobile, full_name, first_name, last_name, email, profile_image_url,
                expertise, interests, role, stack, created_at)
            VALUES (
                @id, @mobile, 'Author', 'Auth', 'Or', '', '', '', '', 'User', '', NOW())
            """,
            [
                new NpgsqlParameter("id", userId),
                new NpgsqlParameter("mobile", "09123334455"),
            ]);

        await ExecuteAsync(connectionString, """
            INSERT INTO contents (
                "Id", title, slug, body, type, author_id, status, views, saves, created_at)
            VALUES (
                @id, 'Title', @slug, 'Body', 'Article', @authorId, 'Published', 0, 0, NOW())
            """,
            [
                new NpgsqlParameter("id", contentId),
                new NpgsqlParameter("slug", "unique-slug"),
                new NpgsqlParameter("authorId", userId),
            ]);

        var exception = await Assert.ThrowsAsync<PostgresException>(() =>
            ExecuteAsync(connectionString, """
                INSERT INTO contents (
                    "Id", title, slug, body, type, author_id, status, views, saves, created_at)
                VALUES (
                    @id, 'Duplicate', @slug, 'Body', 'Article', @authorId, 'Published', 0, 0, NOW())
                """,
                [
                    new NpgsqlParameter("id", Guid.NewGuid()),
                    new NpgsqlParameter("slug", "unique-slug"),
                    new NpgsqlParameter("authorId", userId),
                ]));

        Assert.Equal(PostgresErrorCodes.UniqueViolation, exception.SqlState);
    }

    [PostgreSqlFact]
    public async Task Analytics_event_id_primary_key_is_enforced()
    {
        var connectionString = await CreateDatabaseAndMigrateAsync();
        var eventId = Guid.NewGuid();

        await ExecuteAsync(connectionString, """
            INSERT INTO analytics_event_receipts (
                "EventId", event_type, occurred_at_utc, processed_at_utc,
                processing_status, metric_date_utc, schema_version)
            VALUES (
                @eventId, 'identity.user_login_succeeded', NOW(), NOW(),
                'processed', CURRENT_DATE, 1)
            """,
            [new NpgsqlParameter("eventId", eventId)]);

        var exception = await Assert.ThrowsAsync<PostgresException>(() =>
            ExecuteAsync(connectionString, """
                INSERT INTO analytics_event_receipts (
                    "EventId", event_type, occurred_at_utc, processed_at_utc,
                    processing_status, metric_date_utc, schema_version)
                VALUES (
                    @eventId, 'identity.user_login_succeeded', NOW(), NOW(),
                    'processed', CURRENT_DATE, 1)
                """,
                [new NpgsqlParameter("eventId", eventId)]));

        Assert.Equal(PostgresErrorCodes.UniqueViolation, exception.SqlState);
    }

    [PostgreSqlFact]
    public async Task Audit_record_id_primary_key_is_enforced()
    {
        var connectionString = await CreateDatabaseAndMigrateAsync();
        var auditId = Guid.NewGuid();

        await ExecuteAsync(connectionString, """
            INSERT INTO audit_records (
                id, occurred_at_utc, category, action, outcome, actor_type, metadata, created_at_utc)
            VALUES (
                @id, NOW(), 'security', 'security.test', 'success', 'system', '{}', NOW())
            """,
            [new NpgsqlParameter("id", auditId)]);

        var exception = await Assert.ThrowsAsync<PostgresException>(() =>
            ExecuteAsync(connectionString, """
                INSERT INTO audit_records (
                    id, occurred_at_utc, category, action, outcome, actor_type, metadata, created_at_utc)
                VALUES (
                    @id, NOW(), 'security', 'security.test', 'success', 'system', '{}', NOW())
                """,
                [new NpgsqlParameter("id", auditId)]));

        Assert.Equal(PostgresErrorCodes.UniqueViolation, exception.SqlState);
    }

    [PostgreSqlFact]
    public async Task Outbox_message_id_primary_key_is_enforced()
    {
        var connectionString = await CreateDatabaseAndMigrateAsync();
        var messageId = Guid.NewGuid();

        await ExecuteAsync(connectionString, """
            INSERT INTO outbox_messages (
                "Id", occurred_at_utc, type, payload, attempt_count)
            VALUES (
                @id, NOW(), 'content.published.v1', '{}', 0)
            """,
            [new NpgsqlParameter("id", messageId)]);

        var exception = await Assert.ThrowsAsync<PostgresException>(() =>
            ExecuteAsync(connectionString, """
                INSERT INTO outbox_messages (
                    "Id", occurred_at_utc, type, payload, attempt_count)
                VALUES (
                    @id, NOW(), 'content.published.v1', '{}', 0)
                """,
                [new NpgsqlParameter("id", messageId)]));

        Assert.Equal(PostgresErrorCodes.UniqueViolation, exception.SqlState);
    }

    [PostgreSqlFact]
    public async Task Search_documents_source_unique_constraint_is_enforced()
    {
        var connectionString = await CreateDatabaseAndMigrateAsync();
        var sourceId = Guid.NewGuid();

        await ExecuteAsync(connectionString, """
            INSERT INTO search_documents (
                "Id", source_type, source_id, title, slug, summary, url,
                is_published, source_updated_at_utc, indexed_at_utc, last_event_id)
            VALUES (
                @id, 'content', @sourceId, 'Title', 'slug', 'Summary', '/content/slug',
                TRUE, NOW(), NOW(), @eventId)
            """,
            [
                new NpgsqlParameter("id", Guid.NewGuid()),
                new NpgsqlParameter("sourceId", sourceId),
                new NpgsqlParameter("eventId", Guid.NewGuid()),
            ]);

        var exception = await Assert.ThrowsAsync<PostgresException>(() =>
            ExecuteAsync(connectionString, """
                INSERT INTO search_documents (
                    "Id", source_type, source_id, title, slug, summary, url,
                    is_published, source_updated_at_utc, indexed_at_utc, last_event_id)
                VALUES (
                    @id, 'content', @sourceId, 'Duplicate', 'slug-2', 'Summary', '/content/slug-2',
                    TRUE, NOW(), NOW(), @eventId)
                """,
                [
                    new NpgsqlParameter("id", Guid.NewGuid()),
                    new NpgsqlParameter("sourceId", sourceId),
                    new NpgsqlParameter("eventId", Guid.NewGuid()),
                ]));

        Assert.Equal(PostgresErrorCodes.UniqueViolation, exception.SqlState);
    }

    private static async Task ExecuteAsync(
        string connectionString,
        string sql,
        IEnumerable<NpgsqlParameter> parameters)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddRange(parameters.ToArray());
        await command.ExecuteNonQueryAsync();
    }
}
