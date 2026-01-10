using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Neur.Server.Net.Application.Clients.Options;
using Neur.Server.Net.Infrastructure.Clients.Contracts.CollegeClient;
using Neur.Server.Net.Infrastructure.Interfaces;

namespace Neur.Server.Net.Infrastructure.Clients;

public class CollegeClient : ICollegeClient {
    /// <summary>
    /// The college client to send request to college API 
    /// </summary>
    private readonly HttpClient _httpClient;
    private readonly CollegeClientOptions _options;
    
    /// <summary>
    /// Initializes a new <see cref="CollegeClient"/> class
    /// </summary>
    /// <param name="httpClient">The HTTP client for API requests</param>
    /// <param name="options">Configuration options for the client</param>
    public CollegeClient(HttpClient httpClient, IOptions<CollegeClientOptions> options) {
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
    
    /// <summary>
    /// Method to send authenticate request
    /// </summary>
    /// <param name="username">The user's login name</param>
    /// <param name="password">The user's password</param>
    /// <returns>User data if authentication success</returns>
    public async Task<AuthUserResponse?> AuthenticateAsync(string username, string password) {
        var userResponse = await SendAuthRequest(username, password);
        if (userResponse != null) {
            return userResponse;
        }
        return null;
    }
}