using HeliosClockCommon.Helper;
using HeliosClockCommon.Hubs;
using HeliosClockCommon.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HeliosService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IHubContext<HeliosHub, IHeliosHub> _clockHub;

        public Worker(ILogger<Worker> logger, IHubContext<HeliosHub, IHeliosHub> heliosHub)
        {
            _logger = logger;
            _clockHub = heliosHub;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            ////while (!stoppingToken.IsCancellationRequested)
            ////{
            ////    //_logger.LogInformation("Worker running at: {Time}", DateTime.Now);
            ////    // await _clockHub.Clients.All.ShowTime(DateTime.Now);
            ////    await _clockHub.Clients.All.SetColorString(ColorHelpers.HexConverter(Color.Red), ColorHelpers.HexConverter(Color.Blue));
            ////    await Task.Delay(1000);
            ////}
            return Task.CompletedTask;
        }
    }
}
