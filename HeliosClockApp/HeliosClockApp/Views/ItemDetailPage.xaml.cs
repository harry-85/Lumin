using HeliosClockApp.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace HeliosClockApp.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}