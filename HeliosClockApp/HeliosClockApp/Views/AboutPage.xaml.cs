using HeliosClockApp.Models;
using HeliosClockApp.ViewModels;
using System;
using Xamarin.Forms;

namespace HeliosClockApp.Views
{
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();
            var viewModel = (AboutViewModel)BindingContext;

            ////viewModel.BlackCommand.Execute(null);
            ////StartColor = Color.Black;
            ////EndColor = Color.Black;

            ((AboutViewModel)BindingContext).HeliosService.OnEndColorChanged += HeliosService_OnEndColorChanged;
            ((AboutViewModel)BindingContext).HeliosService.OnStartColorChanged += HeliosService_OnStartColorChanged;
            ((AboutViewModel)BindingContext).HeliosService.OnConnected += HeliosService_OnConnected;
        }

        private void HeliosService_OnConnected(object sender, EventArgs e)
        {
            var viewModel = (AboutViewModel)BindingContext;

            if (((AboutViewModel)BindingContext).HeliosService.IsStartup)
            {
                viewModel.SetRefreshRateCommand.Execute(sliderSpeed.Value);
                viewModel.BlackCommand.Execute(null);
            }
        }

        private void HeliosService_OnStartColorChanged(object sender, Systems.EventArgs<System.Drawing.Color> e)
        {
            StartColor = e.Args;
            GradientStart.Color = StartColor;
        }

        private void HeliosService_OnEndColorChanged(object sender, Systems.EventArgs<System.Drawing.Color> e)
        {
            EndColor = e.Args;
            GradientStop.Color = EndColor;
        }

        public Color StartColor
        {
            get { return (Color)GetValue(StartColorProperty); }
            set { SetValue(StartColorProperty, value); }
        }

        public static readonly BindableProperty StartColorProperty = BindableProperty.Create(nameof(StartColor), typeof(Color), typeof(Color), null);

        public Color EndColor
        {
            get { return (Color)GetValue(EndProperty); }
            set { SetValue(EndProperty, value); }
        }

        public static readonly BindableProperty EndProperty = BindableProperty.Create(nameof(EndColor), typeof(Color), typeof(Color), null);

        private void ButtonRandomColor_Clicked(object sender, EventArgs e)
        {
            Random rnd = new Random();
            StartColor = System.Drawing.Color.FromArgb(255, rnd.Next(256), rnd.Next(256), rnd.Next(256));
            EndColor = System.Drawing.Color.FromArgb(255, rnd.Next(256), rnd.Next(256), rnd.Next(256));

            var viewModel = (AboutViewModel)BindingContext;
            viewModel.SetRandomolorCommand.Execute(new ColorModel { StartColor = StartColor , EndColor = EndColor});
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            var viewModel = (AboutViewModel)BindingContext;
            viewModel.AddGradientCommand.Execute(null);
        }

        private void Slider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            labelSpeed.Text = ((int)e.NewValue).ToString();
            var viewModel = (AboutViewModel)BindingContext;
            viewModel.SetRefreshRateCommand.Execute(e.NewValue);

        }
    }
}