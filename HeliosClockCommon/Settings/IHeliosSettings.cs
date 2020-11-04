using HeliosClockCommon.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace HeliosClockCommon.Settings
{
    public interface IHeliosSettings
    {
        public List<ColorSaveItem> Items { get; set; }
    }
}
