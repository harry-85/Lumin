using HeliosClockCommon.Defaults;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace HeliosClockCommon.Configurator
{
    public class ConfigureService : IConfigureService
    {
        public LuminConfigs Config { get; set; } = LuminConfigs.GetDefaultConfig();
        private readonly FileInfo configFile;
        private readonly ILogger<ConfigureService> logger;

        /// <summary>Initializes a new instance of the <see cref="ConfigureService"/> class.</summary>
        public ConfigureService(ILogger<ConfigureService> logger)
        {
            this.logger = logger;

            logger.LogInformation("Loading Configuration File from: {0} ...", DefaultValues.UnixSavePath);

            configFile = new FileInfo(DefaultValues.UnixSavePath);
        }

        /// <summary>Reads the lumin configuration.</summary>
        public async Task ReadLuminConfig()
        {
            Config = new LuminConfigs();

            if (!configFile.Exists)
            {
                Config = LuminConfigs.GetDefaultConfig();
                return;
            }

            FileStream fileStream = new FileStream(configFile.FullName, FileMode.Open);
            Dictionary<string, string> configs = new Dictionary<string, string>();

            using (StreamReader reader = new StreamReader(fileStream))
            {
                string line;
                while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    line = line.Trim();

                    //Skip Comments
                    if (line.StartsWith("#"))
                        continue;

                    var input = line.Split("=");
                    configs.Add(input[0].Trim(), input[1].Trim());
                }
            }

            foreach (var prop in typeof(LuminConfigs).GetProperties())
            {
                if (configs.ContainsKey(prop.Name))
                {
                    var configValue = configs[prop.Name];
                    if (null != prop && prop.CanWrite)
                    {
                        prop.SetValue(Config, Convert.ChangeType(configValue, prop.PropertyType), null);
                        logger.LogDebug("Set Configuration Value: {0} = {1}", prop.Name, configValue);
                    }
                }
            }
        }
    }
}
