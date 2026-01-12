namespace SteadyLearn.Modules.Auth.ResetPassword;

using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SteadyLearn.Common.Models;

/// <summary>
/// API endpoints for password reset.
/// </summary>
public static class Endpoint
{
    public static void MapResetPasswordEndpoints(this IEndpointRouteBuilder app)
    {
        // Request password reset
        app.MapPost("/api/auth/forgot-password", async (
            RequestPasswordResetCommand command,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(command, cancellationToken);

            return result.Match<IResult>(
                onSuccess: response => Results.Ok(
                    ApiResponse<RequestPasswordResetResponse>.Ok(response)),
                onFailure: error => Results.BadRequest(
                    ApiErrorResponse.FromError(error.Code, error.Message)));
        })
        .WithName("ForgotPassword")
        .WithTags("Auth")
        .Produces<ApiResponse<RequestPasswordResetResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Request password reset";
            operation.Description = "Sends a password reset link to the user's email if the account exists.";
            return operation;
        });

        // Complete password reset
        app.MapPost("/api/auth/reset-password", async (
            CompletePasswordResetCommand command,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(command, cancellationToken);

            return result.Match<IResult>(
                onSuccess: response => Results.Ok(
                    ApiResponse<CompletePasswordResetResponse>.Ok(response)),
                onFailure: error => error.Code switch
                {
                    "TOKEN_EXPIRED" => Results.Json(
                        ApiErrorResponse.FromError(error.Code, error.Message),
                        statusCode: StatusCodes.Status410Gone),
                    _ => Results.BadRequest(
                        ApiErrorResponse.FromError(error.Code, error.Message))
                });
        })
        .WithName("ResetPassword")
        .WithTags("Auth")
        .Produces<ApiResponse<CompletePasswordResetResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status410Gone)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Reset password";
            operation.Description = "Resets the user's password using the token from the email.";
            return operation;
        });
    }
}
