using HeliosClockCommon.Enumerations;
using HeliosClockCommon.LedCommon;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace HeliosClockCommon.Interfaces
{
    public interface IHeliosManager
    {
        int Brightness { get; set; }

        /// <summary>Gets a value indicating whether this instance is running.</summary>
        /// <value><c>true</c> if this instance is running; otherwise, <c>false</c>.</value>
        bool IsRunning { get; }

        /// <summary>Gets or sets the led controller.</summary>
        /// <value>The led controller.</value>
        ILedController LedController { get; set; }

        /// <summary>Gets or sets the start color.</summary>
        /// <value>The start color.</value>
        Color StartColor { get; set; }

        /// <summary>Gets or sets the end color.</summary>
        /// <value>The end color.</value>
        Color EndColor { get; set; }

        /// <summary>Gets or sets the automatic off time in [ms].</summary>
        /// <value>The automatic off time.</value>
        double AutoOffTime { get; set; }

        /// <summary>Gets or sets the refresh speed.</summary>
        /// <value>The refresh speed.</value>
        int RefreshSpeed { get; set; }

        /// <summary>Sets the on off.</summary>
        /// <param name="onOff">The on off.</param>
        /// <returns></returns>
        Task SetOnOff(PowerOnOff onOff, LedSide side, Color color);

        /// <summary>Runs the led mode.</summary>
        /// <param name="mode">The mode.</param>
        /// <returns></returns>
        Task RunLedMode(LedMode mode);

        /// <summary>Stops the led mode.</summary>
        Task StopLedMode();

        /// <summary>Sets the color.</summary>
        /// <param name="startColor">The start color.</param>
        /// <param name="endColor">The end color.</param>
        /// <returns></returns>
        Task SetColor(Color startColor, Color endColor, ColorInterpolationMode interpolationMode);

        /// <summary>Sets the random color.</summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task SetRandomColor();

        /// <summary>Refreshes the screen.</summary>
        Task RefreshScreen();
    }
}
