using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Neur.Server.Net.API;
using Neur.Server.Net.API.Extensions;
using Neur.Server.Net.API.Options;
using Neur.Server.Net.API.Validators;
using Neur.Server.Net.Application.Interfaces;
using Neur.Server.Net.Application.Services;
using Neur.Server.Net.Application.Services.Options;
using Neur.Server.Net.Core.Repositories;
using Neur.Server.Net.Infrastructure;
using Neur.Server.Net.Infrastructure.Interfaces;
using Neur.Server.Net.Postgres;
using Neur.Server.Net.Postgres.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCorsPolicy(builder.Configuration.GetSection("Services").Get<ServiceOptions>());

builder.Services.AddSwaggerApi();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(nameof(JwtOptions)));
builder.Services.Configure<CollegeServiceOptions>(builder.Configuration.GetSection("Services").GetSection(
    nameof(CollegeService)));

builder.Services.AddDbContext<ApplicationDbContext>();
builder.Services.AddScoped<IUsersRepository, UsersRepository>();
builder.Services.AddScoped<IModelsRepository, ModelsRepository>();
builder.Services.AddScoped<IChatsRepository, ChatsRepository>();

builder.Services.AddScoped<HttpClient>();

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ICollegeService, CollegeService>();

builder.Services.AddScoped<IJwtProvider, JwtProvider>();

builder.Services.AddApiAuthentication(builder.Configuration.GetSection(nameof(JwtOptions)).Get<JwtOptions>());

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