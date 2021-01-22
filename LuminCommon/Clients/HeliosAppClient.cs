using System;
using System.Drawing;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LuminCommon.Defaults;
using LuminCommon.Enumerations;
using LuminCommon.EventArgs;
using LuminCommon.Helper;
using LuminCommon.Interfaces;
using LuminCommon.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace LuminCommon.Clients
{
    public partial class HeliosAppClient
    {
        /// <summary>Gets or sets a value indicating whether this instance is initial connection.</summary>
        /// <value><c>true</c> if this instance is initial connection; otherwise, <c>false</c>.</value>
        public static bool IsInitialConnection { get; set; } = true;

        /// <summary>Gets the client identifier.</summary>
        /// <value>The client identifier.</value>
        public string ClientId => _connection.ConnectionId;

        public IPAddress IPAddress { get; set; }

        /// <summary>Occurs when connection to hub established.</summary>
        public event EventHandler<NotifyControllerEventArgs> OnControllerNotified;

        /// <summary>Occurs when connection to hub established.</summary>
        public event EventHandler<EventArgs<bool>> OnConnected;

        /// <summary>Occurs when [led client changed].</summary>
        public event EventHandler<EventArgs<string>> LedClientChanged;

        /// <summary>The connection.</summary>
        private HubConnection _connection;

        /// <summary>Initializes a new instance of the <see cref="HeliosAppClient"/> class.</summary>
        public HeliosAppClient()
        {
            IPAddress = null; 
        }

        public async Task SendColor(LedClient ledClient, Color startColor, Color endColor)
        {
            string mode = ColorInterpolationMode.HueMode.ToString();
            await _connection.InvokeAsync(nameof(IHeliosHub.SetColorString), ledClient.Id, ColorHelpers.HexConverter(startColor), ColorHelpers.HexConverter(endColor), mode).ConfigureAwait(false);
        }

        public async Task StartMode(LedClient ledClient, string mode)
        {
            await _connection.InvokeAsync(nameof(IHeliosHub.StartMode), ledClient.Id, mode).ConfigureAwait(false);
        }

        public async Task SetOnOff(LedClient ledClient, string onOff, string side)
        {
            await _connection.InvokeAsync<string>(nameof(IHeliosHub.SetOnOff), ledClient.Id, onOff, side).ConfigureAwait(false);
        }

        public async Task Stop(LedClient ledClient)
        {
            await _connection.InvokeAsync(nameof(IHeliosHub.Stop), ledClient.Id).ConfigureAwait(false);
        }

        public async Task SetRefreshSpeed(LedClient ledClient, string speed)
        {
            await _connection.InvokeAsync<string>(nameof(IHeliosHub.SetRefreshSpeed), ledClient.Id, speed).ConfigureAwait(false);
        }

        public async Task SetBrightness(LedClient ledClient, string brightness)
        {
            await _connection.InvokeAsync<string>(nameof(IHeliosHub.SetBrightness), ledClient.Id, brightness).ConfigureAwait(false);
        }
        public async Task SetRandomColor(LedClient ledClient)
        {
            await _connection.InvokeAsync<string>(nameof(IHeliosHub.SetRandomColor), ledClient.Id).ConfigureAwait(false);
        }

        public async Task RegisterAsController()
        {
           await _connection.InvokeAsync<string>(nameof(IHeliosHub.RegisterAsController), ClientId).ConfigureAwait(false);
        }

        /// <summary>Initializes this instance.</summary>
        public void Initialize()
        {
            _connection.On<string>(nameof(IHeliosHub.LedClientChanged), OnLedClientChanged);
            _connection.On<string, string>(nameof(IHeliosHub.NotifyController), OnNotifyController);
            _connection.Reconnected += _connection_Reconnected;
        }

        private Task _connection_Reconnected(string arg)
        {
            OnConnected?.Invoke(this, new EventArgs<bool>(IsInitialConnection));
            return Task.CompletedTask;
        }

        /// <summary>Called when any LED client changed.</summary>
        /// <param name="clients">The clients.</param>
        private Task OnLedClientChanged(string clients)
        {
            LedClientChanged.Invoke(this, new EventArgs<string>(clients));
            return Task.CompletedTask;
        }

        /// <summary>Called when [notify controller].</summary>
        /// <param name="startColor">The start color.</param>
        /// <param name="endColor">The end color.</param>
        private Task OnNotifyController(string startColor, string endColor)
        {
            OnControllerNotified?.Invoke(this, new NotifyControllerEventArgs { StartColor = ColorHelpers.FromHex(startColor), EndColor = ColorHelpers.FromHex(endColor) });
            return Task.CompletedTask;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            string URL = string.Format(DefaultValues.HubUrl, IPAddress, DefaultValues.SignalPortOne);

            HubConnection oldConnection = _connection;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () => await StopOldConnection(oldConnection).ConfigureAwait(false));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            _connection = new HubConnectionBuilder().WithUrl(URL).WithAutomaticReconnect().Build();
            Initialize();

            // Loop is here to wait until the server is running
            while (_connection.State != HubConnectionState.Connected && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await _connection.StartAsync(cancellationToken).ConfigureAwait(false);

                    while (_connection.State == HubConnectionState.Connecting && !cancellationToken.IsCancellationRequested)
                    {
                        await Task.Delay(100).ConfigureAwait(false);
                    }

                    break;
                }
                catch (ObjectDisposedException)
                {
                    _connection = new HubConnectionBuilder().WithUrl(URL).Build();
                    await Task.Delay(100).ConfigureAwait(false);
                }
                catch
                {
                    await Task.Delay(100).ConfigureAwait(false);
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
