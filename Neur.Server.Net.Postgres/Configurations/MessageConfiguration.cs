using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neur.Server.Net.Core.Entities;

namespace Neur.Server.Net.Postgres.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<MessageEntity> {
    public void Configure(EntityTypeBuilder<MessageEntity> builder) {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder
            .HasOne(x => x.Chat)
            .WithMany()
            .HasForeignKey(x => x.ChatId);
        builder.Property(x => x.Content).IsRequired();
        builder.Property(x => x.Role).IsRequired();
        builder.HasIndex(x => x.CreatedAt);
    }
}