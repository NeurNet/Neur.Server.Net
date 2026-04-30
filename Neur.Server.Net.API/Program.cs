using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Neur.Server.Net.API.Extensions;
using Neur.Server.Net.API.Middleware;
using Neur.Server.Net.API.Options;
using Neur.Server.Net.Application.Clients.Options;
using Neur.Server.Net.Application.Interfaces;
using Neur.Server.Net.Application.Interfaces.Clients;
using Neur.Server.Net.Application.Interfaces.Services;
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
builder.Services.Configure<CollegeClientOptions>(builder.Configuration.GetSection("Services").GetSection(
    nameof(CollegeClient)));
builder.Services.Configure<OllamaClientOptions>(builder.Configuration.GetSection("Services").GetSection(nameof(OllamaClient)));

var collegeClientOptions = builder.Configuration.GetSection("Services").GetSection(nameof(CollegeClient))
    .Get<CollegeClientOptions>()!;

builder.Services.ConfigureHttpJsonOptions(options => {
    options.SerializerOptions.DefaultIgnoreCondition = 
        JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.Converters.Add(new JsonStringEnumMemberConverter());
});


builder.Services.AddCorsPolicy(builder.Configuration.GetSection("Services").Get<ServiceOptions>());
builder.Services.AddDatabaseConfiguration();

builder.Services.AddSingleton<GenerationService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<GenerationService>());
builder.Services.AddSingleton<GenerationQueueService>();
builder.Services.AddSwaggerApi();

builder.Services.AddHttpClient<ICollegeClient, CollegeClient>(client => {
    client.Timeout = TimeSpan.FromSeconds(collegeClientOptions.TimeoutSeconds);
});

builder.Services.AddSingleton<IOllamaClient, OllamaClient>();
builder.Services.AddHttpClient();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IModelService, ModelService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IOllamaService, OllamaService>();
builder.Services.AddScoped<GenerationRequestService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

builder.Services.AddScoped<IJwtProvider, JwtProvider>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddApiAuthentication(builder.Configuration.GetSection(nameof(JwtOptions)).Get<JwtOptions>());

var app = builder.Build();
app.UseSerilogRequestLogging();

app.UseMiddleware<MiddlewareHandler>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<LoggingMiddleware>();

app.AddMappedEndpoints();

// Миграции для базы данных
using (var scope = app.Services.CreateScope()) {
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.Run();