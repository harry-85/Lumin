using HeliosClockCommon.ServiceCommons;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace HeliosClockCommon.LedCommon
{
    public abstract class AbstractLedController : ILedController
    {
        /// <summary>The disposed indicator.</summary>
        protected bool disposed = false;

        public int DimRatio { get; set; }

        /// <summary>Gets or sets a value indicating whether this instance is smoothing.</summary>
        /// <value><c>true</c> if this instance is smoothing; otherwise, <c>false</c>.</value>
        public bool IsSmoothing { get; set; }

        /// <summary>Gets or sets the API service settings.</summary>
        /// <value>The API service settings.</value>
        public APIServiceSettings APIServiceSettings { get; set; }

        /// <summary>Initializes a new instance of the <see cref="AbstractLedController"/> class.</summary>
        /// <param name="settings">The settings.</param>
        public AbstractLedController() 
        {
            DimRatio = 100;
        }

        public static ILedController CreateController<T>(APIServiceSettings serviceSettings) where T : ILedController, new()
        {
            T controller = new T
            {
                APIServiceSettings = serviceSettings
            };
            serviceSettings.PropertyChanged += controller.Settings_PropertyChanged;
            return controller;
        }

        public LedPixel[] ActualScreen { get; set; }

        /// <summary>Handles the PropertyChanged event of the Settings control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        public virtual void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(APIServiceSettings.ScreenPixelOffset):
                    PixelOffset = APIServiceSettings.ScreenPixelOffset;
                    break;
                case nameof(APIServiceSettings.LedCount):
                    LedCount = APIServiceSettings.LedCount;
                    break;
            }
        }

        /// <summary>Gets or sets the clock speed in Hz.</summary>
        /// <value>The clock speed oin Hz.</value>
        public int ClockSpeedHz { get; set; }
        /// <summary>Gets or sets the pixel delta.</summary>
        /// <value>The pixel delta.</value>
        public int PixelOffset { get; set; }

        public abstract Task SendPixels(LedPixel[] pixels);

        /// <summary>Sets the screen colors.</summary>
        /// <param name="screen">The screen.</param>
        public abstract Task SetScreenColors(LedScreen screen);

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public abstract void Dispose();

        /// <summary>Gets the pixel count.</summary>
        /// <value>The pixel count.</value>
        public virtual int LedCount { get; set; }

        /// <summary>Gets or sets the CancellationToken.</summary>
        /// <value>The token.</value>
        public CancellationToken Token { get; set; }
       
        /// <summary>Repaints this instance.</summary>
        public async Task Repaint()
        {
            if (ActualScreen == null)
                return;
            await SendPixels(ActualScreen).ConfigureAwait(false);
        }
    }
}
