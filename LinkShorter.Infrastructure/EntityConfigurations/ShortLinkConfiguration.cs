using LinkShorter.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkShorter.Infrastructure.EntityConfigurations;

public class ShortLinkConfiguration : IEntityTypeConfiguration<ShortLink>
{
    public void Configure(EntityTypeBuilder<ShortLink> builder)
    {
        builder
            .HasIndex(c => c.Token)
            .IsUnique();
        
        builder
            .HasIndex(c => c.Url)
            .IsUnique();
    }
}