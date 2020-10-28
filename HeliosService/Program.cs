using HeliosClockCommon.Defaults;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace HeliosService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

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
