using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Neur.Server.Net.API.Extensions;
using Neur.Server.Net.API.Middleware;
using Neur.Server.Net.API.Options;
using Neur.Server.Net.Application.Interfaces;
using Neur.Server.Net.Application.Interfaces.Clients;
using Neur.Server.Net.Application.Interfaces.Services;
using Neur.Server.Net.Application.Options;
using Neur.Server.Net.Application.Services;
using Neur.Server.Net.Application.Services.Background;
using Neur.Server.Net.Infrastructure;
using Neur.Server.Net.Infrastructure.Clients;
using Neur.Server.Net.Infrastructure.Interfaces;
using Neur.Server.Net.Infrastructure.Services;
using Neur.Server.Net.Postgres;
using Neur.Server.Net.Postgres.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.AddLogging();

// Configuration

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(nameof(JwtOptions)));
builder.Services.Configure<SettingsOptions>(builder.Configuration.GetSection("Services"));
builder.Services.ConfigureHttpJsonOptions(options => {
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.Converters.Add(new JsonStringEnumMemberConverter(JsonNamingPolicy.CamelCase));
});


builder.Services.AddCorsPolicy(builder.Configuration.Get<ServiceOptions>());
builder.Services.AddDatabaseConfiguration();

builder.Services.AddSingleton<GenerationService>();
builder.Services.AddSingleton<IGenerationService>(sp => sp.GetRequiredService<GenerationService>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<GenerationService>());
builder.Services.AddSingleton<GenerationQueueService>();
builder.Services.AddSwaggerApi();

builder.Services.AddSingleton<ICollegeClient, CollegeClient>();
builder.Services.AddSingleton<IOllamaClient, OllamaClient>();
builder.Services.AddHttpClient();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IModelService, ModelService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IOllamaService, OllamaService>();
builder.Services.AddScoped<GenerationRequestService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();

builder.Services.AddScoped<IJwtProvider, JwtProvider>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddApiAuthentication(builder.Configuration.GetSection(nameof(JwtOptions)).Get<JwtOptions>());

var app = builder.Build();
app.UseSerilogRequestLogging();

app.UseMiddleware<MiddlewareHandler>();

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<LoggingMiddleware>();

app.AddMappedEndpoints();

// Миграции для базы данных
using (var scope = app.Services.CreateScope()) {
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    var settingsService = scope.ServiceProvider.GetRequiredService<ISettingsService>();
    await settingsService.InitAsync();
}

app.Run();