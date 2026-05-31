using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Neur.Server.Net.API.Contracts.Models;
using Neur.Server.Net.API.Extensions;
using Neur.Server.Net.API.Validators;
using Neur.Server.Net.Application.Exceptions;
using Neur.Server.Net.Application.Interfaces;
using Neur.Server.Net.Application.Interfaces.Services;
using Neur.Server.Net.Core.Data;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Records;

namespace Neur.Server.Net.API.EndPoints;

public static class ModelEndPoints {
    public static IEndpointRouteBuilder MapModelsEndPoints(this IEndpointRouteBuilder app) {
        var endpoints = app
            .MapGroup("/api/models")
            .ProducesProblem(401)
            .WithTags("Models");

        endpoints.MapGet(String.Empty, GetAll)
            .WithSummary("Получить список всех моделей")
            .Produces<List<GetModelResponse>>(200)
            .RequireAuthorization();

        endpoints.MapPost(String.Empty, Add)
            .WithSummary("Создать модель")
            .Produces<CreateModelResponse>(200)
            .Produces(400)
            .RequireAuthorization("AdminOnly");

        endpoints.MapPut("{id:guid}", Update)
            .WithSummary("Обновить модель")
            .Produces(200)
            .Produces(400)
            .Produces(404)
            .RequireAuthorization("AdminOnly");

        endpoints.MapDelete("{id:guid}", Delete)
            .WithSummary("Удалить модель")
            .Produces(200)
            .Produces(404)
            .RequireAuthorization("AdminOnly");

        return endpoints;
    }

    private static async Task<IResult> Update(Guid id, [FromBody] UpdateModelRequest req, [FromServices] IModelService modelService) {
        var validator = new UpdateModelRequestValidator();
        var result = validator.Validate(req);

        if (!result.IsValid) {
            throw new ValidateException(result.Errors[0].ErrorMessage);
        }

        var model = new ModelEntity(
            id: id,
            name: req.name,
            modelName: req.model,
            context: req.context,
            type: Enum.Parse<ModelType>(req.type),
            version: req.version,
            status: Enum.Parse<ModelStatus>(req.status),
            createdAt: DateTime.UtcNow
        );

        await modelService.UpdateAsync(model);
        return Results.Ok();
    }

    private static async Task<IResult> Delete(Guid id, [FromServices] IModelService modelService) {
        await modelService.DeleteAsync(id);
        return Results.Ok();
    }

    private static async Task<IResult> Add([FromBody] CreateModelReqest req, [FromServices] IModelService modelService) {
        var validator = new CreateModelRequestValidator();
        var result = validator.Validate(req);

        if (!result.IsValid) {
            throw new ValidateException(result.Errors[0].ErrorMessage);
        }
        
        var model = new ModelEntity(
            name: req.name,
            modelName: req.model,
            context: req.context,
            type: Enum.Parse<ModelType>(req.type),
            version: req.version,
            status: Enum.Parse<ModelStatus>(req.status),
            createdAt: DateTime.UtcNow
        );
        var createdModel = await modelService.CreateAsync(model);
        return Results.Ok(new CreateModelResponse(createdModel.Id.ToString()));
    }

    private static async Task<IResult> GetAll(ClaimsPrincipal claims, [FromServices] IModelService modelService) {
        var user = claims.ToCurrentUser();
        var models = await modelService.GetAllByUserRoleAsync(user.userId);

        var result = models.Select(model => new GetModelResponse(
            id:  model.Id,
            name: model.Name,
            model: model.ModelName,
            type: model.Type,
            version: model.Version,
            status: model.Status.ToString(),
            createdAt: model.CreatedAt,
            updatedAt: model.UpdatedAt
        ));
        return Results.Ok(result);
    }
}