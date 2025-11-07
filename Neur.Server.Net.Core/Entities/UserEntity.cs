using Neur.Server.Net.Core.Interfaces;

namespace Neur.Server.Net.Core.Entities;

public class UserEntity {
    public Guid Id { get; init; }
    public string Username { get; init; }
    public string Name { get; init; }
    public string Surname { get; init; }
    public UserRole UserRole { get; init; }
    
    public int Tokens { get; set; }
    private UserEntity() {}

    private UserEntity(Guid id, string username, string name, string surname, UserRole userRole, int tokens) {
        Id = id;
        Username = username;
        Name = name;
        Surname = surname;
        UserRole = userRole;
        Tokens = tokens;
    }

    public static UserEntity Create(Guid id, string username, string name, string surname, UserRole userRole, int tokens) {
        return new UserEntity(
            id: id,
            username: username,
            name: name,
            surname: surname,
            userRole: userRole,
            tokens: tokens
        );
    }
}