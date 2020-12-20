using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HeliosClockCommon.Defaults;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HeliosClockCommon.Discorvery
{
    public class DiscroveryServer : BackgroundService
    {
        private readonly ILogger<DiscroveryServer> logger;
        private DiscoverFactory discoverFactory;
        /// <summary>Initializes a new instance of the <see cref="DiscroveryServer"/> class.</summary>
        /// <param name="logger">The logger.</param>
        public DiscroveryServer(DiscoverFactory factory, ILogger<DiscroveryServer> logger)
        {
            this.discoverFactory = factory;
            this.logger = logger;
        }

        /// <summary>
        /// This method is called when the <see cref="T:Microsoft.Extensions.Hosting.IHostedService" /> starts. The implementation should return a task that represents
        /// the lifetime of the long running operation(s) being performed.
        /// </summary>
        /// <param name="stoppingToken">Triggered when <see cref="M:Microsoft.Extensions.Hosting.IHostedService.StopAsync(System.Threading.CancellationToken)" /> is called.</param>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Started Discovery Server ...");
            var responseData = Encoding.ASCII.GetBytes(DefaultDiscoveryValues.DefaultDiscoveryResponse);

            await Task.Run(async () =>
            {
                UdpClient server = discoverFactory.UdpClient;
                try
                {
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        var clientRequestData = await server.ReceiveAsync().ConfigureAwait(false);
                        var clientRequest = Encoding.ASCII.GetString(clientRequestData.Buffer);

                        if (clientRequest != DefaultDiscoveryValues.DefaultDiscoveryRequest)
                            continue;

                        var clientEp = clientRequestData.RemoteEndPoint;

                        logger.LogTrace("Received {0} from {1}, sending response ...", clientRequest, clientEp.Address.ToString());
                        server.Send(responseData, responseData.Length, clientEp);
                    }
                }
                catch (Exception ex)
                {
                    server?.Close();

                    logger.LogWarning("Cannot wait for to discovery request. Message: {0} ...", ex.Message);
                    await Task.Delay(1000).ConfigureAwait(false);
                }
            }, stoppingToken).ConfigureAwait(false);

            logger.LogInformation("Discovery Server Ended ...");
        }
    }
}
