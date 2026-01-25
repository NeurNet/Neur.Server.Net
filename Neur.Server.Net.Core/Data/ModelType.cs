using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Neur.Server.Net.Core.Records;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ModelType {
    [EnumMember(Value = "text")]
    text,
    [EnumMember(Value = "code")]
    code,
    [EnumMember(Value = "image")]
    image
}