using System.Text;
using Neur.Server.Net.Core.Entities;

namespace Neur.Server.Net.Infrastructure;

public class ContextManager {
    private StringBuilder _sb;
    private readonly int _maxLength;
    
    public ContextManager(int maxLength) {
        _sb = new StringBuilder();
        _maxLength = maxLength;
    }

    private string NormalizeMessage(string message) {
        var normalized = string.Empty;
        var tokens = message.Split("\n");
        
        bool isThink = false;

        foreach (var token in tokens) {
            if (token == "<think>") {
                isThink = true;
            }
            else if (token == "</think>") {
                isThink = false;
                continue;
            }

            if (!isThink) {
                normalized += token;
            }
        }
        
        return normalized.Trim();
    }

    public void AddBaseContext(string context) {
        _sb.AppendLine("SYSTEM:");
        _sb.AppendLine(context);
    }

    public void AddChatHistory(List<MessageEntity> messages) {
        if (messages.Count > 1) {
            _sb.AppendLine("CHAT HISTORY:");
            var messageBuilder = new StringBuilder();
        
            for (int i = messages.Count-2; i > 0; i--) {
                var message = $"[MESSAGE %{i}] {messages[i].Role.ToString()}: {NormalizeMessage(messages[i].Content)}[/MESSAGE]";
                if ((_sb.Length + message.Length) <= _maxLength) {
                    messageBuilder.AppendLine(message);
                }
            }
            _sb.AppendLine(messageBuilder.ToString());
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