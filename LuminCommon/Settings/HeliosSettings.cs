using LuminCommon.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace LuminCommon.Settings
{

    [XmlRoot("HeliosSettings", IsNullable = false)]
    public class HeliosSettings : IHeliosSettings
    {
        [XmlAttribute("ClockItems")]
        public List<ColorSaveItem> Items { get; set; }

        public HeliosSettings() 
        {
            Items = new List<ColorSaveItem>();
        }
    }
}
