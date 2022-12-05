using LinkShorter.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkShorter.Infrastructure.EntityConfigurations;

public class BarcodeInfoConfiguration : IEntityTypeConfiguration<BarcodeInfo>
{
    public void Configure(EntityTypeBuilder<BarcodeInfo> builder)
    {
        builder
            .HasIndex(c => c.Path)
            .IsUnique();

        builder
            .HasOne<ShortLink>()
            .WithOne()
            .HasForeignKey<BarcodeInfo>(c => c.ShortLinkId);
    }
}