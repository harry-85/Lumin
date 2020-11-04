using HeliosClockCommon.Helper;
using System;
using System.Drawing;
using System.Xml.Serialization;

namespace HeliosClockCommon.Models
{
    /// <summary>Color item to be saved by UI.</summary>
    [Serializable]
    public class ColorSaveItem
    {
        string startColor;
        string endColor;

        public string Id { get; set; }
        public string Name { get; set; }
        
        [XmlIgnore]
        public Color StartColor { get => ColorHelpers.FromHex(startColor); set => startColor = ColorHelpers.HexConverter(value); }

        public string StartColorString { get => startColor; set => startColor = value; }


        [XmlIgnore]
        public Color EndColor { get => ColorHelpers.FromHex(endColor); set => endColor = ColorHelpers.HexConverter(value); }

        public string EndColorString { get => endColor; set => endColor = value; }
    }
}