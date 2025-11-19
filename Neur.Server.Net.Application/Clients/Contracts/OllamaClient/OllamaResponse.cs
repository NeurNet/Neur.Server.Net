namespace Neur.Server.Net.Application.Services.Contracts.OllamaService;

public record OllamaResponse (
    string model,
    string response,
    bool done
);