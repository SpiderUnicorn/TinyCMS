using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TinyCMS.Base;
using TinyCMS.Commerce;
using TinyCMS.Commerce.Models;
using TinyCMS.Data.Builder;
using TinyCMS.FileStorage;
using TinyCMS.FileStorage.Storage;
using TinyCMS.Interfaces;
using TinyCMS.Node.ResizeImage;
using TinyCMS.QuestionNodes;
using TinyCMS.Security;

namespace TinyCMS
{
    /// <summary>
    /// Extension methods for setting up TinyCMS services in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class TinyCMSServiceCollectionExtensions
    {

        /// <summary>
        ///  Adds TinyCMS services to the <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <param name="secretKey">The JWT secret</param>
        /// <returns></returns>
        public static IServiceCollection AddTinyCMS(this IServiceCollection services, string secretKey)
        {
            InterfaceResolver.Instance.Add<IOrderArticle, OrderArticle>();

            services.AddFileStorage();
            services.AddCommerceConfiguration((settings) =>
            {
                // change commerce settings here
            });

            services.AddCMSConfiguration((settings) =>
            {
                settings.JWTSettings = new JWTSettings(secretKey);
                settings.AddAssemblyWithNodes<Question>();
                settings.AddAssemblyWithNodes<ResizeImage>();
                settings.AddAssemblyWithNodes<Commerce.Nodes.Product>();

                JsonConvert.DefaultSettings = (() => ConfigureCmsSettings(settings.NodeFactoryInstance));
            });

            return services;
        }

        public static JsonSerializerSettings ConfigureCmsSettings(INodeTypeFactory factory)
        {
            var settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.ObjectCreationHandling = ObjectCreationHandling.Replace;
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            settings.Converters.Add(new JsonNodeConverter(factory));
            settings.Converters.Add(new JsonMappedInterfaceConverter());
            return settings;
        }
    }
}
