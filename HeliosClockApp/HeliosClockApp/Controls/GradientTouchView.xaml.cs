using HeliosClockApp.Services;
using HeliosClockCommon.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace HeliosClockApp.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GradientTouchView : ContentView, INotifyPropertyChanged
    {
        public event EventHandler<EventArgs> OnFrameTouchEvent;
        private Color startColor;
        private Color endColor;
        public IHeliosAppService HeliosService = DependencyService.Get<IHeliosAppService>();

        /// <summary>Gets or sets the start color.</summary>
        /// <value>The start color.</value>
        public Color StartColor
        {
            get => startColor; set
            {
                startColor = value;
                OnPropertyChanged();
            }
        }
        /// <summary>Gets or sets the end color.</summary>
        /// <value>The end color.</value>
        public Color EndColor
        {
            get => endColor; set
            {
                endColor = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Initializes a new instance of the <see cref="GradientTouchView"/> class.</summary>
        public GradientTouchView()
        {
            InitializeComponent();
            BindingContext = this;
            StartColor = Color.Black;
            EndColor = Color.DarkGreen;
            PropertyChanged += GradientTouchView_PropertyChanged;
        }

        /// <summary>Handles the PropertyChanged event of the GradientTouchView control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void GradientTouchView_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(StartColor))
            {
                GradientStart.Color = StartColor;
            }
            if (e.PropertyName == nameof(EndColor))
            {
                GradientStop.Color = EndColor;
            }
        }

        private void StartSwipeGesture_Swiped(object sender, SwipedEventArgs e)
        {
            ChangeColor(e, true);
        }

        private void EndSwipeGesture_Swiped(object sender, SwipedEventArgs e)
        {
            ChangeColor(e, false);
        }

        private void frameTouchedEvent_Tapped(object sender, EventArgs e)
        {
            OnFrameTouchEvent?.Invoke(sender, e);
        }


        private void ChangeColor(SwipedEventArgs e, bool isStartColor)
        {
            Task.Run(() =>
            {
                Color color;
                if (isStartColor)
                    color = HeliosService.StartColor;
                else
                    color = HeliosService.EndColor;

                if (ColorHelpers.CompareColor(color, Color.Black) || ColorHelpers.CompareColor(color, Color.White))
                {
                    color = Color.FromHsla(color.Hue, 1, 0.5);
                }

                double delta = 0.05;

                switch (e.Direction)
                {
                    case SwipeDirection.Down:
                        if(isStartColor)
                            HeliosService.StartColor = Color.FromHsla(CheckHSLValue(color.Hue - delta), color.Saturation, color.Luminosity);
                        else
                            HeliosService.EndColor = Color.FromHsla(CheckHSLValue(color.Hue - delta), color.Saturation, color.Luminosity);
                        break;
                    case SwipeDirection.Up:
                        if (isStartColor)
                            HeliosService.StartColor = Color.FromHsla(CheckHSLValue(color.Hue + delta), color.Saturation, color.Luminosity);
                        else
                            HeliosService.EndColor = Color.FromHsla(CheckHSLValue(color.Hue + delta), color.Saturation, color.Luminosity);
                        break;
                }
            });
        }

        private double CheckHSLValue(double input)
        {
            if (input < 0.0)
                input = 1.0 + input;
                 
            if (input > 1.0)
                input = input - 1.0;
            return input;
        }
    }
}