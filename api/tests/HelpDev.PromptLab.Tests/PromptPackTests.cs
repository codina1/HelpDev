using HelpDev.Modules.PromptLab.Domain;
using HelpDev.Modules.PromptLab.Domain.Packs;
using HelpDev.Modules.PromptLab.Domain.Prompts;
using HelpDev.Modules.PromptLab.Domain.Specifications;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.PromptLab.Tests;

public sealed class PromptPackTests
{
    private static readonly DateTime Now = new(2026, 8, 17, 8, 0, 0, DateTimeKind.Utc);
    private static readonly Guid AuthorId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    [Fact]
    public void Create_starts_as_unpublished_draft()
    {
        var pack = CreatePack(title: "  Starter pack  ");

        Assert.Equal("Starter pack", pack.Title);
        Assert.Equal("starter-pack", pack.Slug.Value);
        Assert.Equal(AuthorId, pack.AuthorId);
        Assert.Equal(PromptPackStatus.Draft, pack.Status);
        Assert.False(pack.IsPublic);
        Assert.Null(pack.PublishedAt);
        Assert.Empty(pack.Items);
        Assert.True(pack.CanBeEditedBy(AuthorId));
    }

    [Fact]
    public void Create_rejects_invalid_fields()
    {
        var titleEx = Assert.Throws<DomainException>(() => CreatePack(title: " "));
        Assert.Equal(PromptLabErrorCodes.PackTitleRequired, titleEx.Code);

        var slugEx = Assert.Throws<DomainException>(() => CreatePack(slug: "Bad Slug!"));
        Assert.Equal(PromptLabErrorCodes.PackSlugInvalid, slugEx.Code);

        var authorEx = Assert.Throws<DomainException>(() => CreatePack(authorId: Guid.Empty));
        Assert.Equal(PromptLabErrorCodes.PackAuthorInvalid, authorEx.Code);
    }

    [Fact]
    public void Pack_preserves_item_order_and_rejects_duplicates()
    {
        var pack = CreatePack();
        var first = CreateApprovedPrompt();
        var second = CreateApprovedPrompt();

        var firstItem = pack.AddItem(AuthorId, Guid.NewGuid(), first, Now.AddMinutes(1));
        var secondItem = pack.AddItem(AuthorId, Guid.NewGuid(), second, Now.AddMinutes(2));

        Assert.Equal(new[] { 1, 2 }, pack.Items.Select(item => item.Order));
        Assert.Equal(new[] { first.Id, second.Id }, pack.Items.Select(item => item.PromptId));
        Assert.Equal(pack.Id, firstItem.PackId);
        Assert.Equal(pack.Id, secondItem.PackId);

        var duplicate = Assert.Throws<DomainException>(
            () => pack.AddItem(AuthorId, Guid.NewGuid(), first, Now.AddMinutes(3)));
        Assert.Equal(PromptLabErrorCodes.PackItemDuplicate, duplicate.Code);

        pack.ReorderItem(AuthorId, first.Id, 2, Now.AddMinutes(4));
        Assert.Equal(new[] { second.Id, first.Id }, pack.Items.Select(item => item.PromptId));
        Assert.Equal(new[] { 1, 2 }, pack.Items.Select(item => item.Order));

        pack.RemoveItem(AuthorId, second.Id, Now.AddMinutes(5));
        Assert.Single(pack.Items);
        Assert.Equal(1, pack.Items[0].Order);
        Assert.Equal(first.Id, pack.Items[0].PromptId);
    }

    [Fact]
    public void Draft_prompt_cannot_be_added_and_unapproved_pack_is_not_public()
    {
        var pack = CreatePack();
        var draft = CreateDraftPrompt();
        var spec = new PublicPromptPackSpecification();

        var hiddenPrompt = Assert.Throws<DomainException>(
            () => pack.AddItem(AuthorId, Guid.NewGuid(), draft, Now.AddMinutes(1)));
        Assert.Equal(PromptLabErrorCodes.PackItemPromptNotPublic, hiddenPrompt.Code);

        var empty = Assert.Throws<DomainException>(() => pack.Approve(Now.AddMinutes(2)));
        Assert.Equal(PromptLabErrorCodes.PackEmpty, empty.Code);

        pack.AddItem(AuthorId, Guid.NewGuid(), CreateApprovedPrompt(), Now.AddMinutes(4));

        Assert.Throws<DomainException>(pack.GetPublicItems);
        Assert.False(spec.IsSatisfiedBy(pack));

        pack.Submit(AuthorId, Now.AddMinutes(5));
        pack.Approve(Now.AddMinutes(6));

        Assert.True(pack.IsPublic);
        Assert.Equal(Now.AddMinutes(6), pack.PublishedAt);
        Assert.Single(pack.GetPublicItems());
        Assert.True(spec.IsSatisfiedBy(pack));
        Assert.Contains(pack.DomainEvents, e => e is PromptPackApprovedDomainEvent);
    }

    [Fact]
    public void Non_owner_cannot_mutate_pack_items()
    {
        var pack = CreatePack();
        var prompt = CreateApprovedPrompt();
        var stranger = Guid.NewGuid();

        var forbidden = Assert.Throws<DomainException>(
            () => pack.AddItem(stranger, Guid.NewGuid(), prompt, Now.AddMinutes(1)));
        Assert.Equal(PromptLabErrorCodes.PackEditForbidden, forbidden.Code);
    }

    private static PromptPack CreatePack(
        string title = "Starter pack",
        string slug = "starter-pack",
        Guid? authorId = null) =>
        PromptPack.Create(
            Guid.NewGuid(),
            title,
            slug,
            "A set of prompts",
            null,
            authorId ?? AuthorId,
            Now);

    private static Prompt CreateApprovedPrompt()
    {
        var prompt = CreateDraftPrompt();
        prompt.Submit(AuthorId, Now.AddMinutes(1));
        prompt.Approve(Now.AddMinutes(2));
        return prompt;
    }

    private static Prompt CreateDraftPrompt() =>
        Prompt.Create(
            Guid.NewGuid(),
            "Review helper",
            "review-helper-" + Guid.NewGuid().ToString("N")[..8],
            null,
            "Review {{code}}",
            null,
            PromptMediaType.Text,
            Guid.NewGuid(),
            Guid.NewGuid(),
            AuthorId,
            Now);
}
