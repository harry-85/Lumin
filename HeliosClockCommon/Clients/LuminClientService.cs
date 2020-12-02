using HeliosClockCommon.Configurator;
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
    public partial class LuminClientService : IHostedService
    {
        private readonly ILogger<LuminClientService> _logger;
        private HubConnection connection;
        private readonly ILuminManager manager;
        private readonly ILedController ledController;
        private bool isConnecting;
        private bool isRunning = false;
        public ConfigureService ConfigureService { get; }
        public string ClientId => connection.ConnectionId;

        /// <summary>Initializes a new instance of the <see cref="LuminClientService"/> class.</summary>
        /// <param name="logger">The logger.</param>
        /// <param name="manager">The manager.</param>
        public LuminClientService(ILogger<LuminClientService> logger, ILuminManager manager, ConfigureService configureService)
        {
            _logger = logger;
            _logger.LogInformation("Initializing LuminClient ...");

            ConfigureService = configureService;

            this.manager = manager;
            ledController = manager.LedController;
            _logger.LogInformation("LuminClient Initialized...");
        }

        /// <summary>Initializes this instance.</summary>
        private async Task Initialize()
        {
            manager.NotifyController += async (s, e) =>
            {
                await connection.InvokeAsync(nameof(IHeliosHub.NotifyController), ColorHelpers.HexConverter(e.StartColor), ColorHelpers.HexConverter(e.EndColor)).ConfigureAwait(false);
            };

            connection.On<string, string, string>(nameof(IHeliosHub.SetColorString), SetColor);
            connection.On(nameof(IHeliosHub.SetRandomColor), SetRandomColor);
            connection.On<string>(nameof(IHeliosHub.StartMode), OnStartMode);
            connection.On(nameof(IHeliosHub.Stop), OnStop);
            connection.On<string>(nameof(IHeliosHub.SetRefreshSpeed), OnSetRefreshSpeed);
            connection.On<string, string>(nameof(IHeliosHub.SetOnOff), SetOnOff);
            connection.On<string>(nameof(IHeliosHub.SetBrightness), SetBrightness);

            connection.Reconnected += _connection_Reconnected;

            await ConfigureService.ReadLuminConfig().ConfigureAwait(false);

            //Set led color count of LedController
            ledController.LedCount = ConfigureService.Config.LedCount;

            _logger.LogInformation("Local Lumin Client Initialized ...");
        }

        /// <summary>On reconnected.</summary>
        /// <param name="arg">The argument.</param>
        private async Task _connection_Reconnected(string arg)
        {
            await RegisterAsLedClient().ConfigureAwait(false);
        }

        /// <summary>Sets the on off.</summary>
        /// <param name="onOff">The on off.</param>
        private async Task SetOnOff(string onOff, string side)
        {
            _logger.LogDebug("Local On / Off Command : {0} ...", onOff);
            await manager.SetOnOff((PowerOnOff)Enum.Parse(typeof(PowerOnOff), onOff), (LedSide)Enum.Parse(typeof(LedSide), side), Color.White).ConfigureAwait(false);
        }

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

        private async Task RegisterAsLedClient()
        {
            await connection.InvokeAsync<string>(nameof(IHeliosHub.RegisterAsLedClient), ClientId, ConfigureService.Config.Name).ConfigureAwait(false);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Lumin Client ...");
            DiscoveryClient discoveryClient = new DiscoveryClient();
            discoveryClient.OnIpDiscovered += async (s, e) =>
            {
                _logger.LogInformation("Server IP Discovered: {0} ...", e.Args);
                discoveryClient.StopDiscoveryClient();
                await ConnectToServer(cancellationToken, e.Args).ConfigureAwait(false);
            };

            await discoveryClient.StartDiscoveryClient(cancellationToken).ConfigureAwait(false);
        }

        private async Task ConnectToServer(CancellationToken cancellationToken, IPAddress serverIpAddress)
        {
            if (isConnecting)
                return;

            isConnecting = true;

            string URL = string.Format(DefaultValues.HubUrl, serverIpAddress.ToString(), DefaultValues.SignalPortOne);
            _logger.LogInformation("Connecting to Server with address: {0} ...", URL);

            connection = new HubConnectionBuilder().WithUrl(URL).WithAutomaticReconnect().Build();

            await Task.Run(async () =>
            {
                await Initialize().ConfigureAwait(false);

                _logger.LogInformation("Local Client: Connecting ...");
                // Loop is here to wait until the server is running
                while (connection.State != HubConnectionState.Connected && !cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        await connection.StartAsync(cancellationToken).ConfigureAwait(false); ;

                        while (connection.State == HubConnectionState.Connecting && !cancellationToken.IsCancellationRequested)
                        {
                            await Task.Delay(1000).ConfigureAwait(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("Local Client: Error Connecting: {0} ...", ex.Message);
                        await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
                    }
                }

                _logger.LogInformation("Local Client: Connection Successfully. Status: {0} ...", connection.State.ToString());
            }).ConfigureAwait(false);

            if(!cancellationToken.IsCancellationRequested)
                await RegisterAsLedClient().ConfigureAwait(false);

            isConnecting = false;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await connection.DisposeAsync().ConfigureAwait(false);
        }
    }
}
