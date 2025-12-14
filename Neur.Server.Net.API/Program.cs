using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Neur.Server.Net.API;
using Neur.Server.Net.API.Extensions;
using Neur.Server.Net.API.Options;
using Neur.Server.Net.API.Validators;
using Neur.Server.Net.Application.Clients;
using Neur.Server.Net.Application.Clients.Options;
using Neur.Server.Net.Application.Interfaces;
using Neur.Server.Net.Application.Services;
using Neur.Server.Net.Application.Services.Background;
using Neur.Server.Net.Core.Repositories;
using Neur.Server.Net.Infrastructure;
using Neur.Server.Net.Infrastructure.Interfaces;
using Neur.Server.Net.Postgres;
using Neur.Server.Net.Postgres.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCorsPolicy(builder.Configuration.GetSection("Services").Get<ServiceOptions>());
builder.Services.AddDbContext<ApplicationDbContext>();
builder.Services.AddSingleton<GenerationService>();
builder.Services.AddSingleton<GenerationQueueService>();
builder.Services.AddHostedService<GenerationService>();
builder.Services.AddSwaggerApi();
builder.Services.AddHttpClient();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(nameof(JwtOptions)));
builder.Services.Configure<CollegeClientOptions>(builder.Configuration.GetSection("Services").GetSection(
    nameof(CollegeClient)));
builder.Services.Configure<OllamaClientOptions>(builder.Configuration.GetSection("Services").GetSection(nameof(OllamaClient)));

builder.Services.AddSingleton<OllamaClient>();
builder.Services.AddSingleton<ICollegeClient, CollegeClient>();

builder.Services.AddScoped<IUsersRepository, UsersRepository>();
builder.Services.AddScoped<IModelsRepository, ModelsRepository>();
builder.Services.AddScoped<IChatsRepository, ChatsRepository>();
builder.Services.AddScoped<IGenerationRequestsRepository, GenerationRequestsRepository>();
builder.Services.AddScoped<IMessagesRepository, MessagesRepository>();

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IModelService, ModelService>();
builder.Services.AddScoped<GenerationRequestService>();
builder.Services.AddScoped<MessageService>();

builder.Services.AddScoped<IJwtProvider, JwtProvider>();

builder.Services.AddApiAuthentication(builder.Configuration.GetSection(nameof(JwtOptions)).Get<JwtOptions>());


builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumMemberConverter());
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumMemberConverter());
});

var app = builder.Build();

// if (app.Environment.IsDevelopment()) {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.AddMappedEndpoints();

// Миграции для базы данных
using (var scope = app.Services.CreateScope()) {
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}
app.Run();