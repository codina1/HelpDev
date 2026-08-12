using HelpDev.Modules.Administration.Domain.FeatureFlags;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDev.Modules.Administration.Infrastructure.Persistence;

public sealed class FeatureFlagConfiguration : IEntityTypeConfiguration<FeatureFlag>
{
    public void Configure(EntityTypeBuilder<FeatureFlag> builder)
    {
        builder.ToTable("administration_feature_flags");

        builder.HasKey(flag => flag.Id);

        builder.Property(flag => flag.Id)
            .ValueGeneratedNever();

        builder.Property(flag => flag.Key)
            .IsRequired()
            .HasMaxLength(FeatureFlag.KeyMaxLength)
            .HasColumnName("key");

        builder.HasIndex(flag => flag.Key)
            .IsUnique()
            .HasDatabaseName("ux_administration_feature_flags_key");

        builder.Property(flag => flag.IsEnabled)
            .IsRequired()
            .HasColumnName("is_enabled");

        builder.HasIndex(flag => flag.IsEnabled)
            .HasDatabaseName("ix_administration_feature_flags_is_enabled");

        builder.Property(flag => flag.Description)
            .HasMaxLength(FeatureFlag.DescriptionMaxLength)
            .HasColumnName("description");

        builder.Property(flag => flag.CreatedAtUtc)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("created_at_utc");

        builder.Property(flag => flag.UpdatedAtUtc)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("updated_at_utc");
    }
}
