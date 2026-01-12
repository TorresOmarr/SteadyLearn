namespace SteadyLearn.Modules.Auth.Logout;

using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SteadyLearn.Common.Models;

/// <summary>
/// API endpoint for user logout.
/// </summary>
public static class Endpoint
{
    public static void MapLogoutEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/logout", async (
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            // Get user ID from claims
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Results.Unauthorized();
            }

            var command = new LogoutCommand { UserId = userId };
            var result = await mediator.Send(command, cancellationToken);

            // Clear refresh token cookie
            httpContext.Response.Cookies.Delete("refreshToken");

            return result.Match<IResult>(
                onSuccess: response => Results.Ok(
                    ApiResponse<LogoutResponse>.Ok(response)),
                onFailure: error => Results.BadRequest(
                    ApiErrorResponse.FromError(error.Code, error.Message)));
        })
        .RequireAuthorization()
        .WithName("Logout")
        .WithTags("Auth")
        .Produces<ApiResponse<LogoutResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Logout user";
            operation.Description = "Invalidates the user's refresh token and clears the cookie.";
            return operation;
        });
    }
}
