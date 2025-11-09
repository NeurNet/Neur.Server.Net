namespace Neur.Server.Net.API.Contracts.Users;
/// <summary>
/// Ответ от сервера при аутентификации
/// </summary>
/// <param name="id">Id пользователя</param>
/// <param name="username">Имя пользователя</param>
/// <param name="tokens">Токены</param>
public record UserAuthResponse(
    string id,
    string username,
    string tokens
);