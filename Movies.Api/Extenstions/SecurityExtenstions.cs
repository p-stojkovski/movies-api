using FluentValidation;
using Movies.Application.Repositories;
using Movies.Application.Services;
using Movies.Application;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Movies.Api.Auth;

namespace Movies.Api.Extenstions;

public static class SecurityExtenstions
{
    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(x =>
        {
            //For real world app,it is not recommended to use SymetricKey,
            //instead use public key crypography, certificate, etc...
            x.TokenValidationParameters = new TokenValidationParameters
            {
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!)),
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidIssuer = config["Jwt:Issuer"],
                ValidAudience = config["Jwt:Audience"],
                ValidateIssuer = true,
                ValidateAudience = true,
            };
        });

        return services;
    }

    public static IServiceCollection AddAuthorizationServices(this IServiceCollection services)
    {
        services.AddAuthorization(x =>
        {
            x.AddPolicy(AuthConstants.AdminUserPolicyName,
                p => p.RequireClaim(AuthConstants.AdminUserPolicyName, "true"));

            x.AddPolicy(AuthConstants.TrustMemberPolicyName,
                p => p.RequireAssertion(c =>
                    c.User.HasClaim(m => m is { Type: AuthConstants.AdminUserClaimName, Value: "true" }) ||
                    c.User.HasClaim(m => m is { Type: AuthConstants.TrustMemberClaimName, Value: "true" }))
                );
        });

        return services;
    }
}
