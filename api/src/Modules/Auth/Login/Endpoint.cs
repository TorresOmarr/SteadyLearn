namespace SteadyLearn.Modules.Auth.Login;

using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SteadyLearn.Common.Models;

/// <summary>
/// API endpoint for user login.
/// </summary>
public static class Endpoint
{
    public static void MapLoginEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/login", async (
            LoginCommand command,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(command, cancellationToken);

            return result.Match(
                onSuccess: response =>
                {
                    // Set refresh token as HttpOnly cookie
                    httpContext.Response.Cookies.Append("refreshToken", response.RefreshToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.UtcNow.AddDays(7)
                    });

                    // Return response without refresh token in body (it's in the cookie)
                    return Results.Ok(ApiResponse<object>.Ok(new
                    {
                        response.AccessToken,
                        response.ExpiresIn,
                        response.User
                    }));
                },
                onFailure: error => error.Code switch
                {
                    "ACCOUNT_NOT_VERIFIED" => Results.Json(
                        ApiErrorResponse.FromError(error.Code, error.Message),
                        statusCode: StatusCodes.Status403Forbidden),
                    _ => Results.BadRequest(
                        ApiErrorResponse.FromError(error.Code, error.Message))
                });
        })
        .WithName("Login")
        .WithTags("Auth")
        .Produces<ApiResponse<LoginResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Login a user";
            operation.Description = "Authenticates a user and returns access and refresh tokens.";
            return operation;
        });
    }
}
