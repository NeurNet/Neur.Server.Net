using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Neur.Server.Net.Core.Interfaces;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ModelStatus {
    [EnumMember(Value = "open")]
    open,
    [EnumMember(Value = "locked")]
    locked
}