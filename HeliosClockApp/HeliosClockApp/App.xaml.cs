using HeliosClockApp.Services;
using HeliosClockApp.Views;
using HeliosClockCommon.Messages;
using HeliosClockCommon.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace HeliosClockApp
{
    public partial class App : Application
    {
        private CancellationTokenSource cancellationTokenSource;
        private CancellationToken cancellationToken;

        public IHeliosAppService HeliosService { get; private set; }
        public IDataStore<ColorSaveItem> dataStore { get; private set; }

        public App()
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            DependencyService.Register<HeliosAppService>();

            HeliosService = DependencyService.Get<IHeliosAppService>();
            dataStore = DependencyService.Get<MockDataStore>();

            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
            ResetTokens();
            base.OnStart();
            Task.Run(async () => await HeliosService.ConnectToServer(cancellationToken)).ConfigureAwait(false);
        }

        protected override void OnSleep()
        {
            StopTokens();
            base.OnSleep();
        }

        protected override void OnResume()
        {
            ResetTokens();
            base.OnResume();
            Task.Run(async () => await HeliosService.ConnectToServer(cancellationToken)).ConfigureAwait(false);
        }

        private void ResetTokens()
        {
            StopTokens();
            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;
        }

        private void StopTokens()
        {
            cancellationTokenSource?.Cancel();
        }
    }
}
