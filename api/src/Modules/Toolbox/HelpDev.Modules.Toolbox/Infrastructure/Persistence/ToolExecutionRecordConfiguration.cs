using HelpDev.Modules.Toolbox.Domain;
using HelpDev.Modules.Toolbox.Domain.Execution;
using HelpDev.Modules.Toolbox.Domain.Tools;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDev.Modules.Toolbox.Infrastructure.Persistence;

public sealed class ToolExecutionRecordConfiguration : IEntityTypeConfiguration<ToolExecutionRecord>
{
    public void Configure(EntityTypeBuilder<ToolExecutionRecord> builder)
    {
        builder.ToTable("toolbox_execution_records");

        builder.HasKey(record => record.Id);

        builder.Property(record => record.Id)
            .ValueGeneratedNever();

        builder.Property(record => record.ToolId)
            .IsRequired()
            .HasColumnName("tool_id");

        builder.HasOne<ToolDefinition>()
            .WithMany()
            .HasForeignKey(record => record.ToolId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(record => record.UserId)
            .HasColumnName("user_id");

        builder.Property(record => record.ToolType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(40)
            .HasColumnName("tool_type");

        builder.Property(record => record.Succeeded)
            .IsRequired()
            .HasColumnName("succeeded");

        builder.Property(record => record.DurationMilliseconds)
            .IsRequired()
            .HasColumnName("duration_milliseconds");

        builder.Property(record => record.InputPreview)
            .HasMaxLength(ToolboxLimits.MaxHistoryInputPreview)
            .HasColumnName("input_preview");

        builder.Property(record => record.OutputPreview)
            .HasMaxLength(ToolboxLimits.MaxHistoryOutputPreview)
            .HasColumnName("output_preview");

        builder.Property(record => record.ErrorCode)
            .HasMaxLength(100)
            .HasColumnName("error_code");

        builder.Property(record => record.ExecutedAtUtc)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("executed_at_utc");

        builder.HasIndex(record => record.UserId)
            .HasDatabaseName("ix_toolbox_execution_records_user_id");

        builder.HasIndex(record => record.ToolId)
            .HasDatabaseName("ix_toolbox_execution_records_tool_id");

        builder.HasIndex(record => record.ExecutedAtUtc)
            .HasDatabaseName("ix_toolbox_execution_records_executed_at_utc");

        builder.HasIndex(record => record.Succeeded)
            .HasDatabaseName("ix_toolbox_execution_records_succeeded");
    }
}
