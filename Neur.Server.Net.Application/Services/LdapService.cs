using Neur.Server.Net.Application.Interfaces;
using Neur.Server.Net.Application.Services.Contracts;

namespace Neur.Server.Net.Application.Services;

public class LdapService : ILdapService {
    private readonly HttpClient _httpClient;
    
    public LdapService(HttpClient httpClient) {
        _httpClient = httpClient;
    }

    public async Task<LdapUser?> AuthenticateAsync(string username, string password) {
        if (username == "i24s0202" & password == "123") { // Пока нет LDAP, просто для теста
            return new LdapUser(Username: "i24s0202", Name: "Григорий", "Воробьёв");
        }
        else if (username == "i24s0208" & password == "1234") { // Пока нет LDAP, просто для теста
            return new LdapUser(Username: "i24s0208", Name: "Чувак", "Крутой");
        }
        return null;
    }
}