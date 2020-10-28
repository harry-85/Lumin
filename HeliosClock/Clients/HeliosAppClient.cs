using HeliosClock.Interfaces;
using HeliosClockCommon.Defaults;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace HeliosClockAPI.Clients
{
    public partial class HeliosAppClient : IHeliosHub
    {
        private readonly ILogger<HeliosServerClient> _logger;
        private HubConnection _connection;

        public HeliosAppClient(ILogger<HeliosServerClient> logger)
        {
            _logger = logger;
            _connection = new HubConnectionBuilder().WithUrl(string.Format(DefaultValues.HubUrl, DefaultValues.Port, "192.168.0.136")).Build();
            _connection.On<Color, Color>(nameof(IHeliosHub.SetColor), SetColor);
        }

        public async Task SendColor(Color startColor, Color endColor)
        {
            await _connection.InvokeAsync(nameof(IHeliosHub.SetColor), startColor, endColor);
        }

        public Task SetAlarm(DateTime alarmTime)
        {
            throw new NotImplementedException();
        }

        public Task SetColor(Color startColor, Color endColor)
        {
            throw new NotImplementedException();
        }

        public Task SignalClient(string user, string message)
        {
            throw new NotImplementedException();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Loop is here to wait until the server is running
            while (true)
            {
                try
                {
                    await _connection.StartAsync(cancellationToken).ConfigureAwait(false);

                    break;
                }
                catch
                {
                    await Task.Delay(1000).ConfigureAwait(false);
                }
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _connection.DisposeAsync().ConfigureAwait(false);
        }
    }
}
