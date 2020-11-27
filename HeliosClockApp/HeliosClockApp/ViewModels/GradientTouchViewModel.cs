using HeliosClockApp.Views;
using HeliosClockCommon.Helper;
using HeliosClockCommon.Messages;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace HeliosClockApp.ViewModels
{
    public class GradientTouchViewModel: BaseViewModel, INotifyPropertyChanged
    {
        //Referenced from XAML Code
        public ObservableCollection<string> ColorValues { get; } = new ObservableCollection<string>(Enumerable.Range(0, (int)HSLColor.Scale).Select(s => s.ToString()));
        public Command<(int, int, IList<int>)> ItemSelectedCommand { get; }

        private Color startColor;
        private Color endColor;

        /// <summary>Gets or sets the start color.</summary>
        /// <value>The start color.</value>
        public Color StartColor
        {
            get => startColor; set
            {
                startColor = value;
                OnPropertyChanged();
            }
        }
        /// <summary>Gets or sets the end color.</summary>
        /// <value>The end color.</value>
        public Color EndColor
        {
            get => endColor; set
            {
                endColor = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Initializes a new instance of the <see cref="GradientTouchViewModel"/> class.</summary>
        public GradientTouchViewModel()
        {
            StartColor = Color.Black;
            EndColor = Color.DarkGreen;

            ItemSelectedCommand = new Command<(int Component, int Row, IList<int> ItemIndexes)>(tuple =>
            {
                var (selectedWheelIndex, selectedItemIndex, selectedItemsIndexes) = tuple;
                EndColor = (System.Drawing.Color)(new HSLColor(selectedItemIndex, HSLColor.Scale, HSLColor.Scale / 2.0));
                //After the selection has changed, update the SelectedDate string
            });
        }

        public void ChangeColor(SwipedEventArgs e, bool isStartColor)
        {
            Task.Run(async () =>
            {
                Color color;
                if (isStartColor)
                    color = HeliosService.StartColor;
                else
                    color = HeliosService.EndColor;

                if (ColorHelpers.CompareColor(color, Color.Black) || ColorHelpers.CompareColor(color, Color.White))
                {
                    color = Color.FromHsla(color.Hue, 1, 0.5);
                }

                double delta = 0.05;

                switch (e.Direction)
                {
                    case SwipeDirection.Down:
                        if (isStartColor)
                            StartColor = Color.FromHsla(CheckHSLValue(color.Hue - delta), color.Saturation, color.Luminosity);
                        else
                            EndColor = Color.FromHsla(CheckHSLValue(color.Hue - delta), color.Saturation, color.Luminosity);
                        break;
                    case SwipeDirection.Up:
                        if (isStartColor)
                            StartColor = Color.FromHsla(CheckHSLValue(color.Hue + delta), color.Saturation, color.Luminosity);
                        else
                            EndColor = Color.FromHsla(CheckHSLValue(color.Hue + delta), color.Saturation, color.Luminosity);
                        break;
                }
                await SendColor().ConfigureAwait(false);
            });
        }

        public async Task StartColorPicker(bool isStartColor)
        {
            //Unsubscribe from all messages
            Close();

            MessagingCenter.Subscribe<SetColorFromGradientMessage, Color>(this, "SetColor", async (sender, color) =>
            {
                if (isStartColor)
                    StartColor = color;
                else
                    EndColor = color;

                await SendColor().ConfigureAwait(false);
            });

            //This will push the SetColorPage on the stack
            await Shell.Current.GoToAsync(nameof(SetColorPage));
        }

        private void Close()
        {
            MessagingCenter.Unsubscribe<SetColorFromGradientMessage, Color>(this, "SetColor");
        }

        /// <summary>Sends the color.</summary>
        private async Task SendColor()
        {
            HeliosService.StartColor = StartColor;
            HeliosService.EndColor = EndColor;
            await HeliosService.SendColor().ConfigureAwait(false);
        }

        private double CheckHSLValue(double input)
        {
            if (input < 0.0)
                input = 1.0 + input;

            if (input > 1.0)
                input = input - 1.0;
            return input;
        }
    }
}
