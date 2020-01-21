using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using System;
using System.Net.Http;
using TimeSheetCore;
using TimeSheetCore.Services;

namespace TimeSheetApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        readonly string CorsAllowEverything = "_corsAllowEverything";


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(CorsAllowEverything,
                    builder => { builder.AllowAnyOrigin().AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod(); });
            });

            services.AddMvc(options => options.EnableEndpointRouting = false)
                .AddNewtonsoftJson(options =>
                    options.SerializerSettings.ContractResolver =
                        new CamelCasePropertyNamesContractResolver());

            services.Configure<IISOptions>(options => { options.AutomaticAuthentication = true; });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1",
                    new Microsoft.OpenApi.Models.OpenApiInfo { Title = "TimeSheetApi", Version = "v1" });
            });

            var appConfig = Configuration.GetSection("AppConfig").Get<AppConfig>();

            services.AddTransient<IPrikkingenService, PrikkingenService>();

            services.AddHttpClient<IPrikkingenService, PrikkingenService>(c =>
                c.BaseAddress = new Uri(appConfig.ClientBaseUrl))
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
                {
                    UseDefaultCredentials = true
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            loggerFactory.AddLog4Net();

            app.UseCors(CorsAllowEverything);

            app.UseMvc();

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
#if DEBUG
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "TimeSheetApi V1 (DEBUG)");
#else
                c.SwaggerEndpoint("/TimeSheetApi/swagger/v1/swagger.json", "TimeSheetApi V1 (RELEASE)");
#endif
            });
        }
    }
}
