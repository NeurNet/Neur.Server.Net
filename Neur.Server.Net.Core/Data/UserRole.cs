using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Neur.Server.Net.Core.Data;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserRole {
    [EnumMember(Value = "student")]
    Student = 0,
    [EnumMember(Value = "teacher")]
    Teacher = 1,
    [EnumMember(Value = "admin")]
    Admin = 2
}