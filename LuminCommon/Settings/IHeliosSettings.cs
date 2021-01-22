using LuminCommon.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LuminCommon.Settings
{
    public interface IHeliosSettings
    {
        public List<ColorSaveItem> Items { get; set; }
    }
}
