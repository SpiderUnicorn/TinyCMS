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
    /// <summary>
    /// Extension methods for <see cref="IServiceCollection"/> to add register TinyCms services.
    /// </summary>
    public static class TinyCmsServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Tiny CMS to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> services.</param>
        /// <param name="configure"> An action for configuring services to register with <see cref="TinyCmsOptions" />.</param>
        /// <returns>An <see cref="ITinyCmsBuilder" /> for adding more parts of TinyCms</returns>
        public static ITinyCmsBuilder AddTinyCMS(this IServiceCollection services, Action<TinyCmsOptions> configure)
        {
            var settings = new TinyCmsOptions();
            configure(settings);
            return Setup(services, settings);
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
