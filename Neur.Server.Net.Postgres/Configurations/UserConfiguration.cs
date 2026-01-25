using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neur.Server.Net.Core.Entities;

namespace Neur.Server.Net.Postgres.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<UserEntity> {
    public void Configure(EntityTypeBuilder<UserEntity> builder) {
        builder.ToTable("Users");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Username).IsRequired().ValueGeneratedNever();
        builder.Property(x => x.Name).HasMaxLength(255);
        builder.Property(x => x.Surname).HasMaxLength(255);
        builder
            .HasMany(x => x.Chats)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}