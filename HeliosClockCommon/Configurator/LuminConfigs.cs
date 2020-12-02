using System;
using System.Collections.Generic;
using System.Text;

namespace HeliosClockCommon.Configurator
{
    public class LuminConfigs
    {
        public string Name { get; set; }

        public static LuminConfigs GetDefaultConfig() => new LuminConfigs { Name = "Lumin Client" };
    }
}
