using LuminCommon.Enumerations;
using System.ComponentModel;
using System.Drawing;
using System.Dynamic;
using System.Runtime.CompilerServices;

namespace LuminCommon.ServiceCommons
{
    public class APIServiceSettings : INotifyPropertyChanged
    {
        private bool acttiveRainbow;
        private int rainbowSpeed;
        private int fadeSpeed;
        private Color startColor;
        private Color endColor;
        private string videoCaptureDevice;
        private int ledCount;
        private int screenPixelOffset;
        public event PropertyChangedEventHandler PropertyChanged;
        private LedMode ledMode;

        //##############################################################
        //General  Settings
        //##############################################################

        public APIServiceSettings()
        {
            IsValid = true;
        }

        public bool IsValid { get; set; }


        /// <summary>Gets or sets the led mode.</summary>
        /// <value>The led mode.</value>
        public LedMode LedMode
        {
            get => ledMode;
            set
            {
                ledMode = value;
                NotifyPropertyChanged();
            }

        }

        /// <summary>Gets or sets the fade time.</summary>
        /// <value>The fade time.</value>
        public int FadeSpeed
        {
            get => fadeSpeed;
            set
            {
                fadeSpeed = value;
                NotifyPropertyChanged();
            }

        }

        /// <summary>Gets or sets the start color.</summary>
        /// <value>The start color.</value>
        public Color StartColor
        {
            get => startColor;
            set
            {
                startColor = value;
                NotifyPropertyChanged();
            }

        }

        /// <summary>Gets or sets the rainbow speed.</summary>
        /// <value>The rainbow speed.</value>
        public int RainbowSpeed
        {
            get => rainbowSpeed;
            set
            {
                rainbowSpeed = value;
                NotifyPropertyChanged();
            }

        }

        /// <summary>Gets or sets a value indicating whether [active rainbow].</summary>
        /// <value><c>true</c> if [active rainbow]; otherwise, <c>false</c>.</value>
        public bool ActiveRainbow
        {
            get => acttiveRainbow;
            set
            {
                acttiveRainbow = value;
                NotifyPropertyChanged();
            }

        }

        /// <summary>Gets or sets the end color.</summary>
        /// <value>The end color.</value>
        public Color EndColor
        {
            get => endColor;
            set
            {
                endColor = value;
                NotifyPropertyChanged();
            }

        }

        /// <summary>Gets or sets the video capture device.</summary>
        /// <value>The video capture device.</value>
        public string VideoCaptureDevice
        {
            get => videoCaptureDevice;
            set
            {
                videoCaptureDevice = value;
                NotifyPropertyChanged();
            }

        }

        //##############################################################
        //Screen Settings
        //##############################################################

        /// <summary>Gets or sets the Led Countr in pixel (LED Pixels).</summary>
        public int LedCount
        {
            get => ledCount;
            set
            {
                ledCount = value;
                NotifyPropertyChanged();
            }

        }

        /// <summary>Gets or sets the offset in led pixel from the zero point.</summary>
        /// <remarks>Logical zero point is left lower corner. Countedirection is always clockwise.</remarks>
        public int ScreenPixelOffset
        {
            get => screenPixelOffset;
            set
            {
                screenPixelOffset = value;
                NotifyPropertyChanged();
            }

        }


        // This method is called by the Set accessor of each property.  
        // The CallerMemberName attribute that is applied to the optional propertyName  
        // parameter causes the property name of the caller to be substituted as an argument.  
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //public void CloneDataFrom(APIServiceSettings settings)
        //{
        //    LedMode = settings.LedMode;
        //    LedFlowDirection = settings.LedFlowDirection;
        //    ScreenLedHeight = settings.ScreenLedHeight;
        //    ScreenLedWidth = settings.ScreenLedWidth;
        //    ScreenPixelOffset = settings.ScreenPixelOffset;
        //    StartColor = settings.StartColor;
        //    EndColor = settings.EndColor;
        //    FadeSpeed = settings.FadeSpeed;
        //    ScreenPixelOffset = settings.ScreenPixelOffset;
        //    VideoCaptureDevice = settings.VideoCaptureDevice;
        //    ActiveRainbow = settings.acttiveRainbow;
        //    RainbowSpeed = settings.RainbowSpeed;
        //}

        public static APIServiceSettings CreateDfault()
        {
            APIServiceSettings settings = new APIServiceSettings
            {
                //FadeSpeed = DefaultValues.FadeSpeed,
                //ScreenLedHeight = DefaultValues.ScreenHeight,
                //ScreenLedWidth = DefaultValues.ScreenWidth,
                //ScreenPixelOffset = DefaultValues.ScreenOffset,
                //VideoCaptureDevice = DefaultValues.VideoCaptureDevicePath,
                //LedFlowDirection = DefaultValues.LedFlowDirection,
                //RainbowSpeed = DefaultValues.RainbowSpeed,
                //ActiveRainbow = DefaultValues.ActiveRainbow

            };
            return settings;
        }
    }
}
