using Neur.Server.Net.Core.Entities;

namespace Neur.Server.Net.Infrastructure;

public interface IContextManager {
    void AddBaseContext(string context);
    void AddChatHistory(List<MessageEntity> messages);
    void AddCurrentPrompt(string prompt);
    string GetContext();
}