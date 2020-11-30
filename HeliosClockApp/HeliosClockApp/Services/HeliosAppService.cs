using HeliosClockCommon.Clients;
using HeliosClockCommon.Discorvery;
using HeliosClockCommon.Enumerations;
using System;
using System.Drawing;
using System.Net;
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
        public event EventHandler<EventArgs> OnDisconnected;
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

        /// <summary>Initializes a new instance of the <see cref="HeliosAppService"/> class.</summary>
        public HeliosAppService()
        {
            Client = new HeliosAppClient();
            Client.OnConnected += async (s, e) =>
            {
                OnConnected?.Invoke(this, e);

                if (e.Args)
                {
                    await StopMode().ConfigureAwait(false);
                    await SetBrightness(255).ConfigureAwait(false);
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

        /// <summary>Sets the brightness.</summary>
        /// <param name="brightness">The brightness.</param>
        public async Task SetBrightness(int brightness)
        {
            try
            {
                await Client.SetBrightness(brightness.ToString()).ConfigureAwait(false);
            }
            catch
            {
            }
        }

        /// <summary>Sets the on off.</summary>
        /// <param name="onOff">The on off.</param>
        /// <param name="side"></param>
        public async Task SetOnOff(PowerOnOff onOff, LedSide side = LedSide.Full)
        {
            try
            {
                await Client.SetOnOff(onOff.ToString(), side.ToString()).ConfigureAwait(false);
                await StopMode().ConfigureAwait(false);
            }
            catch
            {
            }
        }

        /// <summary>Starts the mode.</summary>
        /// <param name="mode">The mode.</param>
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

        /// <summary>Sets the refresh speed of the actual running task on the server.</summary>
        /// <param name="speed">The speed.</param>
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

        /// <summary>Stops the actual running task on the server.</summary>
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

        /// <summary>Connects to server.</summary>
        public async Task StartAsync(CancellationToken cancellationToken, IPAddress serverAddress)
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = new CancellationTokenSource();
            token = cancellationTokenSource.Token;

            Client.IPAddress = serverAddress;
            await Client.StartAsync(token).ConfigureAwait(false);
        }

        /// <summary>Connects to server.</summary>
        /// <param name="cancellationToken"></param>
        public async Task ConnectToServer(CancellationToken cancellationToken)
        {
            DiscoveryClient discoveryClient = new DiscoveryClient();
            discoveryClient.OnIpDiscovered += async (s, e) =>
            {
                discoveryClient.StopDiscoveryClient();
                await StartAsync(cancellationToken, e.Args).ConfigureAwait(false);
            };

            await discoveryClient.StartDiscoveryClient(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Stops the server.</summary>
        public async Task StopServer()
        {
            cancellationTokenSource?.Cancel();
            await Client.StopAsync().ConfigureAwait(false);
            OnDisconnected?.Invoke(this, new EventArgs());
        }
    }
}
