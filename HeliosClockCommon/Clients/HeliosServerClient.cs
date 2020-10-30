using HeliosClockCommon.Defaults;
using HeliosClockCommon.Enumerations;
using HeliosClockCommon.Helper;
using HeliosClockCommon.Interfaces;
using HeliosClockCommon.LedCommon;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Drawing;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace HeliosClockCommon.Clients
{
    public partial class HeliosServerClient : IHostedService
    {
        private readonly ILogger<HeliosServerClient> _logger;
        private HubConnection _connection;
        private IHeliosManager manager;
        private ILedController ledController;

        private CancellationTokenSource cancellationTokenSource;
        private CancellationToken cancellationToken;

        public HeliosServerClient(ILogger<HeliosServerClient> logger, IHeliosManager manager)
        {
            this.manager = manager;
            ledController = manager.LedController;

            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;

            _logger = logger;
        }

        private void Initialize()
        {
            string URL = string.Format(DefaultValues.HubUrl, "localhost", DefaultValues.SignalPortOne);
            _logger.LogInformation(URL);

            _connection = new HubConnectionBuilder().WithUrl(URL).Build();

            _connection.On<string, string>(nameof(IHeliosHub.SetColorString), async (startColor, endColor) =>
            {
                _logger.LogInformation("Incomming Color Change ...");

                var leds = new LedScreen(ledController);

                manager.StartColor = ColorHelpers.FromHex(startColor);
                manager.EndColor = ColorHelpers.FromHex(endColor);

                var colors = await ColorHelpers.ColorGradient(manager.StartColor, ColorHelpers.FromHex(endColor), ledController.LedCount).ConfigureAwait(false);

                for (int i = 0; i < ledController.LedCount; i++)
                {
                    leds.SetPixel(ref i, colors[i]);
                }

                ledController.IsSmoothing = true;
                await ledController.SendPixels(leds.pixels).ConfigureAwait(false);
                ledController.IsSmoothing = false;
            });

            _connection.On<string>(nameof(IHeliosHub.StartMode), OnStartMode);
            _connection.On(nameof(IHeliosHub.Stop), OnStop);
            _connection.On<string>(nameof(IHeliosHub.SetRefreshSpeed), OnSetRefreshSpeed);
            _connection.On<string>(nameof(IHeliosHub.SetOnOff), SetOnOff);

            _logger.LogInformation("Local Helios Client Initialized ...");
        }

        /// <summary>Sets the on off.</summary>
        /// <param name="onOff">The on off.</param>
        public async Task SetOnOff(string onOff)
        {
            _logger.LogInformation("Local Helios On / Off Command : {0} ...", onOff);
            await manager.SetOnOff(onOff).ConfigureAwait(false);
        }

        public Task SetAlarm(DateTime alarmTime)
        {
            throw new NotImplementedException();
        }

        public Task SignalClient(string user, string message)
        {
            throw new NotImplementedException();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Initialize();
           
            _logger.LogInformation("Local Client: Connecting ...");
            // Loop is here to wait until the server is running
            while (_connection.State != HubConnectionState.Connected && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await _connection.StartAsync(cancellationToken);

                    // _logger.LogInformation("Local Client: Status: {0} ...", _connection.State.ToString());

                    // break;

                    while (_connection.State == HubConnectionState.Connecting && !cancellationToken.IsCancellationRequested)
                    {
                        await Task.Delay(1000);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Local Client: Error Connecting: {0}", ex.Message);
                    await Task.Delay(1000, cancellationToken);
                }
            }

            _logger.LogInformation("Local Client: Connection Successfully ... Status: {0}", _connection.State.ToString());
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _connection.DisposeAsync().ConfigureAwait(false);
        }

        private async Task OnStartMode(string mode)
        {
            await OnStop().ConfigureAwait(false);

            _logger.LogInformation("Local Client: Mode change to: {0} ...", mode);

            Enum.TryParse(mode, out LedMode ledMode);
            await manager.RunLedMode(ledMode, cancellationToken).ConfigureAwait(false);
        }

        private Task OnSetRefreshSpeed(string speed)
        {
            _logger.LogInformation("Set refresh speed: {0} ...", speed);
            manager.RefreshSpeed = int.Parse(speed);
            return Task.CompletedTask;
        }

        private Task OnStop()
        {
            _logger.LogInformation("Local Client: Mode stop command ...");

            cancellationTokenSource.Cancel();
            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;

            return Task.CompletedTask;
        }

    }
}
