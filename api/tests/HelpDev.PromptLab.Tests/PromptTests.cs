using HelpDev.Modules.PromptLab.Domain;
using HelpDev.Modules.PromptLab.Domain.Prompts;
using HelpDev.Modules.PromptLab.Domain.Specifications;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.PromptLab.Tests;

public sealed class PromptTests
{
    private static readonly DateTime Now = new(2026, 8, 17, 8, 0, 0, DateTimeKind.Utc);
    private static readonly Guid AuthorId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    [Fact]
    public void Create_starts_as_draft_owned_by_author()
    {
        var prompt = CreatePrompt(title: "  Review helper  ", content: "  Review {{code}}  ");

        Assert.Equal("Review helper", prompt.Title);
        Assert.Equal("review-helper", prompt.Slug.Value);
        Assert.Equal("Review {{code}}", prompt.Content);
        Assert.Equal(PromptStatus.Draft, prompt.Status);
        Assert.Equal(AuthorId, prompt.AuthorId);
        Assert.Equal(0, prompt.Views);
        Assert.Equal(0, prompt.CopyCount);
        Assert.Equal(Now, prompt.CreatedAt);
        Assert.Equal(Now, prompt.UpdatedAt);
        Assert.Null(prompt.PublishedAt);
        Assert.False(prompt.IsPublic);
        Assert.True(prompt.CanBeEditedBy(AuthorId));
        Assert.False(prompt.CanBeEditedBy(Guid.NewGuid()));
    }

    [Fact]
    public void Create_rejects_invalid_fields()
    {
        var titleEx = Assert.Throws<DomainException>(() => CreatePrompt(title: " "));
        Assert.Equal(PromptLabErrorCodes.PromptTitleRequired, titleEx.Code);

        var slugEx = Assert.Throws<DomainException>(() => CreatePrompt(slug: "Bad Slug!"));
        Assert.Equal(PromptLabErrorCodes.PromptSlugInvalid, slugEx.Code);

        var contentEx = Assert.Throws<DomainException>(() => CreatePrompt(content: " "));
        Assert.Equal(PromptLabErrorCodes.PromptContentRequired, contentEx.Code);

        var authorEx = Assert.Throws<DomainException>(() => CreatePrompt(authorId: Guid.Empty));
        Assert.Equal(PromptLabErrorCodes.PromptAuthorInvalid, authorEx.Code);

        var modelEx = Assert.Throws<DomainException>(() => CreatePrompt(aiModel: " "));
        Assert.Equal(PromptLabErrorCodes.PromptAiModelRequired, modelEx.Code);

        var mediaEx = Assert.Throws<DomainException>(() => CreatePrompt(mediaType: (PromptMediaType)99));
        Assert.Equal(PromptLabErrorCodes.PromptMediaTypeInvalid, mediaEx.Code);
    }

    [Fact]
    public void Owner_can_edit_draft_and_non_owner_cannot()
    {
        var prompt = CreatePrompt();
        var stranger = Guid.NewGuid();

        Assert.True(prompt.Update(
            AuthorId,
            "Updated title",
            "review-helper",
            "desc",
            "Updated body",
            "/covers/a.png",
            PromptMediaType.Image,
            "gpt-4o",
            Now.AddMinutes(1)));

        Assert.Equal("Updated title", prompt.Title);
        Assert.Equal("Updated body", prompt.Content);
        Assert.Equal(PromptMediaType.Image, prompt.MediaType);
        Assert.Equal(Now.AddMinutes(1), prompt.UpdatedAt);

        var forbidden = Assert.Throws<DomainException>(() => prompt.Update(
            stranger,
            "Hacked",
            "review-helper",
            null,
            "secret",
            null,
            PromptMediaType.Text,
            "gpt-4o-mini",
            Now.AddMinutes(2)));
        Assert.Equal(PromptLabErrorCodes.PromptEditForbidden, forbidden.Code);
        Assert.Equal("Updated title", prompt.Title);
    }

