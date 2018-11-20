﻿using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Swagger;
using TinyCMS.Data.Builder;
using TinyCMS.FileStorage;
using TinyCMS.Interfaces;
using TinyCMS.Serializer;
using TinyCMS.SocketServer;
using TinyCMS.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TinyCMS.Commerce;
using TinyCMS.Commerce.Models;
using TinyCMS.Commerce.Services;

using TinyCMS.Proxy;
using TinyCMS.Commerce.Nodes;
using TinyCMS.Storage;

namespace TinyCMS
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureCMS(IServiceCollection services)
        {
            var typeMapper = new InterfaceResolverFactory();
            typeMapper.Add<IOrderArticle, OrderArticle>();

            var nodeFactory = new NodeTypeFactory();
            nodeFactory.RegisterTypes(typeof(Startup).Assembly);
            nodeFactory.RegisterTypes(typeof(Node.ResizeImage.ResizImage).Assembly);
            nodeFactory.RegisterTypes(typeof(TinyCMS.Commerce.Nodes.Product).Assembly);


            //var shopConverter = new JsonShopNodeConverter>(typeof(Node));
            var secretKey = Configuration["JWTSecret"];
            var securitySettings = new JWTSettings(secretKey);

            ConfigureStorage(services);

            ConfigureShop(services);

            services
                .AddSingleton<INodeTypeFactory>(nodeFactory)
                .AddSingleton<IStorageService, JsonStorageService>()
                .AddSingleton<IShopFactory, ShopFactory>()
                .AddSingleton<ProxyService>()
                .AddSingleton<IJWTSettings>(securitySettings)
                .AddSingleton<INodeStorage, NodeFileStorage<Container>>()
                .AddSingleton((sp) =>
                {
                    return sp.GetService<INodeStorage>().Load();
                })
                .AddSingleton<INodeSerializer, NodeSerializer>()
                .AddSingleton<ITokenValidator, GoogleTokenValidator>()
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
                    IssuerSigningKey = securitySettings.GetSecurityKey(),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            JsonConvert.DefaultSettings = (() =>
            {
                var settings = new JsonSerializerSettings();
                settings.NullValueHandling = NullValueHandling.Ignore;
                settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                settings.Converters.Add(new JsonNodeConverter(nodeFactory));
                settings.Converters.Add(new JsonMappedInterfaceConverter(typeMapper));
                return settings;
            });
        }

        private void ConfigureStorage(IServiceCollection services)
        {
            services.AddTransient<IFile, TinyCMS.FileStorage.Storage.File>();
            services.AddTransient<IDirectory, TinyCMS.FileStorage.Storage.Directory>();
            services.AddSingleton<IFileStorageService,TinyCMS.FileStorage.Storage.FileStorageService>();
        }

        private void ConfigureShop(IServiceCollection services)
        {
            services
                .AddTransient<IOrder, NodeOrder>()
                .AddTransient<IArticle, ShopArticle>()
                .AddTransient<IShopArticleWithProperties, ShopArticle>()
                .AddTransient<IOrderArticle, OrderArticle>()
                .AddSingleton<IOrderService, NodeOrderService>()
                .AddSingleton<IArticleService, MockArticleService>()
                .AddSingleton<IProductService, MockProductService>();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureCMS(services);

            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new Info { Title = "TinyCMS API", Version = "v1" }); });

            services.AddMvc().AddJsonOptions(options =>
            {
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                options.SerializerSettings.MaxDepth = 15;
                options.SerializerSettings.Converters.Add(new StringEnumConverter
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                });
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);



            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ReactClient/build";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
                app.UseHttpsRedirection();
            }


            app.UseSwagger();
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "TinyCMS API V1"); });
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseAuthentication();

            var options = new ProxyOptions()
            {
                LocalUrl = "/shopproxy",
                Destination = "https://www.bygglagret.se/Core.WebShop,Core.WebShop.ShopCommon.asmx"
            };
            app.UseMiddleware<ProxyMiddleware>(Options.Create(options));

            app.UseSocketServer(serviceProvider);
            app.UseMvc();
            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ReactClient";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }
    }
}
