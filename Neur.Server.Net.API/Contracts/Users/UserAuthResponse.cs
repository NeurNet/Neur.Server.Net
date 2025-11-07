namespace Neur.Server.Net.API.Contracts.Users;
/// <summary>
/// Ответ от сервера при аутентификации
/// </summary>
/// <param name="id">Id</param>
/// <param name="tokens">Токены</param>
public record UserAuthResponse(
    string id,
    string tokens
);