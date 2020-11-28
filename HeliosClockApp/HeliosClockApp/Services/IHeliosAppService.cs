using HeliosClockCommon.Clients;
using HeliosClockCommon.Enumerations;
using System;
using System.Drawing;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace HeliosClockApp.Services
{
    public interface IHeliosAppService
    {
        /// <summary>Occurs when [on start color changed].</summary>
        event EventHandler<EventArgs<Color>> OnStartColorChanged;

        /// <summary>Occurs when [on end color changed].</summary>
        event EventHandler<EventArgs<Color>> OnEndColorChanged;

        /// <summary>Occurs when [on connected].</summary>
        event EventHandler<EventArgs<bool>> OnConnected;

        /// <summary>Occurs when [on mode change].</summary>
        event EventHandler<EventArgs<LedMode>> OnModeChange;

        /// <summary>Gets or sets the start color.</summary>
        /// <value>The start color.</value>
        public Color StartColor { get; set; }

        /// <summary>Gets or sets the end color.</summary>
        /// <value>The end color.</value>
        public Color EndColor { get; set; }

        /// <summary>Gets or sets the client.</summary>
        /// <value>The client.</value>
        HeliosAppClient Client { get; set; }

        /// <summary>Sends the color.</summary>
        Task SendColor();

        /// <summary>Connects to server.</summary>
        Task ConnectToServer(CancellationToken cancellationToken);

        /// <summary>Starts the mode.</summary>
        /// <param name="mode">The mode.</param>
        Task StartMode(LedMode mode);

        /// <summary>Sets the on off.</summary>
        /// <param name="onOff">The on off.</param>
        Task SetOnOff(PowerOnOff onOff, LedSide side = LedSide.Full);

        /// <summary>Stops the actual running task on the server.</summary>
        Task StopMode();

        /// <summary>Sets the refresh speed of the actual running task on the server.</summary>
        /// <param name="speed">The speed.</param>
        Task SetRefreshSpeed(int speed);

        /// <summary>Sets the brightness.</summary>
        /// <param name="brightness">The brightness.</param>
        Task SetBrightness(int brightness);
    }
}
