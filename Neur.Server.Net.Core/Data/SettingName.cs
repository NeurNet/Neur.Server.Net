using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Neur.Server.Net.Core.Data;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SettingName {
    [EnumMember(Value = "ollama")]
    Ollama = 0,
    [EnumMember(Value = "auth")]
    Auth = 1
}
