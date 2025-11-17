using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neur.Server.Net.Core.Entities;

namespace Neur.Server.Net.Postgres.Configurations;

public class RequestConfiguration : IEntityTypeConfiguration<RequestEntity> {
    public void Configure(EntityTypeBuilder<RequestEntity> builder) {
        builder.ToTable("Requests");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.HasIndex(x => x.CreatedAt);
        builder.Property(x => x.StartedAt);
        builder.Property(x => x.Prompt).HasMaxLength(3000).IsRequired();
        builder.Property(x => x.Response);
        builder.Property(x => x.FinishedAt);
    }
}