using HeliosClockApp.Models;
using HeliosClockApp.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace HeliosClockApp.Views
{
    public partial class GradientColorPage : ContentPage
    {
        public Item Item { get; set; }

        private GradientColorViewModel GradientColorViewModel;

        public GradientColorPage()
        {
            InitializeComponent();
            BindingContext = new GradientColorViewModel();
            GradientColorViewModel = (GradientColorViewModel)BindingContext;

            ((GradientColorViewModel)BindingContext).PropertyChanged += GradientColorPage_PropertyChanged;
        }

        private void GradientColorPage_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GradientColorViewModel.StartColor))
            {
                GradientStart.Color = GradientColorViewModel.StartColor;
            }

            if (e.PropertyName == nameof(GradientColorViewModel.EndColor))
            {
                GradientStop.Color = GradientColorViewModel.EndColor;
            }
        }
    }
}