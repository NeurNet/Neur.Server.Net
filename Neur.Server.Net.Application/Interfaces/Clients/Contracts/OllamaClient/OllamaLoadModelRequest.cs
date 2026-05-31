namespace Neur.Server.Net.Application.Interfaces.Clients.Contracts.OllamaClient;

public record OllamaLoadModelRequest(
    string name,
    bool stream
);