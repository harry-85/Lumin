using HeliosClockAPIStandard;
using HeliosClockAPIStandard.Controller;
using HeliosClockAPIStandard.GPIOListeners;
using HeliosClockCommon.Clients;
using HeliosClockCommon.Configurator;
using HeliosClockCommon.Defaults;
using HeliosClockCommon.Discorvery;
using HeliosClockCommon.Hubs;
using HeliosClockCommon.Interfaces;
using HeliosClockCommon.LedCommon;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace HeliosService
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            ILedController controller = new LedAPA102Controller { LedCount = 58 };
            ILuminManager heliosManager = new LuminManager(controller);
            ConfigureService configureService = new ConfigureService();

            services.AddHostedService<DiscroveryServer>();

            services.AddSignalR(options => { options.EnableDetailedErrors = true; });
            services.AddSingleton(controller);
            services.AddSingleton(heliosManager);
            services.AddSingleton(configureService);
            services.AddHostedService<GPIOService>();
            services.AddHostedService<HeliosServerClient>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<HeliosHub>(DefaultValues.BaseUrl);
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with SignalR");
                });
            });
        }
    }
}