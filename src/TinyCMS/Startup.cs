using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Swagger;
using TinyCMS.Base;
using TinyCMS.Commerce;
using TinyCMS.Commerce.Models;
using TinyCMS.Commerce.Nodes;
using TinyCMS.Commerce.Services;
using TinyCMS.Data.Builder;
using TinyCMS.FileStorage;
using TinyCMS.FileStorage.Storage;
using TinyCMS.Interfaces;
using TinyCMS.Node.ResizeImage;
using TinyCMS.Proxy;
using TinyCMS.QuestionNodes;
using TinyCMS.Security;
using TinyCMS.Serializer;
using TinyCMS.SocketServer;
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

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            var secretKey = Configuration["JWTSecret"];

            services.AddTinyCMS((configure) =>
                {
                    configure.AddAssemblyWithNodes<Question>();
                    configure.AddAssemblyWithNodes<ResizeImage>();
                    configure.AddAssemblyWithNodes<Commerce.Nodes.Product>();

                    JsonConvert.DefaultSettings = (() => ConfigureCmsSettings(configure.NodeFactoryInstance));
                })
                .AddFileStorage()
                .AddCommerce()
                .AddJwtAuthentication(secretKey);

            services.AddProxy()
                .AddSwaggerGen(c => { c.SwaggerDoc("v1", new Info { Title = "TinyCMS API", Version = "v1" }); })
                .AddSpaStaticFiles(config => { config.RootPath = "ReactClient/build"; });

            services.AddMvc()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    options.SerializerSettings.MaxDepth = 15;
                    options.SerializerSettings.Converters.Add(new StringEnumConverter
                    {
                        NamingStrategy = new CamelCaseNamingStrategy()
                    });
                    options.SerializerSettings.ObjectCreationHandling = ObjectCreationHandling.Replace;
                }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
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

            app.UseProxy("/shopproxy", "https://www.bygglagret.se/Core.WebShop,Core.WebShop.ShopCommon.asmx");

            app.UseTinyCms();
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
