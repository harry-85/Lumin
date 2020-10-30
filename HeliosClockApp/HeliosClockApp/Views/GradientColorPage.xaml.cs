using HeliosClockApp.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace HeliosClockApp.Views
{
    public partial class GradientColorPage : ContentPage
    {
        /// <summary>The gradient color view model</summary>
        private GradientColorViewModel gradientColorViewModel;

        /// <summary>Initializes a new instance of the <see cref="GradientColorPage"/> class.</summary>
        public GradientColorPage()
        {
            InitializeComponent();
            BindingContext = new GradientColorViewModel();
            gradientColorViewModel = (GradientColorViewModel)BindingContext;

            ((GradientColorViewModel)BindingContext).PropertyChanged += GradientColorPage_PropertyChanged;

            GradientStart.Color = ((GradientColorViewModel)BindingContext).HeliosService.StartColor;
            GradientStop.Color = ((GradientColorViewModel)BindingContext).HeliosService.EndColor;
        }

        /// <summary>Handles the PropertyChanged event of the GradientColorPage control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void GradientColorPage_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(gradientColorViewModel.StartColor))
            {
                GradientStart.Color = gradientColorViewModel.StartColor;
            }

            if (e.PropertyName == nameof(gradientColorViewModel.EndColor))
            {
                GradientStop.Color = gradientColorViewModel.EndColor;
            }
        }
    }
}