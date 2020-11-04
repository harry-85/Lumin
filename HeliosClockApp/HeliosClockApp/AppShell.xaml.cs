using HeliosClockApp.Views;
using HeliosClockCommon.Messages;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace HeliosClockApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(ItemDetailPage), typeof(ItemDetailPage));
            Routing.RegisterRoute(nameof(GradientColorPage), typeof(GradientColorPage));
            Routing.RegisterRoute(nameof(SetColorPage), typeof(SetColorPage));

            MessagingCenter.Subscribe<NavigateHomeMessage>(new NavigateHomeMessage(), "NavigateHomeMessage", (message) => { TabBar.CurrentItem = TabPageHome; });
        }
    }
}
