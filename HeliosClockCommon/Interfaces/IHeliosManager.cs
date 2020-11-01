using HeliosClockCommon.Enumerations;
using HeliosClockCommon.LedCommon;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace HeliosClockCommon.Interfaces
{
    public interface IHeliosManager
    {
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
        Task SetOnOff(string onOff);

        /// <summary>Runs the led mode.</summary>
        /// <param name="mode">The mode.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task RunLedMode(LedMode mode, CancellationToken cancellationToken);

        /// <summary>Sets the color.</summary>
        /// <param name="startColor">The start color.</param>
        /// <param name="endColor">The end color.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task SetColor(Color startColor, Color endColor, ColorInterpolationMode interpolationMode, CancellationToken cancellationToken);
    }
}
