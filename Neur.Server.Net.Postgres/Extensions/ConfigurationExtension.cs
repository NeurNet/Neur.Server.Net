using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Neur.Server.Net.Core.Repositories;
using Neur.Server.Net.Postgres.Repositories;

namespace Neur.Server.Net.Postgres.Extensions;

public static class ConfigurationExtension {
    public static void AddDatabaseConfiguration(this IServiceCollection services) {
        services.AddDbContext<ApplicationDbContext>();
        services.AddScoped<IUsersRepository, UsersRepository>();
        services.AddScoped<IModelsRepository, ModelsRepository>();
        services.AddScoped<IChatsRepository, ChatsRepository>();
        services.AddScoped<IGenerationRequestsRepository, GenerationRequestsRepository>();
        services.AddScoped<IMessagesRepository, MessagesRepository>();
    }
}