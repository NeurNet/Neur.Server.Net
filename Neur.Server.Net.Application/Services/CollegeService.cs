using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Neur.Server.Net.Application.Interfaces;
using Neur.Server.Net.Application.Services.Contracts;

namespace Neur.Server.Net.Application.Services;

public class CollegeService : ICollegeService {
    private readonly HttpClient _httpClient;
    private readonly CollegeServiceOptions _options;
    
    public CollegeService(HttpClient httpClient, IOptions<CollegeServiceOptions> options) {
        _httpClient = httpClient;
        _options = options.Value;
    }

    private async Task<AuthUserResponse?> SendAuthRequest(string username, string password) {
        var requestBody = JsonSerializer.Serialize(new AuthRequest(
            username: username,
            password: password
        ));
        var stringContent = new StringContent(requestBody, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{_options.url}/api/v1/users/signin", stringContent);
        if (response.IsSuccessStatusCode) {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<AuthResponse>(content).user;
        }
        
        return null;
    }

    public async Task<AuthUserResponse?> AuthenticateAsync(string username, string password) {
        var userResponse = await SendAuthRequest(username, password);
        if (userResponse != null) {
            return userResponse;
        }
        return null;
    }
}