using HelpDev.Modules.Identity.Domain.Entities;
using HelpDev.Modules.Identity.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDev.Modules.Identity.Infrastructure.Persistence;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id)
            .ValueGeneratedNever();

        builder.Property(user => user.Mobile)
            .IsRequired()
            .HasMaxLength(15)
            .HasColumnName("mobile");

        builder.HasIndex(user => user.Mobile)
            .IsUnique();

        builder.Property(user => user.FullName)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("full_name");

        builder.Property(user => user.FirstName)
            .IsRequired()
            .HasMaxLength(100)
            .HasDefaultValue(string.Empty)
            .HasColumnName("first_name");

        builder.Property(user => user.LastName)
            .IsRequired()
            .HasMaxLength(100)
            .HasDefaultValue(string.Empty)
            .HasColumnName("last_name");

        builder.Property(user => user.Email)
            .IsRequired()
            .HasMaxLength(200)
            .HasDefaultValue(string.Empty)
            .HasColumnName("email");

        builder.Property(user => user.ProfileImageUrl)
            .IsRequired()
            .HasMaxLength(500)
            .HasDefaultValue(string.Empty)
            .HasColumnName("profile_image_url");

        builder.Property(user => user.Expertise)
            .IsRequired()
            .HasMaxLength(200)
            .HasDefaultValue(string.Empty)
            .HasColumnName("expertise");

        builder.Property(user => user.Interests)
            .IsRequired()
            .HasMaxLength(500)
            .HasDefaultValue(string.Empty)
            .HasColumnName("interests");

        builder.Property(user => user.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(UserRole.User)
            .HasColumnName("role");

        builder.Property(user => user.Stack)
            .IsRequired()
            .HasMaxLength(500)
            .HasDefaultValue(string.Empty)
            .HasColumnName("stack");

        builder.Property(user => user.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(user => user.LastLogin)
            .IsRequired(false)
            .HasColumnName("last_login");
    }
}
