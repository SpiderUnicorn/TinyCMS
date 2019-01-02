using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TinyCMS.Base.Security;
using TinyCMS.FileStorage;
using TinyCMS.Interfaces;
using TinyCMS.Security;
using TinyCMS.Serializer;

namespace TinyCMS.Base
{
    public static class TinyCmsServiceCollectionExtensions
    {
        public static ITinyCmsBuilder AddTinyCMS(this IServiceCollection services, Action<TinyCmsOptions> options)
        {
            var settings = new TinyCmsOptions();
            options.Invoke(settings);
            return Setup(services, settings);

        }

        public static ITinyCmsBuilder AddTinyCMS(this IServiceCollection services, IOptions<TinyCmsOptions> options)
        {
            var settings = options.Value;
            return Setup(services, settings);

        }

        private static IServiceCollection AddSingletonMapped<T>(this IServiceCollection services, Type type)
        {
            return services.AddSingleton(type);
        }

        private static ITinyCmsBuilder Setup(IServiceCollection services, TinyCmsOptions settings)
        {
            var nodeFactory = settings.NodeFactoryInstance;

            services
                .AddSingleton<ITokenDecoder, TokenDecoder>()
                .AddSingleton(nodeFactory)
                .AddSingleton(typeof(INodeSerializer), settings.NodeSerializer)
                .AddSingleton(typeof(SchemaSerializer), settings.SchemaSerializer)
                .AddSingleton(typeof(ITokenValidator), settings.TokenValidator);

            return new TinyCmsBuilder(services);
        }

    }
}
