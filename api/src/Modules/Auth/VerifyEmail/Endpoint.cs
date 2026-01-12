namespace SteadyLearn.Modules.Auth.VerifyEmail;

using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SteadyLearn.Common.Models;

/// <summary>
/// API endpoint for email verification.
/// </summary>
public static class Endpoint
{
    public static void MapVerifyEmailEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/auth/verify-email", async (
            string token,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new VerifyEmailCommand { Token = token };
            var result = await mediator.Send(command, cancellationToken);

            return result.Match<IResult>(
                onSuccess: response => Results.Ok(
                    ApiResponse<VerifyEmailResponse>.Ok(response)),
                onFailure: error => error.Code switch
                {
                    "VERIFICATION_TOKEN_EXPIRED" => Results.Json(
                        ApiErrorResponse.FromError(error.Code, error.Message),
                        statusCode: StatusCodes.Status410Gone),
                    _ => Results.BadRequest(
                        ApiErrorResponse.FromError(error.Code, error.Message))
                });
        })
        .WithName("VerifyEmail")
        .WithTags("Auth")
        .Produces<ApiResponse<VerifyEmailResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status410Gone)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Verify email";
            operation.Description = "Verifies a user's email using the token sent via email.";
            return operation;
        });
    }
}
