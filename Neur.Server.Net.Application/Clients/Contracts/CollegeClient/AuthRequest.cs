namespace Neur.Server.Net.Application.Services.Contracts;

public record AuthRequest ( 
    string username,
    string password
);