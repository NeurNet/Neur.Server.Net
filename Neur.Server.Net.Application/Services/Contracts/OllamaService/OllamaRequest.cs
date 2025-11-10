namespace Neur.Server.Net.Application.Services.Contracts.OllamaService;

public record OllamaRequest(
    string model,
    string prompt,
    bool stream
);