using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using TinyCMS.Base;
using TinyCMS.Commerce;
using TinyCMS.Commerce.Models;
using TinyCMS.Data.Builder;
using TinyCMS.FileStorage.Storage;
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
                settings.AddAssemblyWithNodes<ResizImage>();
                settings.AddAssemblyWithNodes<Commerce.Nodes.Product>();

                JsonConvert.DefaultSettings = (() => new JsonSerializerSettings().ConfigureCmsSettings(settings.NodeFactoryInstance));
            });

            return services;
        }
    }
}
