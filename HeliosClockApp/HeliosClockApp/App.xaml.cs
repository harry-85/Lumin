using HeliosClockApp.Services;
using HeliosClockApp.Views;
using HeliosClockCommon.Messages;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace HeliosClockApp
{
    public partial class App : Application
    {
        public IHeliosAppService HeliosService { get; private set; }
        public App()
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            DependencyService.Register<HeliosAppService>();

            HeliosService = DependencyService.Get<IHeliosAppService>();

            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
            Task.Run(async () => await HeliosService.ConnectToServer());
            Xamarin.Forms.MessagingCenter.Send<ConnectedMessage>(new ConnectedMessage(), "ConnectedToServer");
        }

        protected override void OnSleep()
        {
            Xamarin.Forms.MessagingCenter.Send<ConnectedMessage>(new ConnectedMessage(), "DicsonnectedFromServer");
        }

        protected override void OnResume()
        {
            Task.Run(async () => await HeliosService.ConnectToServer());
            Xamarin.Forms.MessagingCenter.Send<ConnectedMessage>(new ConnectedMessage(), "ConnectedToServer");
        }
    }
}
