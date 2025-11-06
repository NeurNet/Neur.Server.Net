using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Neur.Server.Net.API.Extensions;
using Neur.Server.Net.Application.Interfaces;
using Neur.Server.Net.Application.Services;
using Neur.Server.Net.Core.Repositories;
using Neur.Server.Net.Infrastructure;
using Neur.Server.Net.Infrastructure.Interfaces;
using Neur.Server.Net.Postgres;
using Neur.Server.Net.Postgres.Repositories;

var CorsPolicy = "CorsPolicy";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy,
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                    .WithHeaders(HeaderNames.ContentType)
                    .AllowCredentials();
        });
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(nameof(JwtOptions)));
builder.Services.Configure<CollegeServiceOptions>(builder.Configuration.GetSection(nameof(CollegeServiceOptions)));

builder.Services.AddDbContext<ApplicationDbContext>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<HttpClient>();

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ICollegeService, CollegeService>();

builder.Services.AddScoped<IJwtProvider, JwtProvider>();

builder.Services.AddSwaggerGen();

builder.Services.AddApiAuthentication(builder.Configuration.GetSection(nameof(JwtOptions)).Get<JwtOptions>());

var app = builder.Build();

// if (app.Environment.IsDevelopment()) {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(CorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.AddMappedEndpoints();

// Миграции для базы данных
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.Run();