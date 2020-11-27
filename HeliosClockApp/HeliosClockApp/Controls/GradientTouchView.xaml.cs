using HeliosClockApp.ViewModels;
using System;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace HeliosClockApp.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GradientTouchView : ContentView
    {
        private readonly GradientTouchViewModel model;

        /// <summary>Gets or sets the start color.</summary>
        /// <value>The start color.</value>
        public Color StartColor
        {
            get => model == null ? Color.Red : StartColor;
            set => model.StartColor = value;

        }
        /// <summary>Gets or sets the end color.</summary>
        /// <value>The end color.</value>
        public Color EndColor
        {
            get => model == null ? Color.Red : EndColor;
            set => model.EndColor = value;
        }

        /// <summary>Initializes a new instance of the <see cref="GradientTouchView"/> class.</summary>
        public GradientTouchView()
        {
            InitializeComponent();
            model = (GradientTouchViewModel)BindingContext;
            model.PropertyChanged += GradientTouchViewModel_PropertyChanged;
        }

        private void StartSwipeGesture_Swiped(object sender, SwipedEventArgs e)
        {
            model.ChangeColor(e, true);
        }

        private void EndSwipeGesture_Swiped(object sender, SwipedEventArgs e)
        {
            model.ChangeColor(e, false);
        }

        private async void TapGestureRecognizer_Start_Tapped(object sender, EventArgs e)
        {
            await model.StartColorPicker(true);
        }

        private async void TapGestureRecognizer_End_Tapped(object sender, EventArgs e)
        {
            await model.StartColorPicker(false);
        }

        /// <summary>Handles the PropertyChanged event of the GradientTouchView control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void GradientTouchViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(model.StartColor))
            {
                GradientStart.Color = model.StartColor;
            }
            if (e.PropertyName == nameof(model.EndColor))
            {
                GradientStop.Color = model.EndColor;
            }
        }

        private void WheelPicker_SelectedItemIndexChanged(object sender, Vapolia.WheelPickerCore.WheelChangedEventArgs e)
        {
        }
    }
}