namespace SteadyLearn.Modules.Auth.RefreshToken;

using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SteadyLearn.Common.Models;

/// <summary>
/// API endpoint for token refresh.
/// </summary>
public static class Endpoint
{
    public static void MapRefreshTokenEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/refresh", async (
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            // Get refresh token from cookie
            var refreshToken = httpContext.Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                return Results.BadRequest(
                    ApiErrorResponse.FromError("REFRESH_TOKEN_NOT_FOUND", "Refresh token not found"));
            }

            var command = new RefreshTokenCommand { RefreshToken = refreshToken };
            var result = await mediator.Send(command, cancellationToken);

            return result.Match<IResult>(
                onSuccess: response =>
                {
                    // Set new refresh token as HttpOnly cookie
                    httpContext.Response.Cookies.Append("refreshToken", response.RefreshToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.UtcNow.AddDays(7)
                    });

                    return Results.Ok(ApiResponse<object>.Ok(new
                    {
                        response.AccessToken,
                        response.ExpiresIn
                    }));
                },
                onFailure: error => error.Code switch
                {
                    "TOKEN_EXPIRED" => Results.Json(
                        ApiErrorResponse.FromError(error.Code, error.Message),
                        statusCode: StatusCodes.Status401Unauthorized),
                    _ => Results.BadRequest(
                        ApiErrorResponse.FromError(error.Code, error.Message))
                });
        })
        .WithName("RefreshToken")
        .WithTags("Auth")
        .Produces<ApiResponse<RefreshTokenResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Refresh access token";
            operation.Description = "Uses the refresh token from HttpOnly cookie to generate a new access token.";
            return operation;
        });
    }
}
