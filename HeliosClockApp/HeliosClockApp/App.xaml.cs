using HeliosClockApp.Services;
using HeliosClockApp.Views;
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
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
            Task.Run(async () => await HeliosService.ConnectToServer());
        }

    }
}
