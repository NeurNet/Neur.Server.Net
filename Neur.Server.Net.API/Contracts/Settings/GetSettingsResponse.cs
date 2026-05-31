using Neur.Server.Net.Core.Data;

namespace Neur.Server.Net.API.Contracts.Settings;

public record GetSettingsResponse(SettingName Name, string Content);
