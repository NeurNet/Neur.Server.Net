using System.Text.Json;
using Neur.Server.Net.Core.Abstractions;
using Neur.Server.Net.Core.Data;

namespace Neur.Server.Net.Core.Entities;

public abstract class SettingsContent {
    public string Serialize() => JsonSerializer.Serialize(this, GetType());

    public static T Deserialize<T>(string json) where T : SettingsContent
        => JsonSerializer.Deserialize<T>(json)!;
}

public class SettingsEntity : Entity {
    public SettingName Name { get; set; }
    public string JsonContent { get; set; } = string.Empty;

    private SettingsEntity() {}

    public SettingsEntity(SettingName name, SettingsContent content) {
        Id = Guid.NewGuid();
        Name = name;
        JsonContent = content.Serialize();
    }

    public T GetContent<T>() where T : SettingsContent
        => SettingsContent.Deserialize<T>(JsonContent);

    public void SetContent(SettingsContent content)
        => JsonContent = content.Serialize();
}
