using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Neur.Server.Net.Core.Data;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MessageRole {
    [EnumMember(Value = "user")]
    User,
    [EnumMember(Value = "assistant")]
    Assistant
}