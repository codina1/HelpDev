using HelpDev.Modules.PromptLab.Domain.Favorites;
using HelpDev.Modules.PromptLab.Domain.Prompts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDev.Modules.PromptLab.Infrastructure.Persistence;

public sealed class PromptFavoriteConfiguration : IEntityTypeConfiguration<PromptFavorite>
{
    public void Configure(EntityTypeBuilder<PromptFavorite> builder)
    {
        builder.ToTable("promptlab_favorites");

        builder.HasKey(favorite => favorite.Id);

        builder.Property(favorite => favorite.Id)
            .ValueGeneratedNever();

        builder.Property(favorite => favorite.UserId)
            .IsRequired()
            .HasColumnName("user_id");

        builder.Property(favorite => favorite.PromptDefinitionId)
            .IsRequired()
            .HasColumnName("prompt_definition_id");

        builder.HasOne<PromptDefinition>()
            .WithMany()
            .HasForeignKey(favorite => favorite.PromptDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(favorite => favorite.CreatedAtUtc)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("created_at_utc");

        builder.HasIndex(favorite => new { favorite.UserId, favorite.PromptDefinitionId })
            .IsUnique()
            .HasDatabaseName("ux_promptlab_favorites_user_id_prompt_definition_id");

        builder.HasIndex(favorite => favorite.UserId)
            .HasDatabaseName("ix_promptlab_favorites_user_id");

        builder.HasIndex(favorite => favorite.PromptDefinitionId)
            .HasDatabaseName("ix_promptlab_favorites_prompt_definition_id");
    }
}
