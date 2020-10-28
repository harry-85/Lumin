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
        private IHeliosService heliosService;
        public App()
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            DependencyService.Register<HeliosService>();

            heliosService = DependencyService.Get<IHeliosService>();

            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
            Task.Run(async () => await heliosService.ConnectToServer());
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
            Task.Run(async () => await heliosService.ConnectToServer());
        }

    }
}
