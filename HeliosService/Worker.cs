using HeliosClockCommon.Hubs;
using HeliosClockCommon.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace HeliosService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IHubContext<HeliosHub, IHeliosHub> _clockHub;

        /// <summary>Initializes a new instance of the <see cref="Worker"/> class.</summary>
        /// <param name="logger">The logger.</param>
        /// <param name="heliosHub">The Helios hub.</param>
        public Worker(ILogger<Worker> logger, IHubContext<HeliosHub, IHeliosHub> heliosHub)
        {
            _logger = logger;
            _clockHub = heliosHub;
        }

        /// <summary>
        /// This method is called when the <see cref="T:Microsoft.Extensions.Hosting.IHostedService" /> starts. The implementation should return a task that represents
        /// the lifetime of the long running operation(s) being performed.
        /// </summary>
        /// <param name="stoppingToken">Triggered when <see cref="M:Microsoft.Extensions.Hosting.IHostedService.StopAsync(System.Threading.CancellationToken)" /> is called.</param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the long running operations.</returns>
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
