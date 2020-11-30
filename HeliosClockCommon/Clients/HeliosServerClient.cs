using HeliosClockCommon.Defaults;
using HeliosClockCommon.Discorvery;
using HeliosClockCommon.Enumerations;
using HeliosClockCommon.Helper;
using HeliosClockCommon.Interfaces;
using HeliosClockCommon.LedCommon;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Drawing;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace HeliosClockCommon.Clients
{
    public partial class HeliosServerClient : IHostedService
    {
        private readonly ILogger<HeliosServerClient> _logger;
        private HubConnection _connection;
        private readonly IHeliosManager manager;
        private readonly ILedController ledController;

        /// <summary>Initializes a new instance of the <see cref="HeliosServerClient"/> class.</summary>
        /// <param name="logger">The logger.</param>
        /// <param name="manager">The manager.</param>
        public HeliosServerClient(ILogger<HeliosServerClient> logger, IHeliosManager manager)
        {
            this.manager = manager;
            ledController = manager.LedController;

            _logger = logger;
        }

        /// <summary>Initializes this instance.</summary>
        private void Initialize()
        {
           

            _connection.On<string, string, string>(nameof(IHeliosHub.SetColorString), SetColor);
            _connection.On(nameof(IHeliosHub.SetRandomColor), SetRandomColor);
            _connection.On<string>(nameof(IHeliosHub.StartMode), OnStartMode);
            _connection.On(nameof(IHeliosHub.Stop), OnStop);
            _connection.On<string>(nameof(IHeliosHub.SetRefreshSpeed), OnSetRefreshSpeed);
            _connection.On<string, string>(nameof(IHeliosHub.SetOnOff), SetOnOff);
            _connection.On< string>(nameof(IHeliosHub.SetBrightness), SetBrightness);

            _logger.LogInformation("Local Helios Client Initialized ...");
        }

        /// <summary>Sets the on off.</summary>
        /// <param name="onOff">The on off.</param>
        private async Task SetOnOff(string onOff, string side)
        {
            _logger.LogDebug("Local Helios On / Off Command : {0} ...", onOff);
            await manager.SetOnOff((PowerOnOff)Enum.Parse(typeof(PowerOnOff), onOff), (LedSide)Enum.Parse(typeof(LedSide), side), Color.White).ConfigureAwait(false);
        }
        bool isRunning = false;
        private Task SetColor(string startColor, string endColor, string interpolationMode)
        {
            //return if color change is already in progress
            if (isRunning) return Task.CompletedTask;

            // Start task to avoid input jam
            Task.Run(async () =>
            {
                isRunning = true;
                _logger.LogDebug("Local Color Change: Start: {0} - End: {1} ...", startColor, endColor);
                await manager.SetColor(ColorHelpers.FromHex(startColor), ColorHelpers.FromHex(endColor),(ColorInterpolationMode)Enum.Parse(typeof(ColorInterpolationMode), interpolationMode)).ConfigureAwait(false);
                isRunning = false;
            });

            return Task.CompletedTask;
        }

        private async Task OnStartMode(string mode)
        {
            await OnStop().ConfigureAwait(false);

            _logger.LogDebug("Local Client: Mode change to: {0} ...", mode);

            Enum.TryParse(mode, out LedMode ledMode);
            await manager.RunLedMode(ledMode).ConfigureAwait(false);
        }

        private Task OnSetRefreshSpeed(string speed)
        {
            _logger.LogDebug("Set refresh speed: {0} ...", speed);
            manager.RefreshSpeed = int.Parse(speed);
            return Task.CompletedTask;
        }

        private async Task SetRandomColor()
        {
            await manager.SetRandomColor().ConfigureAwait(false);
        }

        private async Task SetBrightness(string brightness)
        {
            _logger.LogDebug("Set Brightness level to: {0} ...", brightness);
            manager.Brightness = int.Parse(brightness);
            await manager.RefreshScreen().ConfigureAwait(false);
        }

        private async Task OnStop()
        {
            _logger.LogDebug("Local Client: Mode stop command ...");
            await manager.StopLedMode().ConfigureAwait(false);
        }

        private Task SetAlarm(DateTime alarmTime)
        {
            throw new NotImplementedException();
        }

        private Task SignalClient(string user, string message)
        {
            throw new NotImplementedException();
        }

        private bool isConnecting;
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            DiscoveryClient discoveryClient = new DiscoveryClient();
            discoveryClient.OnIpDiscovered += async (s, e) =>
            {
                discoveryClient.StopDiscoveryClient();
                await ConnectToServer(cancellationToken, e.Args).ConfigureAwait(false);
            };

            await discoveryClient.StartDiscoveryCient(cancellationToken).ConfigureAwait(false);
        }

        private async Task ConnectToServer(CancellationToken cancellationToken, IPAddress serverIpAddress)
        {
            if (isConnecting)
                return;

            isConnecting = true;

            string URL = string.Format(DefaultValues.HubUrl, serverIpAddress.ToString(), DefaultValues.SignalPortOne);
            _logger.LogInformation("Connecting to Server with address: {0} ...", URL);

            _connection = new HubConnectionBuilder().WithUrl(URL).WithAutomaticReconnect().Build();

            await Task.Run(async () =>
            {
                Initialize();

                _logger.LogInformation("Local Client: Connecting ...");
                // Loop is here to wait until the server is running
                while (_connection.State != HubConnectionState.Connected && !cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        await _connection.StartAsync(cancellationToken).ConfigureAwait(false); ;

                        while (_connection.State == HubConnectionState.Connecting && !cancellationToken.IsCancellationRequested)
                        {
                            await Task.Delay(1000).ConfigureAwait(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("Local Client: Error Connecting: {0}", ex.Message);
                        await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
                    }
                }

                _logger.LogInformation("Local Client: Connection Successfully ... Status: {0}", _connection.State.ToString());
            }).ConfigureAwait(false);

            isConnecting = false;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _connection.DisposeAsync().ConfigureAwait(false);
        }
    }
}
