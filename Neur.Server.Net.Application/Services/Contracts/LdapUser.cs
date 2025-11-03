namespace Neur.Server.Net.Application.Services.Contracts;

public record LdapUser(
    string Username,
    string Name,
    string Surname
);