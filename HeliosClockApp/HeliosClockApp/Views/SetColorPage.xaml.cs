using HeliosClockApp.Models;
using HeliosClockApp.ViewModels;
using LuminCommon.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace HeliosClockApp.Views
{
    public partial class SetColorPage : ContentPage
    {
        public ColorSaveItem Item { get; set; }

        public SetColorPage()
        {
            InitializeComponent();
            BindingContext = new SetColorViewModel();

            ColorPicker.PickedColorChanged += ColorPicker_PickedColorChanged;
            // ((SetColorViewModel)BindingContext).SelectedColor = ((SetColorViewModel)BindingContext).HeliosService.

        }

        private void ColorPicker_PickedColorChanged(object sender, System.Drawing.Color e)
        {
            ((SetColorViewModel)BindingContext).SelectedColor = e;
        }
    }
}