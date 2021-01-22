using HeliosClockApp.Models;
using LuminCommon.Helper;
using LuminCommon.Models;
using LuminCommon.Serializer;
using LuminCommon.Settings;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HeliosClockApp.Services
{
    public class MockDataStore : IDataStore<ColorSaveItem>
    {
        private List<ColorSaveItem> items;
        private SettingsSerializer serializer;
        public MockDataStore()
        {
            items = new List<ColorSaveItem>();
            //Environment.SpecialFolder.LocalApplication
            FileInfo fileName = new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "save.xml"));
            serializer = new SettingsSerializer(fileName);

            Task.Run(async () =>
            {
                var newItems  = (await serializer.DesirializeAppSettings().ConfigureAwait(false)).Items;
              //  items.Add(new ColorSaveItem { Id = Guid.NewGuid().ToString(), Name = "First item", StartColor = ColorHelpers.HexConverter(Color.Blue), EndColor = Color.Purple });
              //  items.Add(new ColorSaveItem { Id = Guid.NewGuid().ToString(), Name = "Second item", StartColor=Color.Red, EndColor=Color.Green });
                items.AddRange(newItems);
            });
        }

        public async Task<bool> AddItemAsync(ColorSaveItem item)
        {
            items.Add(item);

            await serializer.SerilaizeAppSettings(new HeliosSettings { Items = items }).ConfigureAwait(false);

            return await Task.FromResult(true);
        }

        public async Task<bool> UpdateItemAsync(ColorSaveItem item)
        {
            var oldItem = items.Where((ColorSaveItem arg) => arg.Id == item.Id).FirstOrDefault();
            items.Remove(oldItem);
            items.Add(item);

            await serializer.SerilaizeAppSettings(new HeliosSettings { Items = items }).ConfigureAwait(false);

            return await Task.FromResult(true);
        }

        public async Task<bool> DeleteItemAsync(string id)
        {
            var oldItem = items.Where((ColorSaveItem arg) => arg.Id == id).FirstOrDefault();
            items.Remove(oldItem);

            return await Task.FromResult(true);
        }

        public async Task<ColorSaveItem> GetItemAsync(string id)
        {
            return await Task.FromResult(items.FirstOrDefault(s => s.Id == id));
        }

        public async Task<IEnumerable<ColorSaveItem>> GetItemsAsync(bool forceRefresh = false)
        {
            return await Task.FromResult(items);
        }
    }
}