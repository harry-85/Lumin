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
        /// <summary>Gets or sets a value indicating whether this instance is initial connection.</summary>
        /// <value><c>true</c> if this instance is initial connection; otherwise, <c>false</c>.</value>
        public static bool IsInitialConnection { get; set; } = true;

        /// <summary>Occurs when connection tu hub established.</summary>
        public event EventHandler<EventArgs<bool>> OnConnected;

        /// <summary>The connection.</summary>
        private HubConnection _connection;

        /// <summary>Initializes a new instance of the <see cref="HeliosAppClient"/> class.</summary>
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

        public async Task SetOnOff(string onOff)
        {
            await _connection.InvokeAsync<string>(nameof(IHeliosHub.SetOnOff), onOff).ConfigureAwait(false);
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

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () => await StopOldConnection(_connection).ConfigureAwait(false));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            _connection = new HubConnectionBuilder().WithUrl(URL).Build();

            // Loop is here to wait until the server is running
            while (_connection.State != HubConnectionState.Connected && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await _connection.StartAsync(cancellationToken).ConfigureAwait(false);

                    break;
                }
                catch (ObjectDisposedException ex)
                {
                    _connection = new HubConnectionBuilder().WithUrl(URL).Build();
                    await Task.Delay(1000).ConfigureAwait(false);
                }
                catch
                {
                    await Task.Delay(1000).ConfigureAwait(false);
                }
            }

            OnConnected?.Invoke(this, new EventArgs<bool>(IsInitialConnection));

            if (IsInitialConnection)
                IsInitialConnection = false;
        }

        public async Task StopAsync()
        {
            await _connection.DisposeAsync().ConfigureAwait(false);
        }

        private async Task StopOldConnection(HubConnection oldConnection)
        {
            try
            {
                if (oldConnection == null)
                    await Task.CompletedTask.ConfigureAwait(false);

                await oldConnection.DisposeAsync().ConfigureAwait(false);
            }
            catch
            { }
        }

    }
}
