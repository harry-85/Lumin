using HeliosClockApp.Models;
using HeliosClockApp.ViewModels;
using System;
using Xamarin.Forms;

namespace HeliosClockApp.Views
{
    public partial class ColorControlPage : ContentPage
    {
        /// <summary>Initializes a new instance of the <see cref="ColorControlPage"/> class.</summary>
        public ColorControlPage()
        {
            InitializeComponent();
            var viewModel = (ColorControlModel)BindingContext;

            ((ColorControlModel)BindingContext).HeliosService.OnEndColorChanged += HeliosService_OnEndColorChanged;
            ((ColorControlModel)BindingContext).HeliosService.OnStartColorChanged += HeliosService_OnStartColorChanged;
            ((ColorControlModel)BindingContext).HeliosService.OnConnected += HeliosService_OnConnected;
            ((ColorControlModel)BindingContext).HeliosService.OnModeChange += HeliosService_OnModeChange;
        }

        /// <summary>Handles the OnModeChange event of the HeliosService control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs{HeliosClockCommon.Enumerations.LedMode}"/> instance containing the event data.</param>
        private void HeliosService_OnModeChange(object sender, EventArgs<HeliosClockCommon.Enumerations.LedMode> e)
        {
            switch (e.Args)
            {
                case HeliosClockCommon.Enumerations.LedMode.None:
                    ButtonSpin.BackgroundColor = (Color)Application.Current.Resources["Primary"];
                    ButtonDiscoMode.BackgroundColor = (Color)Application.Current.Resources["Primary"];
                    ButtonKnightRider.BackgroundColor = (Color)Application.Current.Resources["Primary"];
                    break;
                case HeliosClockCommon.Enumerations.LedMode.Spin:
                    ButtonSpin.BackgroundColor = Color.ForestGreen;
                    ButtonDiscoMode.BackgroundColor = (Color)Application.Current.Resources["Primary"];
                    ButtonKnightRider.BackgroundColor = (Color)Application.Current.Resources["Primary"];
                    break;
                case HeliosClockCommon.Enumerations.LedMode.KnightRider:
                    ButtonSpin.BackgroundColor = (Color)Application.Current.Resources["Primary"];
                    ButtonDiscoMode.BackgroundColor = (Color)Application.Current.Resources["Primary"];
                    ButtonKnightRider.BackgroundColor = Color.ForestGreen;
                    break;
                case HeliosClockCommon.Enumerations.LedMode.Disco:
                    ButtonSpin.BackgroundColor = (Color)Application.Current.Resources["Primary"];
                    ButtonKnightRider.BackgroundColor = (Color)Application.Current.Resources["Primary"];
                    ButtonDiscoMode.BackgroundColor = Color.ForestGreen;
                    break;
            }
        }

        /// <summary>Handles the OnConnected event of the HeliosService control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs{System.Boolean}"/> instance containing the event data.</param>
        private void HeliosService_OnConnected(object sender, EventArgs<bool> e)
        {
            var viewModel = (ColorControlModel)BindingContext;

            //Set default values only when app was closed (shutdown) and reopened 
            if (e.Args)
            {
                viewModel.SetRefreshRateCommand.Execute(sliderSpeed.Value);
                viewModel.BlackCommand.Execute(null);
            }
        }

        /// <summary>Handles the OnStartColorChanged event of the HeliosService control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs{System.Drawing.Color}"/> instance containing the event data.</param>
        private void HeliosService_OnStartColorChanged(object sender, EventArgs<System.Drawing.Color> e)
        {
            StartColor = e.Args;
            gradientTouch.StartColor = StartColor;
        }

        /// <summary>Handles the OnEndColorChanged event of the HeliosService control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs{System.Drawing.Color}"/> instance containing the event data.</param>
        private void HeliosService_OnEndColorChanged(object sender, EventArgs<System.Drawing.Color> e)
        {
            EndColor = e.Args;
            gradientTouch.EndColor = EndColor;
        }

        /// <summary>Gets or sets the start color.</summary>
        /// <value>The start color.</value>
        public Color StartColor
        {
            get { return (Color)GetValue(StartColorProperty); }
            set { SetValue(StartColorProperty, value); }
        }

        /// <summary>The start color property</summary>
        public static readonly BindableProperty StartColorProperty = BindableProperty.Create(nameof(StartColor), typeof(Color), typeof(Color), null);

        /// <summary>Gets or sets the end color.</summary>
        /// <value>The end color.</value>
        public Color EndColor
        {
            get { return (Color)GetValue(EndProperty); }
            set { SetValue(EndProperty, value); }
        }

        public static readonly BindableProperty EndProperty = BindableProperty.Create(nameof(EndColor), typeof(Color), typeof(Color), null);

        /// <summary>Handles the Clicked event of the ButtonRandomColor control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonRandomColor_Clicked(object sender, EventArgs e)
        {
            Random rnd = new Random();
            StartColor = System.Drawing.Color.FromArgb(255, rnd.Next(256), rnd.Next(256), rnd.Next(256));
            
            rnd = new Random();
            EndColor = System.Drawing.Color.FromArgb(255, rnd.Next(256), rnd.Next(256), rnd.Next(256));

            var viewModel = (ColorControlModel)BindingContext;
            viewModel.SetRandomColorCommand.Execute(new ColorModel { StartColor = StartColor , EndColor = EndColor});
        }

        /// <summary>Handles the ValueChanged event of the Slider control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ValueChangedEventArgs"/> instance containing the event data.</param>
        private void Slider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            labelSpeed.Text = ((int)e.NewValue).ToString();
            var viewModel = (ColorControlModel)BindingContext;
            viewModel.SetRefreshRateCommand.Execute(e.NewValue);
        }

        /// <summary>Handles the 1 event of the Slider_ValueChanged control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ValueChangedEventArgs"/> instance containing the event data.</param>
        private void Slider_ValueChanged_1(object sender, ValueChangedEventArgs e)
        {
            var viewModel = (ColorControlModel)BindingContext;
            viewModel.SetBrightnessCommand.Execute((int)e.NewValue);
        }
    }
}