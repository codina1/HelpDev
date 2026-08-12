using HelpDev.Modules.Administration.Domain.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDev.Modules.Administration.Infrastructure.Persistence;

public sealed class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        builder.ToTable("administration_system_settings");

        builder.HasKey(setting => setting.Id);

        builder.Property(setting => setting.Id)
            .ValueGeneratedNever();

        builder.Property(setting => setting.Key)
            .IsRequired()
            .HasMaxLength(SystemSetting.KeyMaxLength)
            .HasColumnName("key");

        builder.HasIndex(setting => setting.Key)
            .IsUnique()
            .HasDatabaseName("ux_administration_system_settings_key");

        builder.Property(setting => setting.Value)
            .IsRequired()
            .HasMaxLength(SystemSetting.ValueMaxLength)
            .HasColumnName("value");

        builder.Property(setting => setting.Description)
            .HasMaxLength(SystemSetting.DescriptionMaxLength)
            .HasColumnName("description");

        builder.Property(setting => setting.ValueType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasColumnName("value_type");

        builder.Property(setting => setting.IsPublic)
            .IsRequired()
            .HasColumnName("is_public");

        builder.HasIndex(setting => setting.IsPublic)
            .HasDatabaseName("ix_administration_system_settings_is_public");

        builder.Property(setting => setting.CreatedAtUtc)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("created_at_utc");

        builder.Property(setting => setting.UpdatedAtUtc)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("updated_at_utc");
    }
}
