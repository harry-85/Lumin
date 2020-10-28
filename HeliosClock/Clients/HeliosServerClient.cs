using HeliosClock.Interfaces;
using HeliosClockAPI.Controller;
using HeliosClockCommon.Defaults;
using HeliosClockCommon.Helper;
using HeliosClockCommon.LedCommon;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace HeliosClockAPI.Clients
{
    public partial class HeliosServerClient : IHeliosHub, IHostedService
    {
        private readonly ILogger<HeliosServerClient> _logger;
        private HubConnection _connection;
        private ILedController ledController;

        public HeliosServerClient(ILogger<HeliosServerClient> logger)
        {
            ledController = new LedAPA102Controller();
            ledController.LedCount = 59;

            _logger = logger;
            _connection = new HubConnectionBuilder().WithUrl(string.Format(DefaultValues.HubUrl, DefaultValues.Port, "localhost")).Build();
            _connection.On<Color, Color>(nameof(IHeliosHub.SetColor), SetColor);
        }

        public Task SetAlarm(DateTime alarmTime)
        {
            throw new NotImplementedException();
        }

        public Task SetColor(Color startColor, Color endColor)
        {
            var leds = new LedScreen(ledController);

            var colors = ColorHelpers.ColorGradient(startColor, endColor, ledController.LedCount);

            for (int i = 0; i < ledController.LedCount; i++)
            {
                leds.SetPixel(ref i, colors[i]);
            }

            ledController.SendPixels(leds.pixels);
            return Task.CompletedTask;
        }

        public Task SignalClient(string user, string message)
        {
            throw new NotImplementedException();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
