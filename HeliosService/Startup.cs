using HeliosClockAPIStandard;
using HeliosClockAPIStandard.Controller;
using HeliosClockAPIStandard.GPIOListeners;
using HeliosClockCommon.Clients;
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
        private CancellationTokenSource cancellationTokenSource;
        private CancellationToken token;
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            cancellationTokenSource = new CancellationTokenSource();
            token = cancellationTokenSource.Token;

            ILedController controller = new LedAPA102Controller { LedCount = 58 };
            IHeliosManager heliosManager = new HeliosManager(controller);

            services.AddHostedService<DiscroveryServer>();

            services.AddSignalR(options => { options.EnableDetailedErrors = true; });
            services.AddSingleton(controller);
            services.AddSingleton(heliosManager);


            services.AddHostedService<Worker>();
            services.AddHostedService<GPIOService>();
            services.AddSingleton<HeliosServerClient>();

#pragma warning disable ASP0000 // Do not call 'IServiceCollection.BuildServiceProvider' in 'ConfigureServices'
            var provider = services.BuildServiceProvider();
#pragma warning restore ASP0000 // Do not call 'IServiceCollection.BuildServiceProvider' in 'ConfigureServices'

            var heliosServerClient = provider.GetService<HeliosServerClient>();
            Task.Run(async () => await heliosServerClient.StartAsync(token).ConfigureAwait(false));
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