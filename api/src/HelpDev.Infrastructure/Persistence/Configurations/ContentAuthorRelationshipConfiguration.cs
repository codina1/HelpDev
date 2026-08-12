using HelpDev.Modules.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ContentEntity = HelpDev.Modules.Content.Domain.Entities.Content;

namespace HelpDev.Infrastructure.Persistence.Configurations;

/// <summary>
/// Cross-module Author FK. Kept in Infrastructure so Content does not reference Identity.
/// </summary>
public class ContentAuthorRelationshipConfiguration : IEntityTypeConfiguration<ContentEntity>
{
    public void Configure(EntityTypeBuilder<ContentEntity> builder)
    {
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(content => content.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
