namespace Neur.Server.Net.API.Contracts.Settings;

public record SetSettingsRequest(string BaseUrl, int? Timeout);
