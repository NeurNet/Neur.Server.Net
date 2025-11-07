using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Neur.Server.Net.API.EndPoints;
using Neur.Server.Net.API.Options;
using Neur.Server.Net.Infrastructure;

namespace Neur.Server.Net.API.Extensions;

public static class ApiExtensions {
    public static void AddApiAuthentication(this IServiceCollection services, JwtOptions jwtOptions) {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options => {
                options.TokenValidationParameters = new TokenValidationParameters() {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey))
                };
                options.Events = new JwtBearerEvents {
                    OnMessageReceived = context => {
                        context.Token = context.Request.Cookies["auth_token"];
                        return Task.CompletedTask;
                    }
                };
            });
        services.AddAuthorization();
    }

    public static void AddCorsPolicy(this IServiceCollection services, ServiceOptions serviceOptions) {
        Console.WriteLine("Frontend: " + serviceOptions.Frontend.url);
        services.AddCors(options => {
            options.AddPolicy("CorsPolicy", policy => {
                policy.WithOrigins(serviceOptions.Frontend.url)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }

    public static void AddMappedEndpoints(this IEndpointRouteBuilder app) {
        app.MapUserEndPoints();
    }
}