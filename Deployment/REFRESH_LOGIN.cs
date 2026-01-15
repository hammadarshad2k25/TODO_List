using FastEndpoints;
using FastEndpoints.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using System.Security.Cryptography;
using TODO_List.Application.DTO;
using TODO_List.Infrastructure.Storage;

namespace TODO_List.Deployment
{
    public class REFRESH_LOGIN : Endpoint<RefreshDTO>
    {
        public override void Configure()
        {
            Post("/api/REFRESH_LOGIN");
            AllowAnonymous();
        }

        public override async Task HandleAsync(RefreshDTO req, CancellationToken ct)
        {
            if (!RefreshTokenStore.ValidateToken(req.UserName, req.RenewToken))
            {
                HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await HttpContext.Response.WriteAsync("Invalid refresh token.", ct);
                return;
            }
            var secret = Environment.GetEnvironmentVariable("JWT_KEY");
            var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
            var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
            var expireHours = int.TryParse(Environment.GetEnvironmentVariable("JWT_EXPIRE_HOURS"), out var h) ? h : 1;
            var newToken = JwtBearer.CreateToken(options =>
            {
                options.SigningKey = secret;
                options.ExpireAt = DateTime.UtcNow.AddHours(expireHours);
                options.Issuer = issuer;
                options.Audience = audience;
                options.User.Roles.Add("Admin");
                options.User["UserName"] = req.UserName;
            });
            var newRefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            RefreshTokenStore.StoreToken(req.UserName, newRefreshToken);
            await HttpContext.Response.WriteAsJsonAsync(new
            {
                Token = newToken,
                RefreshToken = newRefreshToken
            }, cancellationToken: ct);
        }
    }
}
