using Neur.Server.Net.Core.Abstractions;
using Neur.Server.Net.Core.Data;

namespace Neur.Server.Net.Core.Entities;

public class SettingsEntity : Entity {
    public SettingName Name { get; set; }
    public string Content { get; set; } = string.Empty;

    private SettingsEntity() {}

    public SettingsEntity(SettingName name, string content) {
        Id = Guid.NewGuid();
        Name = name;
        Content = content;
    }
}
