using HelpDev.Modules.PromptLab.Domain;
using HelpDev.Modules.PromptLab.Domain.Packs;
using HelpDev.Modules.PromptLab.Domain.Prompts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HelpDev.Modules.PromptLab.Infrastructure.Persistence;

public sealed class PromptPackConfiguration : IEntityTypeConfiguration<PromptPack>
{
    public void Configure(EntityTypeBuilder<PromptPack> builder)
    {
        builder.ToTable("promptlab_packs");

        builder.HasKey(pack => pack.Id);

        builder.Property(pack => pack.Id)
            .ValueGeneratedNever();

        builder.Property(pack => pack.Title)
            .IsRequired()
            .HasMaxLength(PromptPack.TitleMaxLength)
            .HasColumnName("title");

        var slugConverter = new ValueConverter<PromptSlug, string>(
            slug => slug.Value,
            value => PromptSlug.FromPersisted(value));

        builder.Property(pack => pack.Slug)
            .HasConversion(slugConverter)
            .IsRequired()
            .HasMaxLength(PromptPack.SlugMaxLength)
            .HasColumnName("slug");

        builder.HasIndex(pack => pack.Slug)
            .IsUnique()
            .HasDatabaseName("ux_promptlab_packs_slug");

        builder.Property(pack => pack.Description)
            .HasMaxLength(PromptPack.DescriptionMaxLength)
            .HasColumnName("description");

        builder.Property(pack => pack.CoverImage)
            .HasMaxLength(PromptLabLimits.MaxPromptCoverImageLength)
            .HasColumnName("cover_image");

        builder.Property(pack => pack.AuthorId)
            .IsRequired()
            .HasColumnName("author_id");

        builder.Property(pack => pack.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(40)
            .HasDefaultValue(PromptPackStatus.Draft)
            .HasColumnName("status");

        builder.Property(pack => pack.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("created_at");

        builder.Property(pack => pack.UpdatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("updated_at");

        builder.Property(pack => pack.PublishedAt)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("published_at");

        builder.HasMany(pack => pack.Items)
            .WithOne()
            .HasForeignKey(item => item.PackId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.Navigation(pack => pack.Items)
            .HasField("_items")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(pack => pack.Status)
            .HasDatabaseName("ix_promptlab_packs_status");

        builder.HasIndex(pack => pack.AuthorId)
            .HasDatabaseName("ix_promptlab_packs_author_id");

        builder.Ignore(pack => pack.IsPublic);
        builder.Ignore(pack => pack.DomainEvents);
        builder.Ignore(pack => pack.HasDomainEvents);
    }
}
