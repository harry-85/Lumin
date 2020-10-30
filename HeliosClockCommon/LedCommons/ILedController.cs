using HeliosClockCommon.ServiceCommons;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace HeliosClockCommon.LedCommon
{
    /// <summary>
    /// Defines the interface for an LED Controller. 
    /// </summary>
    /// <seealso cref="IDisposable" />
    public interface ILedController : IDisposable
    {
        /// <summary>Gets or sets a value indicating whether this instance is smoothing.</summary>
        /// <value><c>true</c> if this instance is smoothing; otherwise, <c>false</c>.</value>
        public bool IsSmoothing { get; set; }

        /// <summary>
        /// Gets or sets the actual screen.
        /// </summary>
        /// <value>
        /// The actual screen.
        /// </value>
        LedPixel[] ActualScreen { get; set; }

        /// <summary>Handles the PropertyChanged event of the Settings control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e);

        /// <summary>Gets or sets the API service settings.</summary>
        /// <value>The API service settings.</value>
        APIServiceSettings APIServiceSettings { get; set; }

        /// <summary>Gets or sets the clock speed in Hz.</summary>
        /// <value>The clock speed oin Hz.</value>
        int ClockSpeedHz { get; set; }

        /// <summary>Gets or sets the pixel delta.</summary>
        /// <value>The pixel delta.</value>
        int PixelOffset { get; set; }

        /// <summary>Sets the screen colors.</summary>
        /// <param name="screen">The screen.</param>
        Task SetScreenColors(LedScreen screen);

        /// <summary>Sends the pixels.</summary>
        /// <param name="pixels">The pixels.</param>
        Task SendPixels(LedPixel[] pixels);

        /// <summary>Gets the height of the get.</summary>
        /// <value>The height of the get.</value>
        int LedCount { get; set; }

        /// <summary>Repaints this instance.</summary>
        Task Repaint();

        /// <summary>Gets or sets the CancellationToken.</summary>
        /// <value>The token.</value>
        CancellationToken Token { get; set; }
    }
}
