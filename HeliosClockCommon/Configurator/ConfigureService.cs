using HeliosClockCommon.Defaults;
using HeliosClockCommon.Properties;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                logger.LogWarning(Resources.ConfigurationFileDoestNotExist, DefaultValues.UnixSavePath);
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

                    //Check Syntax contains an =
                    if (!line.Contains("="))
                    {
                        logger.LogWarning("Cannot read configuration line: \"{0}\" ...", line);
                        continue;
                    }

                    var input = line.Split("=");

                    //Check value ob both sides of = is provided
                    if (input.Length != 2 || input.Where(s => s == null || s.Trim() == string.Empty).ToList().Count > 0)
                    {
                        logger.LogWarning("Cannot read configuration line: \"{0}\" ...", line);
                        continue;
                    }

                    configs.Add(input[0].Trim(), input[1].Trim());
                }
            }

            foreach (var prop in typeof(LuminConfigs).GetProperties())
            {
                if (configs.ContainsKey(prop.Name))
                {
                    var configValue = configs[prop.Name];
                    if (prop != null && prop.CanWrite)
                    {
                        prop.SetValue(Config, Convert.ChangeType(configValue, prop.PropertyType), null);
                        logger.LogDebug("Set Configuration Value: {0} = {1}", prop.Name, configValue);
                    }
                }
            }
        }
    }
}
