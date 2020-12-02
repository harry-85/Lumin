using HeliosClockCommon.Models;
using HeliosClockCommon.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace HeliosClockCommon.Serializer
{
    public class SettingsSerializer
    {
        private readonly FileInfo settingsFile;

        /// <summary>Initializes a new instance of the <see cref="SettingsSerializer"/> class.</summary>
        /// <param name="settingsFile">The settings file.</param>
        public SettingsSerializer(FileInfo settingsFile)
        {
            this.settingsFile = settingsFile;
        }

        /// <summary>Serializes the application settings.</summary>
        /// <param name="settings">The settings.</param>
        public async Task SerilaizeAppSettings(HeliosSettings settings)
        {
            await Task.Run(() =>
            {
                try
                {
                    XmlDocument xmlDocument = new XmlDocument();
                    XmlSerializer serializer = new XmlSerializer(typeof(List<ColorSaveItem>)); //initializes the serializer

                    using (var writer = new StreamWriter(settingsFile.FullName, false))// FileMode.OpenOrCreate); //initializes the writer
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

        /// <summary>Dematerializes the application settings.</summary>
        public async Task<IHeliosSettings> DesirializeAppSettings()
        {
            return await Task.Run(() =>
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<ColorSaveItem>)); //initializes the serializer
                    using var reader = new StreamReader(settingsFile.FullName);
                    List<ColorSaveItem> items = (List<ColorSaveItem>)serializer.Deserialize(reader); //reads from the XML file and inserts it in this variable
                    reader.Close(); //closes the reader
                    
                    return new HeliosSettings { Items = items };
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
