using Neur.Server.Net.Core.Interfaces;

namespace Neur.Server.Net.Core.Entities;

public class UserEntity {
    public Guid Id { get; init; }
    public string Username { get; init; }
    public string Name { get; init; }
    public string Surname { get; init; }
    public Role Role { get; init; }
    
    public int Tokens { get; set; }
    private UserEntity() {}

    private UserEntity(Guid id, string username, string name, string surname, Role role, int tokens) {
        Id = id;
        Username = username;
        Name = name;
        Surname = surname;
        Role = role;
        Tokens = tokens;
    }

    public static UserEntity Create(Guid id, string username, string name, string surname, Role role, int tokens) {
        return new UserEntity(
            id: id,
            username: username,
            name: name,
            surname: surname,
            role: role,
            tokens: tokens
        );
    }
}