    [Fact]
    public void Submitted_and_approved_prompts_cannot_be_edited()
    {
        var prompt = CreatePrompt();
        prompt.Submit(AuthorId, Now.AddMinutes(1));

        var notDraft = Assert.Throws<DomainException>(() => prompt.Update(
            AuthorId,
            "Still draft?",
            "review-helper",
            null,
            "nope",
            null,
            PromptMediaType.Text,
            "gpt-4o-mini",
            Now.AddMinutes(2)));
        Assert.Equal(PromptLabErrorCodes.PromptNotDraft, notDraft.Code);

        prompt.Approve(Now.AddMinutes(3));
        Assert.Throws<DomainException>(() => prompt.Update(
            AuthorId,
            "Approved edit",
            "review-helper",
            null,
            "nope",
            null,
            PromptMediaType.Text,
            "gpt-4o-mini",
            Now.AddMinutes(4)));
    }

    [Fact]
    public void Content_is_not_public_until_approved()
    {
        var prompt = CreatePrompt(content: "private template");
        var spec = new PublicPromptSpecification();

        var hidden = Assert.Throws<DomainException>(() => prompt.GetPublicContent());
        Assert.Equal(PromptLabErrorCodes.PromptNotPublic, hidden.Code);
        Assert.False(spec.IsSatisfiedBy(prompt));

        prompt.Submit(AuthorId, Now.AddMinutes(1));
        Assert.Throws<DomainException>(prompt.GetPublicContent);
        Assert.False(spec.IsSatisfiedBy(prompt));

        prompt.Approve(Now.AddMinutes(2));
        Assert.Equal("private template", prompt.GetPublicContent());
        Assert.True(prompt.IsPublic);
        Assert.Equal(Now.AddMinutes(2), prompt.PublishedAt);
        Assert.True(spec.IsSatisfiedBy(prompt));
        Assert.Contains(prompt.DomainEvents, e => e is PromptApprovedDomainEvent);
    }

    [Fact]
    public void Reject_is_not_public_and_owner_can_return_to_draft()
    {
        var prompt = CreatePrompt();
        prompt.Submit(AuthorId, Now.AddMinutes(1));
        prompt.Reject(Now.AddMinutes(2));

        Assert.Equal(PromptStatus.Rejected, prompt.Status);
        Assert.False(prompt.IsPublic);
        Assert.Throws<DomainException>(prompt.GetPublicContent);

        var stranger = Guid.NewGuid();
        var forbidden = Assert.Throws<DomainException>(() => prompt.ReturnToDraft(stranger, Now.AddMinutes(3)));
        Assert.Equal(PromptLabErrorCodes.PromptEditForbidden, forbidden.Code);

        prompt.ReturnToDraft(AuthorId, Now.AddMinutes(4));
        Assert.Equal(PromptStatus.Draft, prompt.Status);
        Assert.True(prompt.CanBeEditedBy(AuthorId));
    }

    [Fact]
    public void Views_and_copies_are_recorded_only_when_public()
    {
        var prompt = CreatePrompt();
        Assert.Throws<DomainException>(prompt.RecordView);
        Assert.Throws<DomainException>(prompt.RecordCopy);

        prompt.Submit(AuthorId, Now.AddMinutes(1));
        prompt.Approve(Now.AddMinutes(2));
        prompt.RecordView();
        prompt.RecordCopy();
        prompt.RecordCopy();

        Assert.Equal(1, prompt.Views);
        Assert.Equal(2, prompt.CopyCount);
    }

    [Fact]
    public void Illegal_status_transitions_are_rejected()
    {
        var prompt = CreatePrompt();
        Assert.Throws<DomainException>(() => prompt.Approve(Now.AddMinutes(1)));
        Assert.Throws<DomainException>(() => prompt.Reject(Now.AddMinutes(1)));

        prompt.Submit(AuthorId, Now.AddMinutes(1));
        Assert.Throws<DomainException>(() => prompt.Submit(AuthorId, Now.AddMinutes(2)));
        Assert.False(PromptWorkflowRules.IsAllowed(PromptStatus.Approved, PromptStatus.Draft));
    }

    private static Prompt CreatePrompt(
        string title = "Review helper",
        string slug = "review-helper",
        string? description = "Helps review code",
        string content = "Review {{code}}",
        string? coverImage = null,
        PromptMediaType mediaType = PromptMediaType.Text,
        string aiModel = "gpt-4o-mini",
        Guid? authorId = null) =>
        Prompt.Create(
            Guid.NewGuid(),
            title,
            slug,
            description,
            content,
            coverImage,
            mediaType,
            aiModel,
            authorId ?? AuthorId,
            Now);
}
