using HeliosClockApp.ViewModels;
using Xamarin.Forms;

namespace HeliosClockApp.Views
{
    public partial class ItemsPage : ContentPage
    {
        ItemsViewModel _viewModel;

        public ItemsPage()
        {
            InitializeComponent();

            BindingContext = _viewModel = new ItemsViewModel();


            ////CollectionView collectionView = new CollectionView();
            ////collectionView.SetBinding(ItemsView.ItemsSourceProperty, nameof(_viewModel.Items));
            
            ////collectionView.ItemTemplate = new DataTemplate(() =>
            ////{
            ////    StackLayout stackLayout = new StackLayout { Padding = 10 };
                
            ////    return stackLayout;
            ////});

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }

        private void ItemsListView_ChildAdded(object sender, ElementEventArgs e)
        {
           // var val = ((CollectionView)e.Element).Items;
        }
    }
}