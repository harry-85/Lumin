using HeliosClockCommon.Clients;
using HeliosClockCommon.Enumerations;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Systems;

namespace HeliosClockApp.Services
{
    public class HeliosService : IHeliosService
    {
        public HeliosAppClient Client { get; set; }
        private CancellationTokenSource cancellationTokenSource;
        public CancellationToken token;

        public event EventHandler<EventArgs<Color>> OnStartColorChanged;
        public event EventHandler<EventArgs<Color>> OnEndColorChanged;

        private Color startColor;
        private Color endColor;

        public Color StartColor
        {
            get => startColor;
            set
            {
                startColor = value;
                OnStartColorChanged?.Invoke(this, new EventArgs<Color>(startColor));
            }
        }
        public Color EndColor
        {
            get => endColor;
            set
            {
                endColor = value;
                OnEndColorChanged?.Invoke(this, new EventArgs<Color>(endColor));
            }
        }

        public HeliosService()
        {
            Client = new HeliosAppClient();
        }

        public async Task SendColor()
        {
            await Client.SendColor(StartColor, EndColor).ConfigureAwait(false);
        }

        public async Task StartMode(LedMode mode)
        {
            await Client.StartMode(mode.ToString()).ConfigureAwait(false);
        }

        public async Task Stop()
        {
            await Client.Stop().ConfigureAwait(false);
        }

        public async Task ConnectToServer()
        {
            cancellationTokenSource = new CancellationTokenSource();
            token = cancellationTokenSource.Token;

            await Client.StartAsync(token).ConfigureAwait(false);
        }

        public async Task StopServer()
        {
            cancellationTokenSource?.Cancel();
            await Client.StopAsync().ConfigureAwait(false);
        }
    }
}
