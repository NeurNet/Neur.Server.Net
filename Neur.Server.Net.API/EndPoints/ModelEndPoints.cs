using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Neur.Server.Net.API.Contracts.Models;
using Neur.Server.Net.API.Extensions;
using Neur.Server.Net.API.Validators;
using Neur.Server.Net.Application.Interfaces;
using Neur.Server.Net.Core.Data;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Records;

namespace Neur.Server.Net.API.EndPoints;

public static class ModelEndPoints {
    public static IEndpointRouteBuilder MapModelsEndPoints(this IEndpointRouteBuilder app) {
        var endpoints = app
            .MapGroup("/api/models")
            .WithTags("Models")
            .RequireAuthorization("admin");

        endpoints.MapGet(String.Empty, GetAll)
            .WithSummary("Получить список всех моделей")
            .Produces<List<GetModelResponse>>(200)
            .Produces(401);

        endpoints.MapPost(String.Empty, Add)
            .WithSummary("Создать модель")
            .Produces<CreateModelResponse>(200)
            .Produces(400)
            .Produces(401);
        
        return endpoints;
    }

    private static async Task<IResult> Add([FromBody] CreateModelReqest req, [FromServices] IModelService modelService) {
        try {
            var validator = new CreateModelRequestValidator();
            var result = validator.Validate(req);

            if (result.IsValid) {
                var model = new ModelEntity(
                    name: req.name,
                    modelName: req.model,
                    req.context ?? "",
                    type: Enum.Parse<ModelType>(req.type),
                    version: req.version ?? "0.1",
                    status: Enum.Parse<ModelStatus>(req.status),
                    createdAt: DateTime.UtcNow
                );
                var createdModel = await modelService.CreateAsync(model);
                return Results.Ok(new CreateModelResponse(createdModel.Id.ToString()));
            }

            throw new ValidationException(result.Errors[0].ErrorMessage);
        }

        catch (ValidationException e) {
            return Results.BadRequest(e.Message);
        }
    }

    private static async Task<IResult> GetAll(ClaimsPrincipal claims, [FromServices] IModelService modelService) {
        var user = claims.ToCurrentUser();
        var models = await modelService.GetAllByUserRoleAsync(user.userId);

        var result = models.Select(model => new GetModelResponse(
            id:  model.Id,
            name: model.Name,
            model: model.ModelName,
            version: model.Version,
            status: model.Status.ToString(),
            createdAt: model.CreatedAt,
            updatedAt: model.UpdatedAt
        ));
        return Results.Ok(result);
    }
}