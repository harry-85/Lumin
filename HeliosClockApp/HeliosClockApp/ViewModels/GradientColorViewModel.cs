using HeliosClockApp.Models;
using HeliosClockApp.Views;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace HeliosClockApp.ViewModels
{
    public class GradientColorViewModel : BaseViewModel
    {
        private Color startColor;
        private Color endColor;

        public GradientColorViewModel()
        {
            SaveCommand = new Command(OnSave, ValidateSave);
            CancelCommand = new Command(OnCancel);
            SetStartColorCommand = new Command(OnSetStartColor);
            SetEndColorCommand = new Command(OnSetEndColor);

            this.PropertyChanged += (_, __) => SaveCommand.ChangeCanExecute();
        }

        private bool ValidateSave()
        {
            return startColor != null && endColor != null;
        }

        public Color StartColor
        {
            get => startColor;
            set => SetProperty(ref startColor, value);
        }

        public Color EndColor
        {
            get => endColor;
            set => SetProperty(ref endColor, value);
        }

        public Command SaveCommand { get; }

        public Command CancelCommand { get; }

        public Command SetStartColorCommand { get; }

        public Command SetEndColorCommand { get; }

        private async void OnCancel()
        {
            //Unsubscribe from all messages
            Close();

            // This will pop the current page off the navigation stack
            await Shell.Current.GoToAsync("..");
        }

        private async void OnSave()
        {
            //Unsubscribe from all messages
            Close();

            HeliosService.StartColor = StartColor;
            HeliosService.EndColor = EndColor;

            await HeliosService.SendColor().ConfigureAwait(false);

            // This will pop the current page off the navigation stack
            await Shell.Current.GoToAsync("..");
        }

        private async void OnSetStartColor()
        {
            //Unsubscribe from all messages
            Close();

            MessagingCenter.Subscribe<SetColorViewModel, Color>(this, "SetColor", (sender, color) =>
            {
                StartColor = color;
            });

            //This will push the SetColorPage on the stack
            await Shell.Current.GoToAsync(nameof(SetColorPage));
        }

        private async void OnSetEndColor()
        {
            //Unsubscribe from all messages
            Close();

            MessagingCenter.Subscribe<SetColorViewModel, Color>(this, "SetColor", (sender, color) =>
            {
                EndColor = color;
            });

            //This will push the SetColorPage on the stack
            await Shell.Current.GoToAsync(nameof(SetColorPage));
        }

        private void Close()
        {
            MessagingCenter.Unsubscribe<SetColorViewModel, Color>(this, "SetColor");
            MessagingCenter.Unsubscribe<SetColorViewModel, Color>(this, "SetColor");
        }
    }
}
