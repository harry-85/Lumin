using System;
using System.Drawing;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HeliosClockCommon.Clients;
using HeliosClockCommon.Discorvery;
using HeliosClockCommon.Enumerations;
using HeliosClockCommon.Models;

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
        private readonly DiscoveryClient discoveryClient;

        public LedClientManager LedClientManager;

        public LedClient ActiveLedClient { get; set; } = LedClient.AllClients;

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
            LedClientManager = new LedClientManager();
            discoveryClient = new DiscoveryClient(new DiscoverFactory());

            Client = new HeliosAppClient();
            Client.OnConnected += async (s, e) =>
            {
                OnConnected?.Invoke(this, e);

                await Client.RegisterAsController().ConfigureAwait(false);

                if (e.Args)
                {
                    await StopMode().ConfigureAwait(false);
                    await SetBrightness(255).ConfigureAwait(false);
                }
            };

            Client.LedClientChanged += (s, e) =>
            {
                LedClientManager.SetNewClients(e.Args);
            };

            Client.OnControllerNotified += (s, e) =>
            {
                StartColor = e.StartColor;
                EndColor = e.EndColor;
            };

            discoveryClient.OnIpDiscovered += DiscoveryClient_OnIpDiscovered;

        }

        /// <summary>Sends the color to the server.</summary>
        public async Task SendColor()
        {
            try
            {
                await Client.SendColor(ActiveLedClient, StartColor, EndColor).ConfigureAwait(false);
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
                await Client.SetBrightness(ActiveLedClient, brightness.ToString()).ConfigureAwait(false);
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
                await Client.SetOnOff(ActiveLedClient, onOff.ToString(), side.ToString()).ConfigureAwait(false);
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
                await Client.StartMode(ActiveLedClient, mode.ToString()).ConfigureAwait(false);
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
                await Client.SetRefreshSpeed(ActiveLedClient, speed.ToString()).ConfigureAwait(false);
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
                await Client.Stop(ActiveLedClient).ConfigureAwait(false);
            }
            catch
            {
            }
        }

        /// <summary>Connects to server.</summary>
        public async Task StartAsync(IPAddress serverAddress)
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = new CancellationTokenSource();
            token = cancellationTokenSource.Token;

            Client.IPAddress = serverAddress;
            await Client.StartAsync(token).ConfigureAwait(false);
        }


        /// <summary>Handles the OnIpDiscovered event of the DiscoveryClient control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs{IPAddress}"/> instance containing the event data.</param>
        private async void DiscoveryClient_OnIpDiscovered(object sender, EventArgs<IPAddress> e)
        {
            discoveryClient.StopDiscoveryClient();
            await StartAsync(e.Args).ConfigureAwait(false);
        }

        /// <summary>Connects to server.</summary>
        /// <param name="cancellationToken"></param>
        public async Task ConnectToServer(CancellationToken cancellationToken)
        {
            //Throws event: DiscoveryClient_OnIpDiscovered, where program will continue
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
