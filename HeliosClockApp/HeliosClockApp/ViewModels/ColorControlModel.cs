using HeliosClockApp.Models;
using HeliosClockApp.Views;
using HeliosClockCommon.Enumerations;
using HeliosClockCommon.Models;
using System;
using System.Net;
using System.Windows.Input;
using Xamarin.Forms;

namespace HeliosClockApp.ViewModels
{
    public class ColorControlModel : BaseViewModel
    {
        /// <summary>Initializes a new instance of the <see cref="ColorControlModel"/> class.</summary>
        public ColorControlModel()
        {
            Title = "Lumin Control";
            SetRandomColorCommand = new Command<ColorModel>(async (colorModel) =>
            {
                HeliosService.StartColor = colorModel.StartColor;
                HeliosService.EndColor = colorModel.EndColor;
                await HeliosService.SendColor().ConfigureAwait(false);
            });

            BlackCommand = new Command(async () =>
            {
                HeliosService.StartColor = System.Drawing.Color.Black;
                HeliosService.EndColor = System.Drawing.Color.Black;
                await HeliosService.SetOnOff(PowerOnOff.Off, LedSide.Full).ConfigureAwait(false);
            });

            WhiteCommand = new Command(async () =>
            {
                HeliosService.StartColor = System.Drawing.Color.White;
                HeliosService.EndColor = System.Drawing.Color.White;
                await HeliosService.SetOnOff(PowerOnOff.On, LedSide.Full).ConfigureAwait(false);
            });

            StartSpinCommand = new Command(async () =>
            {
                await HeliosService.StartMode(LedMode.Spin).ConfigureAwait(false);
            });

            StartKnightRiderCommand = new Command(async () =>
            {
                await HeliosService.StartMode(LedMode.KnightRider).ConfigureAwait(false);
            });

            StopCommand = new Command(async () =>
            {
                await HeliosService.StopMode().ConfigureAwait(false);
            });

            SetRefreshRateCommand = new Command<double>(async (speed) =>
            {
                await HeliosService.SetRefreshSpeed((int)speed).ConfigureAwait(false);
            });

            SaveColorCommand = new Command(async () =>
            {
                string randomWord = "Saved Color";
                try
                {
                    WebClient client = new WebClient();
                    var downloadedString = client.DownloadString("https://www.wordgenerator.net/application/p.php?id=dictionary_words&type=1&spaceflag=false");
                    randomWord = downloadedString.Split(",")[0];
                }
                catch
                {
                }

                await DataStore.AddItemAsync(new ColorSaveItem { StartColor = HeliosService.StartColor, EndColor = HeliosService.EndColor, Name = randomWord, Id = Guid.NewGuid().ToString() }).ConfigureAwait(false);
            });
            SetBrightnessCommand = new Command<int>(async (brightness) =>
            {
                await HeliosService.SetBrightness(brightness).ConfigureAwait(false);
            });

            AddGradientCommand = new Command(OnAddGradient);
        }

        /// <summary>Gets the set random color command.</summary>
        /// <value>The set random color command.</value>
        public ICommand SetRandomColorCommand { get; }

        /// <summary>Gets the black command.</summary>
        /// <value>The black command.</value>
        public ICommand BlackCommand { get; }

        /// <summary>Gets the white command.</summary>
        /// <value>The white command.</value>
        public ICommand WhiteCommand { get; }

        /// <summary>Gets the start knight rider command.</summary>
        /// <value>The start knight rider command.</value>
        public ICommand StartKnightRiderCommand { get; }

        /// <summary>Gets the set refresh rate command.</summary>
        /// <value>The set refresh rate command.</value>
        public ICommand SetRefreshRateCommand { get; }

        /// <summary>Gets the add gradient command.</summary>
        /// <value>The add gradient command.</value>
        public ICommand AddGradientCommand { get; }

        public ICommand SaveColorCommand { get; }

        /// <summary>Gets the start spin command.</summary>
        /// <value>The start spin command.</value>
        public ICommand StartSpinCommand { get; }

        /// <summary>Gets the stop command.</summary>
        /// <value>The stop command.</value>
        public ICommand StopCommand { get; }
        public ICommand SetBrightnessCommand { get; }

        /// <summary>Called when [add gradient].</summary>
        /// <param name="obj">The object.</param>
        private async void OnAddGradient(object obj)
        {
            await Shell.Current.GoToAsync(nameof(GradientColorPage));
        }
    }
}