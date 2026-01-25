using Neur.Server.Net.Core.Data;

namespace Neur.Server.Net.Infrastructure.Extensions;

public static class UserRoleExtensions {
    public static string ToClaimValue(this UserRole role) => role switch {
        UserRole.Student => "student",
        UserRole.Teacher => "teacher",
        UserRole.Admin => "admin",
        _ => throw new NotImplementedException()
    };

    // public static UserRole ToRole(this string role) => role switch {
    //     "student" => UserRole.Student,
    //     "teacher" => UserRole.Teacher,
    //     "admin" => UserRole.Admin,
    //     _ => throw new NotImplementedException()
    // };
}