using HeliosClockApp.Models;
using HeliosClockApp.ViewModels;
using HeliosClockCommon.Messages;
using System;
using Xamarin.Forms;

namespace HeliosClockApp.Views
{
    public partial class AboutPage : ContentPage
    {
        /// <summary>Initializes a new instance of the <see cref="AboutPage"/> class.</summary>
        public AboutPage()
        {
            InitializeComponent();
            var viewModel = (AboutViewModel)BindingContext;

            ((AboutViewModel)BindingContext).HeliosService.OnEndColorChanged += HeliosService_OnEndColorChanged;
            ((AboutViewModel)BindingContext).HeliosService.OnStartColorChanged += HeliosService_OnStartColorChanged;
            ((AboutViewModel)BindingContext).HeliosService.OnConnected += HeliosService_OnConnected;
            ((AboutViewModel)BindingContext).HeliosService.OnModeChange += HeliosService_OnModeChange;

            MessagingCenter.Subscribe<ConnectedMessage>(new ConnectedMessage(), "ConnectedToServer", (s) =>
            {
                connectingIndicator.IsVisible = false;
                connectingOverlay.IsVisible = false;
            });
            MessagingCenter.Subscribe<ConnectedMessage>(new ConnectedMessage(), "DicsonnectedFromServer", (s) =>
            {
                connectingIndicator.IsVisible = true;
                connectingOverlay.IsVisible = true;
            });
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
                    ButtonKnightRider.BackgroundColor = (Color)Application.Current.Resources["Primary"];
                    break;
                case HeliosClockCommon.Enumerations.LedMode.Spin:
                    ButtonSpin.BackgroundColor = Color.ForestGreen;
                    ButtonKnightRider.BackgroundColor = (Color)Application.Current.Resources["Primary"];
                    break;
                case HeliosClockCommon.Enumerations.LedMode.KnightRider:
                    ButtonSpin.BackgroundColor = (Color)Application.Current.Resources["Primary"];
                    ButtonKnightRider.BackgroundColor = Color.ForestGreen;
                    break;
            }
        }

        /// <summary>Handles the OnConnected event of the HeliosService control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs{System.Boolean}"/> instance containing the event data.</param>
        private void HeliosService_OnConnected(object sender, EventArgs<bool> e)
        {
            var viewModel = (AboutViewModel)BindingContext;

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
            EndColor = System.Drawing.Color.FromArgb(255, rnd.Next(256), rnd.Next(256), rnd.Next(256));

            var viewModel = (AboutViewModel)BindingContext;
            viewModel.SetRandomolorCommand.Execute(new ColorModel { StartColor = StartColor , EndColor = EndColor});
        }

        /// <summary>Handles the ValueChanged event of the Slider control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ValueChangedEventArgs"/> instance containing the event data.</param>
        private void Slider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            labelSpeed.Text = ((int)e.NewValue).ToString();
            var viewModel = (AboutViewModel)BindingContext;
            viewModel.SetRefreshRateCommand.Execute(e.NewValue);

        }
    }
}