namespace Neur.Server.Net.Core.Entities;

public class UserEntity {
    public Guid Id { get; init; }
    public string Username { get; init; }
    public string Name { get; set; }
    public string Surname { get; set; }

    private UserEntity() {}

    private UserEntity(Guid id, string username, string name, string surname) {
        Id = id;
        Username = username;
        Name = name;
        Surname = surname;
    }

    public static UserEntity Create(Guid id, string username, string name, string surname) {
        return new UserEntity(
            id: id,
            username: username,
            name: name,
            surname: surname
        );
    }
}