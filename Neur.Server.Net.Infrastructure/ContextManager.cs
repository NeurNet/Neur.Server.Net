using System.Text;
using Neur.Server.Net.Core.Entities;

namespace Neur.Server.Net.Infrastructure;

public class ContextManager {
    private StringBuilder _sb;
    
    public ContextManager() {
        _sb = new StringBuilder();
    }

    public void AddBaseContext(string context) {
        _sb.AppendLine("SYSTEM:");
        _sb.AppendLine(context);
    }

    public void AddChatHistory(List<MessageEntity> messages) {
        _sb.AppendLine("CHAT HISTORY:");
        foreach (var message in messages) {
            _sb.AppendLine($"- {message.Role.ToString()}: {message.Content}");
        }
    }

    public void AddCurrentPrompt(string prompt) {
        _sb.AppendLine("CURRENT INPUT:");
        _sb.AppendLine(prompt);
    }

    public string GetContext() {
        return _sb.ToString();
    }
}