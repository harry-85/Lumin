using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HeliosClockCommon.Discorvery
{
    public class DiscroveryServer : BackgroundService
    {
        private readonly ILogger<DiscroveryServer> _logger;

        public DiscroveryServer(ILogger<DiscroveryServer> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Started Discrovery Server ...");
            var Server = new UdpClient(8888);
            var ResponseData = Encoding.ASCII.GetBytes("SomeResponseData");

            await Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var ClientRequestData = await Server.ReceiveAsync().ConfigureAwait(false);
                    var ClientEp = ClientRequestData.RemoteEndPoint;
                    var ClientRequest = Encoding.ASCII.GetString(ClientRequestData.Buffer);

                    _logger.LogDebug("Recived {0} from {1}, sending response", ClientRequest, ClientEp.Address.ToString());
                    Server.Send(ResponseData, ResponseData.Length, ClientEp);
                }
            }).ConfigureAwait(false);
        }
    }
}
