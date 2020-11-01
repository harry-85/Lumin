using HeliosClockApp.Services;
using HeliosClockApp.ViewModels;
using HeliosClockApp.Views;
using HeliosClockCommon.Helper;
using HeliosClockCommon.Messages;
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

        private async void TapGestureRecognizer_Start_Tapped(object sender, EventArgs e)
        {
            await StartColorPicker(true);
        }

        private async void TapGestureRecognizer_End_Tapped(object sender, EventArgs e)
        {
            await StartColorPicker(false);
        }

        private async Task StartColorPicker(bool isStartColor)
        {
            //Unsubscribe from all messages
            Close();

            MessagingCenter.Subscribe<SetColorFromGradientMessage, Color>(this, "SetColor", async (sender, color) =>
            {
                if (isStartColor)
                    StartColor = color;
                else
                    EndColor = color;

                await SendColor().ConfigureAwait(false);
            });

            //This will push the SetColorPage on the stack
            await Shell.Current.GoToAsync(nameof(SetColorPage));
        }

        private void Close()
        {
            MessagingCenter.Unsubscribe<SetColorFromGradientMessage, Color>(this, "SetColor");
        }

        private void ChangeColor(SwipedEventArgs e, bool isStartColor)
        {
            Task.Run(async () =>
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
                        if (isStartColor)
                            StartColor = Color.FromHsla(CheckHSLValue(color.Hue - delta), color.Saturation, color.Luminosity);
                        else
                            EndColor = Color.FromHsla(CheckHSLValue(color.Hue - delta), color.Saturation, color.Luminosity);
                        break;
                    case SwipeDirection.Up:
                        if (isStartColor)
                            StartColor = Color.FromHsla(CheckHSLValue(color.Hue + delta), color.Saturation, color.Luminosity);
                        else
                            EndColor = Color.FromHsla(CheckHSLValue(color.Hue + delta), color.Saturation, color.Luminosity);
                        break;
                }
                await SendColor().ConfigureAwait(false);
            });
        }

        private async Task SendColor()
        {
            HeliosService.StartColor = StartColor;
            HeliosService.EndColor = EndColor;
            await HeliosService.SendColor().ConfigureAwait(false);
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