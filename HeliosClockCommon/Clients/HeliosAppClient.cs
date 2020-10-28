using HeliosClockCommon.Defaults;
using HeliosClockCommon.Helper;
using HeliosClockCommon.Interfaces;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace HeliosClockCommon.Clients
{
    public partial class HeliosAppClient : IHeliosHub
    {
        private HubConnection _connection;

        public HeliosAppClient()
        {
            
        }

        public async Task SendColor(Color startColor, Color endColor)
        {
            await _connection.InvokeAsync(nameof(IHeliosHub.SetColorString), ColorHelpers.HexConverter(startColor), ColorHelpers.HexConverter(endColor)).ConfigureAwait(false);
        }

        public async Task StartMode(string mode)
        {
            await _connection.InvokeAsync(nameof(IHeliosHub.StartMode), mode).ConfigureAwait(false);
        }

        public async Task Stop()
        {
            await _connection.InvokeAsync(nameof(IHeliosHub.Stop)).ConfigureAwait(false);
        }

        public async Task SetRefreshSpeed(string speed)
        {
            await _connection.InvokeAsync<string>(nameof(IHeliosHub.SetRefreshSpeed), speed).ConfigureAwait(false);
        }

        public Task SetAlarm(DateTime alarmTime)
        {
            throw new NotImplementedException();
        }

        public Task SetColor(Color startColor, Color endColor)
        {
            throw new NotImplementedException();
        }

        public Task SetColorString(string startColor, string endColor)
        {
            throw new NotImplementedException();
        }

        public Task SignalClient(string user, string message)
        {
            throw new NotImplementedException();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            string URL = string.Format(DefaultValues.HubUrl, "192.168.0.136", DefaultValues.SignalPortOne);

            Console.WriteLine("####################################################");
            Console.WriteLine(URL);

            _connection = new HubConnectionBuilder().WithUrl(URL).Build();

            // Loop is here to wait until the server is running
            while (_connection.State != HubConnectionState.Connected && !cancellationToken.IsCancellationRequested)
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

        public async Task StopAsync()
        {
            await _connection.DisposeAsync().ConfigureAwait(false);
        }
    }
}
