using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neur.Server.Net.Core.Entities;

namespace Neur.Server.Net.Postgres.Configurations;

public class GenerationRequestConfiguration : IEntityTypeConfiguration<GenerationRequestEntity> {
    public void Configure(EntityTypeBuilder<GenerationRequestEntity> builder) {
        builder.ToTable("GenerationRequests");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.TokenCost);
        builder
            .HasOne(x => x.Model)
            .WithMany()
            .HasForeignKey(x => x.ModelId);
        builder
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId);
        builder.HasIndex(x => x.CreatedAt);
        builder.Property(x => x.StartedAt);
        builder.Property(x => x.FinishedAt);
    }
}