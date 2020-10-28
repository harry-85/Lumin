using HeliosClockApp.ViewModels;
using HeliosClockApp.Views;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace HeliosClockApp
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(ItemDetailPage), typeof(ItemDetailPage));
            Routing.RegisterRoute(nameof(GradientColorPage), typeof(GradientColorPage));
            Routing.RegisterRoute(nameof(SetColorPage), typeof(SetColorPage));
        }
    }
}
