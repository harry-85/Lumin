using System;
using System.Drawing;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HeliosClockCommon.Configurator;
using HeliosClockCommon.Defaults;
using HeliosClockCommon.Discorvery;
using HeliosClockCommon.Enumerations;
using HeliosClockCommon.EventArgs;
using HeliosClockCommon.Helper;
using HeliosClockCommon.Interfaces;
using HeliosClockCommon.LedCommon;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
        public IConfigureService ConfigureService { get; }
        public string ClientId => connection.ConnectionId;

        /// <summary>Initializes a new instance of the <see cref="LuminClientService"/> class.</summary>
        /// <param name="logger">The logger.</param>
        /// <param name="manager">The manager.</param>
        public LuminClientService(ILogger<LuminClientService> logger, ILuminManager manager, IConfigureService configureService)
        {
            _logger = logger;
            _logger.LogInformation("Initializing LuminClient ...");

            ConfigureService = configureService;

            this.manager = manager;
            ledController = manager.LedController;
            _logger.LogInformation("LuminClient Initialized...");
        }

        /// <summary>Initializes the lumin clients, sets all SignalR listeners and other events.</summary>
        private async Task Initialize()
        {
            manager.NotifyController += async (s, e) => await NotifyController(e).ConfigureAwait(false);

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

        /// <summary>Sets the color.</summary>
        /// <param name="startColor">The start color.</param>
        /// <param name="endColor">The end color.</param>
        /// <param name="interpolationMode">The interpolation mode.</param>
        private Task SetColor(string startColor, string endColor, string interpolationMode)
        {
            //return if color change is already in progress
            if (isRunning) return Task.CompletedTask;

            // Start task to avoid input jam
            Task.Run(async () =>
            {
                isRunning = true;
                _logger.LogDebug("Local Color Change: Start: {0} - End: {1} ...", startColor, endColor);
                await manager.SetColor(
                    ColorHelpers.FromHex(startColor),
                    ColorHelpers.FromHex(endColor),
                    (ColorInterpolationMode)Enum.Parse(typeof(ColorInterpolationMode),
                    interpolationMode)).ConfigureAwait(false);
                isRunning = false;
            });

            return Task.CompletedTask;
        }

        /// <summary>Called when [start mode].</summary>
        /// <param name="mode">The mode.</param>
        private async Task OnStartMode(string mode)
        {
            await OnStop().ConfigureAwait(false);

            _logger.LogDebug("Local Client: Mode change to: {0} ...", mode);

            Enum.TryParse(mode, out LedMode ledMode);
            await manager.RunLedMode(ledMode).ConfigureAwait(false);
        }

        /// <summary>Called when [set refresh speed].</summary>
        /// <param name="speed">The speed.</param>
        private Task OnSetRefreshSpeed(string speed)
        {
            _logger.LogDebug("Set refresh speed: {0} ...", speed);
            manager.RefreshSpeed = int.Parse(speed);
            return Task.CompletedTask;
        }

        /// <summary>Sets the random color.</summary>
        private async Task SetRandomColor()
        {
            await manager.SetRandomColor().ConfigureAwait(false);
        }

        /// <summary>Sets the brightness.</summary>
        /// <param name="brightness">The brightness.</param>
        private async Task SetBrightness(string brightness)
        {
            _logger.LogDebug("Set Brightness level to: {0} ...", brightness);
            manager.Brightness = int.Parse(brightness);
            await manager.RefreshScreen().ConfigureAwait(false);
        }

        /// <summary>Called when stop command send.</summary>
        private async Task OnStop()
        {
            _logger.LogDebug("Local Client: Mode stop command ...");
            await manager.StopLedMode().ConfigureAwait(false);
        }

        /// <summary>Registers as led client.</summary>
        private async Task RegisterAsLedClient()
        {
            try
            {
                await connection.InvokeAsync<string>(nameof(IHeliosHub.RegisterAsLedClient), ClientId, ConfigureService.Config.Name).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Error in registering as LED Client. Message: {0} ...", ex.Message);
            }
        }

        /// <summary>Notifies the controller.</summary>
        private async Task NotifyController(NotifyControllerEventArgs notifyControllerEventArgs)
        {
            try
            {
                await connection.InvokeAsync(
                    nameof(IHeliosHub.NotifyController),
                    ColorHelpers.HexConverter(notifyControllerEventArgs.StartColor),
                    ColorHelpers.HexConverter(notifyControllerEventArgs.EndColor)).ConfigureAwait(false);
            }
            catch(Exception ex)
            {
                _logger.LogWarning("Error notifying controller. Message: {0} ...", ex.Message);
            }
        }

        /// <summary>Triggered when the application host is ready to start the service.</summary>
        /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
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

        /// <summary>Connects to server.</summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="serverIpAddress">The server IP address.</param>
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

            if (!cancellationToken.IsCancellationRequested)
                await RegisterAsLedClient().ConfigureAwait(false);

            isConnecting = false;
        }

        /// <summary>Triggered when the application host is performing a graceful shutdown.</summary>
        /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await connection.DisposeAsync().ConfigureAwait(false);
        }
    }
}
