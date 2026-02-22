using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Postgres.Configurations;

namespace Neur.Server.Net.Postgres;

public class ApplicationDbContext : DbContext {
    private readonly IConfiguration _configuration;
    
    public ApplicationDbContext(IConfiguration configuration) {
        _configuration = configuration;
    }
    
    public virtual DbSet<UserEntity> Users { get; set; }
    public virtual DbSet<ModelEntity> Models { get; set; }
    public virtual DbSet<ChatEntity> Chats { get; set; }
    public virtual DbSet<GenerationRequestEntity> GenerationRequests { get; set; }
    public virtual DbSet<MessageEntity> Messages { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        optionsBuilder
            .UseNpgsql(_configuration.GetConnectionString("DatabaseContext"))
            .UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()))
            .EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new ModelConfiguration());
        modelBuilder.ApplyConfiguration(new ChatConfiguration());
        modelBuilder.ApplyConfiguration(new GenerationRequestConfiguration());
        modelBuilder.ApplyConfiguration(new MessageConfiguration());
    }
}