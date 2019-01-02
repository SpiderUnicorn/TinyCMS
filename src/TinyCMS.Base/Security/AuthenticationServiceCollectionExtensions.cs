using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using TinyCMS.Base;

namespace TinyCMS.Security
{
    public static class AuthenticationServiceCollectionExtensions
    {
        public static ITinyCmsBuilder AddJwtAuthentication(this ITinyCmsBuilder builder, string key)
        {
            var settings = new JWTSettings(key);

            builder.Services
                .AddSingleton(typeof(IJWTSettings), settings)
                .AddAuthentication(x =>
                {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(x =>
                {
                    x.RequireHttpsMetadata = false;
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = settings.GetSecurityKey(),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });
            return builder;
        }
    }
}
