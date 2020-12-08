using HeliosClockCommon.Defaults;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Net;

namespace HeliosService
{
    public class Program
    {
        /// <summary>Defines the entry point of the application.</summary>
        /// <param name="args">The arguments.</param>
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>Creates the host builder.</summary>
        /// <param name="args">The arguments.</param>
        public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args).UseSystemd().ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.ConfigureKestrel(serverOptions =>
                    {
                        serverOptions.Listen(IPAddress.Any, DefaultValues.SignalPortOne);
                        // serverOptions.Listen(IPAddress.Any, DefaultValues.SignalPortTwo);
                    });
                });
    }
}