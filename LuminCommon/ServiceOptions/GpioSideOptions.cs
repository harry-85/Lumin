using LuminCommon.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace LuminCommon.ServiceOptions
{
    public class GpioSideOptions
    {
        public const string GpioSide = "GpioServiceSide";

        public LedSide LedSide { get; set; }
    }
}
