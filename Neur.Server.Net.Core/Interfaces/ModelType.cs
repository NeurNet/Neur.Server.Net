using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Neur.Server.Net.Core.Interfaces;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ModelType {
    [EnumMember(Value = "text")]
    text,
    [EnumMember(Value = "code")]
    code,
    [EnumMember(Value = "image")]
    image
}