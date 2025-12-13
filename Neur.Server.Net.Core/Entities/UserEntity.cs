using Neur.Server.Net.Core.Abstractions;
using Neur.Server.Net.Core.Data;
using Neur.Server.Net.Core.Records;

namespace Neur.Server.Net.Core.Entities;

public class UserEntity  : Entity {
    public string Username { get; init; }
    public string Name { get; init; }
    public string Surname { get; init; }
    public UserRole Role { get; init; }
    public int Tokens { get; set; }
    public ICollection<ChatEntity> Chats { get; set; }
    private UserEntity() {}

    public UserEntity(string username, string name, string surname, UserRole role, int tokens) {
        Id = Guid.NewGuid();
        Username = username;
        Name = name;
        Surname = surname;
        Role = role;
        Tokens = tokens;
    }
}