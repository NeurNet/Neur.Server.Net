using Neur.Server.Net.Core.Data;

namespace Neur.Server.Net.API.Contracts.Settings;

public record SetSettingsRequest(SettingName Name, string Content);
