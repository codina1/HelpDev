using HelpDev.Modules.Toolbox.Domain.Favorites;
using HelpDev.Modules.Toolbox.Domain.Tools;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDev.Modules.Toolbox.Infrastructure.Persistence;

public sealed class ToolFavoriteConfiguration : IEntityTypeConfiguration<ToolFavorite>
{
    public void Configure(EntityTypeBuilder<ToolFavorite> builder)
    {
        builder.ToTable("toolbox_favorites");

        builder.HasKey(favorite => favorite.Id);

        builder.Property(favorite => favorite.Id)
            .ValueGeneratedNever();

        builder.Property(favorite => favorite.UserId)
            .IsRequired()
            .HasColumnName("user_id");

        builder.Property(favorite => favorite.ToolId)
            .IsRequired()
            .HasColumnName("tool_id");

        builder.HasOne<ToolDefinition>()
            .WithMany()
            .HasForeignKey(favorite => favorite.ToolId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(favorite => favorite.CreatedAtUtc)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("created_at_utc");

        builder.HasIndex(favorite => new { favorite.UserId, favorite.ToolId })
            .IsUnique()
            .HasDatabaseName("ux_toolbox_favorites_user_id_tool_id");

        builder.HasIndex(favorite => favorite.UserId)
            .HasDatabaseName("ix_toolbox_favorites_user_id");

        builder.HasIndex(favorite => favorite.ToolId)
            .HasDatabaseName("ix_toolbox_favorites_tool_id");
    }
}
