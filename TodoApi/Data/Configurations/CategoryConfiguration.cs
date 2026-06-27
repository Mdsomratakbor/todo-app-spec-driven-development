using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoApi.Models.Entities;

namespace TodoApi.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.UserId)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.HasIndex(c => new { c.Name, c.UserId }).IsUnique();
    }
}
