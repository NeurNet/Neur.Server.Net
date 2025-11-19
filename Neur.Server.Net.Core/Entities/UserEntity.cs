using Neur.Server.Net.Core.Data;
using Neur.Server.Net.Core.Records;

namespace Neur.Server.Net.Core.Entities;

public class UserEntity {
    public Guid Id { get; init; }
    public string Username { get; init; }
    public string Name { get; init; }
    public string Surname { get; init; }
    public UserRole Role { get; init; }
    public int Tokens { get; private set; }
    public ICollection<ChatEntity> Chats { get; set; }
    private UserEntity() {}

    private UserEntity(Guid id, string username, string name, string surname, UserRole role, int tokens) {
        Id = id;
        Username = username;
        Name = name;
        Surname = surname;
        Role = role;
        Tokens = tokens;
    }

    public static UserEntity Create(Guid id, string username, string name, string surname, UserRole userRole, int tokens) {
        return new UserEntity(
            id: id,
            username: username,
            name: name,
            surname: surname,
            role: userRole,
            tokens: tokens
        );
    }
    
    public void ConsumeTokens(int count) {
        if (count <= 0) {
            throw new InvalidOperationException("Cannot consume a token with a neuter amount");
        }
        if (Tokens < count) {
            throw new InvalidOperationException("Tokens must be greater than or equal to count");
        }
        Tokens -= count;
    }

    public void AddTokens(int count) {
        if (count <= 0) {
            throw new InvalidOperationException("Cannot add a token with a negative or neuter amount");
        }
        Tokens += count;
    }
}