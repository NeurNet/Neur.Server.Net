using Neur.Server.Net.Application.DTOs;
using Neur.Server.Net.Application.Options;
using Neur.Server.Net.Core.Data;
using Neur.Server.Net.Core.Entities;

namespace Neur.Server.Net.Application.Extensions;

public static class SettingsEntityExtensions {
    public static SettingsContent GetContent(this SettingsEntity entity) => entity.Name switch {
        SettingName.Ollama => entity.GetContent<OllamaSettingsContent>(),
        SettingName.Auth   => entity.GetContent<AuthSettingsContent>(),
        _                  => throw new InvalidOperationException($"Unknown setting: {entity.Name}")
    };

    public static SettingsDto ToDto(this SettingsEntity entity) {
        switch (entity.Name) {
            case SettingName.Ollama: {
                var c = entity.GetContent<OllamaSettingsContent>();
                return new SettingsDto(entity.Name, c.Url, null);
            }
            case SettingName.Auth: {
                var c = entity.GetContent<AuthSettingsContent>();
                return new SettingsDto(entity.Name, c.Url, c.TimeoutSeconds);
            }
            default: throw new InvalidOperationException($"Unknown setting: {entity.Name}");
        }
    }

    public static SettingsContent CreateContent(SettingName name, string baseUrl, int? timeout) => name switch {
        SettingName.Ollama => new OllamaSettingsContent { Url = baseUrl },
        SettingName.Auth   => new AuthSettingsContent   { Url = baseUrl, TimeoutSeconds = timeout ?? 5 },
        _                  => throw new ArgumentOutOfRangeException(nameof(name))
    };
}
