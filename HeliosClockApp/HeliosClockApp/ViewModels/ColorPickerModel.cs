using HeliosClockApp.Models;
using System;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace HeliosClockApp.ViewModels
{
    public class ColorPickerModel : BaseViewModel
    {
        public ColorPickerModel()
        {
            Title = "Set Color";
            SetColorFromPickerCommand = new Command<System.Drawing.Color>(async (color) =>
            {
                HeliosService.StartColor = color;
                HeliosService.EndColor = color;
                await HeliosService.SendColor().ConfigureAwait(false);
            });
        }

        public ICommand SetColorFromPickerCommand { get; }
    }
}