using System.Text;
using System.Text.Json;
using System.Net.Http;
using Neur.Server.Net.Application.Interfaces.Clients;
using Neur.Server.Net.Application.Options;
using Neur.Server.Net.Infrastructure.Clients.Contracts.CollegeClient;
using Neur.Server.Net.Infrastructure.Interfaces;

namespace Neur.Server.Net.Infrastructure.Clients;

public class CollegeClient : ICollegeClient {
    private readonly IHttpClientFactory _httpClientFactory;
    private volatile string _url = string.Empty;
    private int _timeoutSeconds = 5;

    public CollegeClient(IHttpClientFactory httpClientFactory) {
        _httpClientFactory = httpClientFactory;
    }

    public void SetOptions(AuthSettingsContent options) {
        _url = options.Url;
        _timeoutSeconds = options.TimeoutSeconds;
    }

    private async Task<AuthUserResponse?> SendAuthRequest(string username, string password, CancellationToken cancellationToken) {
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.Timeout = TimeSpan.FromSeconds(_timeoutSeconds);

        var requestBody = JsonSerializer.Serialize(new AuthRequest(username: username, password: password));
        var stringContent = new StringContent(requestBody, Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync($"{_url}/api/v1/users/signin", stringContent, cancellationToken);
        if (response.IsSuccessStatusCode) {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<AuthResponse>(content).user;
        }

        return null;
    }

    public async Task<AuthUserResponse?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default) {
        return await SendAuthRequest(username, password, cancellationToken);
    }
}
