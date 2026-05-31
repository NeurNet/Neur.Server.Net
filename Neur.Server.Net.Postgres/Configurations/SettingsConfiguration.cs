using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neur.Server.Net.Core.Entities;

namespace Neur.Server.Net.Postgres.Configurations;

public class SettingsConfiguration : IEntityTypeConfiguration<SettingsEntity> {
    public void Configure(EntityTypeBuilder<SettingsEntity> builder) {
        builder.ToTable("Settings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Name)
            .IsRequired()
            .HasConversion<string>();
        builder.HasIndex(x => x.Name).IsUnique();
        builder.Property(x => x.Content)
            .IsRequired()
            .HasColumnType("jsonb");
    }
}
