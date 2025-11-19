namespace Neur.Server.Net.Application.Services.Contracts.OllamaService;

public record GenerateResponse(
    DateTime StartedAt,
    DateTime FinishedAt,
    string Prompt,
    string Result,
    string Model
);