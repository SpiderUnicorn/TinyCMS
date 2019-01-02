using System;
using Microsoft.Extensions.DependencyInjection;
using TinyCMS.Base;
using TinyCMS.Commerce.Models;
using TinyCMS.Commerce.Nodes;
using TinyCMS.Commerce.Services;
using TinyCMS.Data.Builder;

namespace TinyCMS.Commerce
{
    public static class TinyCmsCommerceServiceCollectionExtensions
    {
        public static ITinyCmsBuilder AddCommerce(this ITinyCmsBuilder builder)
        {
            var settings = new TinyShopOptions();
            return SetupCommerce(builder, settings);
        }

        private static ITinyCmsBuilder SetupCommerce(ITinyCmsBuilder builder, TinyShopOptions settings)
        {
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
