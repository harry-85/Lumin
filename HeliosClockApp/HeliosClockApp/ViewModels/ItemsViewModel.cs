using HeliosClockApp.Models;
using HeliosClockApp.Views;
using HeliosClockCommon.Messages;
using HeliosClockCommon.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace HeliosClockApp.ViewModels
{
    public class ItemsViewModel : BaseViewModel
    {
        /// <summary>The selected item</summary>
        private ColorSaveItem _selectedItem;

        /// <summary>Gets the items.</summary>
        /// <value>The items.</value>
        public ObservableCollection<ColorSaveItem> Items { get; }
        /// <summary>Gets the load items command.</summary>
        /// <value>The load items command.</value>
        public Command LoadItemsCommand { get; }
        /// <summary>Gets the add item command.</summary>
        /// <value>The add item command.</value>
        public Command AddItemCommand { get; }
        /// <summary>Gets the item tapped.</summary>
        /// <value>The item tapped.</value>
        public Command<ColorSaveItem> ItemTapped { get; }

        /// <summary>Initializes a new instance of the <see cref="ItemsViewModel"/> class.</summary>
        public ItemsViewModel()
        {
            Title = "Browse";
            Items = new ObservableCollection<ColorSaveItem>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());

            ItemTapped = new Command<ColorSaveItem>(OnItemSelected);

            AddItemCommand = new Command(OnAddItem);
        }

        async Task ExecuteLoadItemsCommand()
        {
            IsBusy = true;

            try
            {
                Items.Clear();
                var items = await DataStore.GetItemsAsync(true);
                foreach (var item in items)
                {
                    Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void OnAppearing()
        {
            IsBusy = true;
            SelectedItem = null;
        }

        public ColorSaveItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
                OnItemSelected(value);
            }
        }

        private async void OnAddItem(object obj)
        {
            await Shell.Current.GoToAsync(nameof(GradientColorPage));
        }

        async void OnItemSelected(ColorSaveItem item)
        {
            if (item == null)
                return;

            HeliosService.StartColor = item.StartColor;
            HeliosService.EndColor = item.EndColor;

            await HeliosService.SendColor().ConfigureAwait(false);

            MessagingCenter.Send<NavigateHomeMessage>(new NavigateHomeMessage(), "NavigateHomeMessage");

            // This will push the ItemDetailPage onto the navigation stack
            //await Shell.Current.GoToAsync($"{nameof(ItemDetailPage)}?{nameof(ItemDetailViewModel.ItemId)}={item.Id}");
        }
    }
}