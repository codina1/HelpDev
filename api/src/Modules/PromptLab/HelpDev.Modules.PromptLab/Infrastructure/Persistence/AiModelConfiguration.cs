using HelpDev.Modules.PromptLab.Domain.AiModels;
using HelpDev.Modules.PromptLab.Domain.Prompts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HelpDev.Modules.PromptLab.Infrastructure.Persistence;

public sealed class AiModelConfiguration : IEntityTypeConfiguration<AiModel>
{
    public void Configure(EntityTypeBuilder<AiModel> builder)
    {
        builder.ToTable("promptlab_ai_models");

        builder.HasKey(model => model.Id);

        builder.Property(model => model.Id)
            .ValueGeneratedNever();

        builder.Property(model => model.Name)
            .IsRequired()
            .HasMaxLength(AiModel.NameMaxLength)
            .HasColumnName("name");

        var slugConverter = new ValueConverter<PromptSlug, string>(
            slug => slug.Value,
            value => PromptSlug.FromPersisted(value));

        builder.Property(model => model.Slug)
            .HasConversion(slugConverter)
            .IsRequired()
            .HasMaxLength(AiModel.SlugMaxLength)
            .HasColumnName("slug");

        builder.HasIndex(model => model.Slug)
            .IsUnique()
            .HasDatabaseName("ux_promptlab_ai_models_slug");

        builder.Property(model => model.Provider)
            .IsRequired()
            .HasMaxLength(AiModel.ProviderMaxLength)
            .HasColumnName("provider");

        builder.Property(model => model.Logo)
            .HasMaxLength(AiModel.LogoMaxLength)
            .HasColumnName("logo");

        builder.Property(model => model.IsActive)
            .IsRequired()
            .HasColumnName("is_active");

        builder.Property(model => model.CreatedAtUtc)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("created_at_utc");

        builder.Property(model => model.UpdatedAtUtc)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("updated_at_utc");

        builder.HasIndex(model => model.IsActive)
            .HasDatabaseName("ix_promptlab_ai_models_is_active");

        builder.HasIndex(model => model.Provider)
            .HasDatabaseName("ix_promptlab_ai_models_provider");

        builder.Ignore(model => model.DomainEvents);
        builder.Ignore(model => model.HasDomainEvents);
    }
}
