using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using static System.Environment;

namespace HeliosClockCommon.Defaults
{
    public class DefaultValues
    {
        public static string UnixSavePath = string.Format("{0}/.luminConfig/lumin.config", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
        public static string BaseUrl => "/hubs/helios";

        public static int SignalPortOne = 5000;
        public static int SignalPortTwo = 5001;
        public static string HubUrl => "http://{0}:{1}/hubs/helios";

        public static string AllClients = "AllClient";

        public static string DefaultClientName = "Led Client";

        public static int DefaultLedCount = 58;

    }
}
