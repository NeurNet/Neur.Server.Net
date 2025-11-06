using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Neur.Server.Net.API.EndPoints;
using Neur.Server.Net.Infrastructure;

namespace Neur.Server.Net.API.Extensions;

public static class ApiExtensions {
    public static void AddApiAuthentication(this IServiceCollection services, JwtOptions jwtOptions) {
        Console.WriteLine("Меня реально вызвали лол");
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
                        Console.WriteLine("Чёто произошло");
                        return Task.CompletedTask;
                    }
                };
            });
        services.AddAuthorization();
    }

    public static void AddMappedEndpoints(this IEndpointRouteBuilder app) {
        app.MapUserEndPoints();
    }
}