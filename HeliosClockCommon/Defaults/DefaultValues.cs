using System;
using System.Collections.Generic;
using System.Text;

namespace HeliosClockCommon.Defaults
{
    public class DefaultValues
    {
        public static string BaseUrl => "/hubs/helios";

        public static int SignalPortOne = 5000;
        public static int SignalPortTwo = 5001;
        public static string HubUrl => "http://{0}:{1}/hubs/helios";
    }
}
