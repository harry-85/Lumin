using HeliosClockApp.Models;
using HeliosClockCommon.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace HeliosClockApp.ViewModels
{
    public class SetColorViewModel : BaseViewModel
    {
        private Color selectedColor;

        public Color SelectedColor
        {
            get => selectedColor;
            set => SetProperty(ref selectedColor, value);
        }
        public SetColorViewModel()
        {
            SetColorCommand = new Command(OnSave, ValidateSetColor);
            CancelCommand = new Command(OnCancel);
            this.PropertyChanged += (_, __) => SetColorCommand.ChangeCanExecute();
        }

        private bool ValidateSetColor()
        {
            return SelectedColor != null;
        }

        public Command SetColorCommand { get; }
        public Command CancelCommand { get; }

        private async void OnCancel()
        {
            // This will pop the current page off the navigation stack
            await Shell.Current.GoToAsync("..");
        }

        private async void OnSave()
        {
            ////Item newItem = new Item()
            ////{
            ////    Id = Guid.NewGuid().ToString(),
            ////    Text = Text,
            ////    Description = Description
            ////};

            ////await DataStore.AddItemAsync(newItem);

            // This will pop the current page off the navigation stack
            MessagingCenter.Send<SetColorFromGradientMessage, Color>(new SetColorFromGradientMessage(), "SetColor", SelectedColor);
            await Shell.Current.GoToAsync("..");
        }
    }
}
