using Neur.Server.Net.Core.Entities;

namespace Neur.Server.Net.Application.Options;

public class OllamaSettingsContent : SettingsContent {
    public string Url { get; set; } = string.Empty;
}

public class AuthSettingsContent : SettingsContent {
    public string Url { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 5;
}

public class SettingsOptions {
    public OllamaSettingsContent OllamaClient { get; set; } = new();
    public AuthSettingsContent CollegeClient { get; set; } = new();
}
