using HelpDev.Infrastructure.Outbox;
using HelpDev.Infrastructure.Persistence;
using HelpDev.Integration.Tests.Helpers;
using HelpDev.Modules.Content.Domain.Enums;
using HelpDev.Modules.Content.Domain.ValueObjects;
using HelpDev.Modules.Identity.Domain.Entities;
using HelpDev.Modules.Identity.Domain.Enums;
using HelpDev.Modules.Search.Domain;
using HelpDev.Testing.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ContentEntity = HelpDev.Modules.Content.Domain.Entities.Content;

namespace HelpDev.Integration.Tests.Outbox;

[Collection(PostgreSqlCollection.Name)]
public sealed class OutboxEndToEndTests : IntegrationTestClassBase
{
    public OutboxEndToEndTests(PostgreSqlFixture fixture)
        : base(fixture)
    {
    }

    [PostgreSqlFact]
    public async Task Publish_content_writes_outbox_and_processor_creates_search_document()
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var authorId = Guid.NewGuid();
        var contentId = Guid.NewGuid();
        var mobile = TestIds.Truncate($"09{Guid.NewGuid():N}", 11);
        var slug = $"outbox-{Guid.NewGuid():N}";

        context.Users.Add(new User
        {
            Id = authorId,
            Mobile = mobile,
            FullName = "Outbox Author",
            FirstName = "Outbox",
            LastName = "Author",
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow,
        });

        var content = ContentEntity.Create(
            contentId,
            "Outbox Integration Content",
            Slug.Create(slug),
            "Integration test body",
            ContentType.Article,
            authorId,
            ContentStatus.Draft,
            DateTime.UtcNow);
        content.SubmitForReview(authorId, DateTime.UtcNow);
        content.Approve(authorId, DateTime.UtcNow);
        content.Publish(authorId, DateTime.UtcNow);
        context.Contents.Add(content);
        await context.SaveChangesAsync();

        var outboxMessage = await context.OutboxMessages
            .Where(message => message.Type == "content.published.v1")
            .OrderBy(message => message.OccurredAtUtc)
            .FirstAsync();
        Assert.Null(outboxMessage.ProcessedAtUtc);

        var processor = Factory.Services.GetRequiredService<OutboxProcessor>();
        await processor.ProcessBatchAsync(CancellationToken.None);

        await using var verifyScope = Factory.Services.CreateAsyncScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var processedMessage = await verifyContext.OutboxMessages.SingleAsync(message => message.Id == outboxMessage.Id);
        Assert.NotNull(processedMessage.ProcessedAtUtc);

        var searchDocument = await verifyContext.SearchDocuments.SingleOrDefaultAsync(
            document => document.SourceType == SearchSourceTypes.Content && document.SourceId == contentId);
        Assert.NotNull(searchDocument);
        Assert.Equal(slug, searchDocument!.Slug);
        Assert.True(searchDocument.IsPublished);
    }

    [PostgreSqlFact]
    public async Task Invalid_outbox_payload_increments_attempts_until_dead_letter()
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var messageId = Guid.NewGuid();
        context.OutboxMessages.Add(new OutboxMessage
        {
            Id = messageId,
            OccurredAtUtc = DateTime.UtcNow,
            Type = "unknown.event.v1",
            Payload = "{}",
            AttemptCount = 0,
        });
        await context.SaveChangesAsync();

        var processor = Factory.Services.GetRequiredService<OutboxProcessor>();

        for (var attempt = 0; attempt < 3; attempt++)
        {
            await processor.ProcessBatchAsync(CancellationToken.None);
        }

        await using var verifyScope = Factory.Services.CreateAsyncScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var message = await verifyContext.OutboxMessages.SingleAsync(row => row.Id == messageId);

        Assert.Null(message.ProcessedAtUtc);
        Assert.Equal(3, message.AttemptCount);
        Assert.NotNull(message.Error);
        Assert.Contains("Outbox", message.Error, StringComparison.OrdinalIgnoreCase);

        await processor.ProcessBatchAsync(CancellationToken.None);
        var afterDeadLetter = await verifyContext.OutboxMessages.AsNoTracking()
            .SingleAsync(row => row.Id == messageId);
        Assert.Equal(3, afterDeadLetter.AttemptCount);
    }

    [PostgreSqlFact]
    public async Task Failed_message_can_be_retried_after_reset_and_successfully_processed()
    {
        await using var setupScope = Factory.Services.CreateAsyncScope();
        var setupContext = setupScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var authorId = Guid.NewGuid();
        var contentId = Guid.NewGuid();

        setupContext.Users.Add(new User
        {
            Id = authorId,
            Mobile = TestIds.Truncate($"09{Guid.NewGuid():N}", 11),
            FullName = "Retry Author",
            FirstName = "Retry",
            LastName = "Author",
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow,
        });

        var content = ContentEntity.Create(
            contentId,
            "Retry Outbox Content",
            Slug.Create("retry-outbox-slug"),
            "Retry body",
            ContentType.Article,
            authorId,
            ContentStatus.Draft,
            DateTime.UtcNow);
        content.SubmitForReview(authorId, DateTime.UtcNow);
        content.Approve(authorId, DateTime.UtcNow);
        content.Publish(authorId, DateTime.UtcNow);
        setupContext.Contents.Add(content);
        await setupContext.SaveChangesAsync();

        var message = await setupContext.OutboxMessages
            .Where(row => row.Type == "content.published.v1")
            .OrderByDescending(row => row.OccurredAtUtc)
            .FirstAsync();
        message.AttemptCount = 1;
        message.Error = "Simulated failure";
        message.LockId = null;
        message.LockedUntilUtc = null;
        await setupContext.SaveChangesAsync();

        var processor = Factory.Services.GetRequiredService<OutboxProcessor>();
        await processor.ProcessBatchAsync(CancellationToken.None);

        await using var verifyScope = Factory.Services.CreateAsyncScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var processed = await verifyContext.OutboxMessages.SingleAsync(row => row.Id == message.Id);
        Assert.NotNull(processed.ProcessedAtUtc);
        Assert.Null(processed.Error);
    }
}
