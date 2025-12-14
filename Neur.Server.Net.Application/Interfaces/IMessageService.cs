using Neur.Server.Net.Core.Data;
using Neur.Server.Net.Core.Entities;

namespace Neur.Server.Net.Application.Services;

public interface IMessageService {
    Task<Guid> SaveMessageAsync(ChatEntity chat, MessageRole role, string content);
}