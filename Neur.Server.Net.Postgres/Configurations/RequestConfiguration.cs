using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neur.Server.Net.Core.Entities;

namespace Neur.Server.Net.Postgres.Configurations;

public class RequestConfiguration : IEntityTypeConfiguration<RequestEntity> {
    public void Configure(EntityTypeBuilder<RequestEntity> builder) {
        builder.ToTable("Requests");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.Promt).HasMaxLength(3000).IsRequired();
        builder.Property(x => x.Response);
        builder.Property(x => x.FinishedAt);
    }
}