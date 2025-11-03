namespace Neur.Server.Net.Core.Entities;

public class UserEntity {
    public Guid Id { get; init; }
    public string LdapId { get; init; }
    public string Name { get; set; }
    public string Surname { get; set; }

    private UserEntity() {}

    private UserEntity(Guid id, string ldapId, string name, string surname) {
        Id = id;
        LdapId = ldapId;
        Name = name;
        Surname = surname;
    }

    public static UserEntity Create(Guid id, string ldapId, string name, string surname) {
        return new UserEntity(
            id: id,
            ldapId: ldapId,
            name: name,
            surname: surname
        );
    }
}