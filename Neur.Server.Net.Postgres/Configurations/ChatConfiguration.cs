using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neur.Server.Net.Core.Entities;

namespace Neur.Server.Net.Postgres.Configurations;

public class ChatConfiguration : IEntityTypeConfiguration<ChatEntity> {
    public void Configure(EntityTypeBuilder<ChatEntity> builder) {
        builder.ToTable("Chats");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.HasOne(x => x.Model).WithMany().HasForeignKey(x => x.ModelId).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt);
    }
}