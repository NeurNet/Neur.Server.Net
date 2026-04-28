using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Neur.Server.Net.API.Contracts.Chats;

public record GenerateRequest(
    [property: JsonPropertyName("conversation_id")] Guid? ConversationId,
    [property: JsonPropertyName("model_id")] Guid? ModelId,
    [property: JsonPropertyName("message")] [Required] string Message 
);