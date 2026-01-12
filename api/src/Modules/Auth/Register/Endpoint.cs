namespace SteadyLearn.Modules.Auth.Register;

using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SteadyLearn.Common.Models;

/// <summary>
/// API endpoint for user registration.
/// </summary>
public static class Endpoint
{
    public static void MapRegisterEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/register", async (
            RegisterCommand command,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(command, cancellationToken);

            return result.Match<IResult>(
                onSuccess: response => Results.Created(
                    $"/api/users/{response.UserId}",
                    ApiResponse<RegisterResponse>.Ok(response)),
                onFailure: error => Results.BadRequest(
                    ApiErrorResponse.FromError(error.Code, error.Message)));
        })
        .WithName("Register")
        .WithTags("Auth")
        .Produces<ApiResponse<RegisterResponse>>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Register a new user";
            operation.Description = "Creates a new user account and sends a verification email.";
            return operation;
        });
    }
}
