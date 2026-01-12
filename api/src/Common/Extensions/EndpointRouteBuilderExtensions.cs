namespace SteadyLearn.Common.Extensions;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using SteadyLearn.Modules.Auth.Login;
using SteadyLearn.Modules.Auth.Logout;
using SteadyLearn.Modules.Auth.RefreshToken;
using SteadyLearn.Modules.Auth.Register;
using SteadyLearn.Modules.Auth.ResetPassword;
using SteadyLearn.Modules.Auth.VerifyEmail;

/// <summary>
/// Extension methods for endpoint routing.
/// </summary>
public static class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps all authentication endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapRegisterEndpoint();
        app.MapLoginEndpoint();
        app.MapVerifyEmailEndpoint();
        app.MapRefreshTokenEndpoint();
        app.MapLogoutEndpoint();
        app.MapResetPasswordEndpoints();

        return app;
    }
}
