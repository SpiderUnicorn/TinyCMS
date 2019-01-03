using System;
using Microsoft.Extensions.DependencyInjection;
using TinyCMS.Base;
using TinyCMS.Commerce.Models;
using TinyCMS.Commerce.Nodes;
using TinyCMS.Commerce.Services;
using TinyCMS.Data.Builder;

namespace TinyCMS.Commerce
{
    /// <summary>
    /// Extension methods for <see cref="ITinyCmsBuilder"/> to add TinyCms Commerce.
    /// </summary>
    public static class TinyCmsCommerceServiceCollectionExtensions
    {
        /// <summary>
        /// Adds TiyCms Commerce to the <see cref="ITinyCmsBuilder" />.
        /// </summary>
        /// <param name="builder"></param>
        public static ITinyCmsBuilder AddCommerce(this ITinyCmsBuilder builder)
        {
            var settings = new TinyShopOptions();

            InterfaceResolver.Instance.Add<IOrderArticle, OrderArticle>();

            builder.Services
                .AddSingleton(typeof(IShopFactory), settings.ShopFactory)
                .AddTransient(typeof(IOrder), settings.OrderType)
                .AddTransient(typeof(IArticle), settings.ShopArticleType)
                .AddTransient(typeof(IShopArticleWithProperties), settings.ShopArticleWithPropertiesType)
                .AddTransient(typeof(IOrderArticle), settings.OrderArticleType)
                .AddSingleton(typeof(IOrderService), settings.OrderService)
                .AddSingleton(typeof(IArticleService), settings.ArticleService)
                .AddSingleton(typeof(IProductService), settings.ProductService);

            return builder;
        }
    }
}
