namespace Neur.Server.Net.Application.Options;

public class SettingsOptions {
    public OllamaSettingsOptions OllamaClient { get; set; } = new();
    public AuthSettingsOptions CollegeClient { get; set; } = new();
}

public class OllamaSettingsOptions {
    public string Url { get; set; } = string.Empty;
}

public class AuthSettingsOptions {
    public string Url { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 5;
}
