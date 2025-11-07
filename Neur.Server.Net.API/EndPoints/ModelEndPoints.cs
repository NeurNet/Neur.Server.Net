using System.ComponentModel.DataAnnotations;
using Neur.Server.Net.API.Contracts.Models;
using Neur.Server.Net.API.Validators;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Interfaces;
using Neur.Server.Net.Core.Repositories;
using Neur.Server.Net.Postgres.Repositories;

namespace Neur.Server.Net.API.EndPoints;

public static class ModelEndPoints {
    public static IEndpointRouteBuilder MapModelsEndPoints(this IEndpointRouteBuilder app) {
        var endpoints = app.MapGroup("/api/models").WithTags("Models");

        endpoints.MapGet(String.Empty, GetAll)
            .WithSummary("Получить список всех моделей")
            .Produces<List<ModelEntity>>(200)
            .Produces(401)
            .RequireAuthorization();
        
        endpoints.MapPost(String.Empty, Add)
            .WithSummary("Создать модель")
            .Produces<CreateModelResponse>(200)
            .Produces(400)
            .Produces(401)
            .RequireAuthorization();
        
        return endpoints;
    }

    private static async Task<IResult> Add(CreateModelReqest req, IModelsRepository repository) {
        try {
            var validator = new CreateModelRequestValidator();
            var result = validator.Validate(req);

            if (result.IsValid) {
                var model = ModelEntity.Create(
                    id: Guid.NewGuid(),
                    name: req.name,
                    type: Enum.Parse<ModelType>(req.type),
                    version: req.version ?? "0.1",
                    status: Enum.Parse<ModelStatus>(req.status),
                    createdAt: DateTime.UtcNow
                );
                await repository.Add(model);
                return Results.Ok(new CreateModelResponse(model.Id.ToString()));   
            }

            throw new ValidationException(result.Errors[0].ErrorMessage);
        }

        catch (ValidationException e) {
            return Results.BadRequest(e.Message);
        }
    }

    private static async Task<IResult> GetAll(IModelsRepository modelsRepository) {
        var models = await modelsRepository.GetAll();
        return Results.Ok(models);
    }
}