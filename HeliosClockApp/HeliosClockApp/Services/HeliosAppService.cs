using HeliosClockCommon.Clients;
using HeliosClockCommon.Enumerations;
using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace HeliosClockApp.Services
{
    public class HeliosAppService : IHeliosAppService
    {
        public HeliosAppClient Client { get; set; }

        public CancellationToken token;

        public event EventHandler<EventArgs<Color>> OnStartColorChanged;
        public event EventHandler<EventArgs<Color>> OnEndColorChanged;
        public event EventHandler<EventArgs<bool>> OnConnected;
        public event EventHandler<EventArgs<LedMode>> OnModeChange;

        private CancellationTokenSource cancellationTokenSource;
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

        public HeliosAppService()
        {
            Client = new HeliosAppClient();
            Client.OnConnected += async (s, e) =>
            {
                OnConnected?.Invoke(this, e);

                if (e.Args)
                {
                    await StopMode().ConfigureAwait(false);
                }
            };
        }

        /// <summary>Sends the color to the server.</summary>
        public async Task SendColor()
        {
            try
            {
                await Client.SendColor(StartColor, EndColor).ConfigureAwait(false);
            }
            catch
            {
            }
        }

        public async Task SetOnOff(string onOff)
        {
            try
            {
                await Client.SetOnOff(onOff).ConfigureAwait(false);
                await StopMode().ConfigureAwait(false);
            }
            catch
            {
            }
        }

        public async Task StartMode(LedMode mode)
        {
            try
            {
                OnModeChange?.Invoke(this, new EventArgs<LedMode>(mode));
                await Client.StartMode(mode.ToString()).ConfigureAwait(false);
            }
            catch
            {
            }
        }

        public async Task SetRefreshSpeed(int speed)
        {
            try
            {
                await Client.SetRefreshSpeed(speed.ToString()).ConfigureAwait(false);
            }
            catch
            {
            }
        }

        public async Task StopMode()
        {
            try
            {
                OnModeChange?.Invoke(this, new EventArgs<LedMode>(LedMode.None));
                await Client.Stop().ConfigureAwait(false);
            }
            catch
            {
            }
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
