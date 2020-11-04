using HeliosClockCommon.Models;
using HeliosClockCommon.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace HeliosClockCommon.Serializer
{
    public class SettingsSerializer
    {
        private FileInfo settingsFile;

        /// <summary>Initializes a new instance of the <see cref="SettingsSerializer"/> class.</summary>
        /// <param name="settingsFile">The settings file.</param>
        public SettingsSerializer(FileInfo settingsFile)
        {
            this.settingsFile = settingsFile;
        }

        /// <summary>Serilaizes the application settings.</summary>
        /// <param name="settings">The settings.</param>
        public async Task SerilaizeAppSettings(HeliosSettings settings)
        {
            await Task.Run(() =>
            {
                try
                {
                    XmlDocument xmlDocument = new XmlDocument();
                    XmlSerializer serializer = new XmlSerializer(typeof(List<ColorSaveItem>)); //initialises the serialiser

                    using (var writer = new StreamWriter(settingsFile.FullName, false))// FileMode.OpenOrCreate); //initialises the writer
                    {
                        serializer.Serialize(writer, settings.Items); //Writes to the file
                    }
                }
                catch (Exception ex)
                {
                    string a = ex.Message;
                }
            }).ConfigureAwait(false);
        }

        /// <summary>Desirializes the application settings.</summary>
        /// <returns></returns>
        public async Task<IHeliosSettings> DesirializeAppSettings()
        {
        //    if (!settingsFile.Exists)
             //   return new HeliosSettings { Items = new List<ColorSaveItem>() };

            return await Task.Run(() =>
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<ColorSaveItem>)); //initialises the serialiser
                    using (var reader = new StreamReader(settingsFile.FullName)) //, FileMode.Open); //Initialises the reader
                    {
                        List<ColorSaveItem> items = (List<ColorSaveItem>)serializer.Deserialize(reader); //reads from the xml file and inserts it in this variable
                        reader.Close(); //closes the reader
                        return new HeliosSettings { Items = items };
                    }
                }
                catch (Exception ex)
                {
                    string a = ex.Message;
                    return new HeliosSettings { Items = new List<ColorSaveItem>() };
                }
            }).ConfigureAwait(false);
        }
    }
}
