using Neur.Server.Net.Core.Data;

namespace Neur.Server.Net.Application.DTOs;

public record SettingsDto(SettingName Name, string BaseUrl, int? Timeout);
