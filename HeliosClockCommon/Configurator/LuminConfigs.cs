using HeliosClockCommon.Defaults;

namespace HeliosClockCommon.Configurator
{
    public class LuminConfigs
    {
        /// <summary>Gets or sets the name of the LED Client.</summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>Gets or sets the led count for the LED Controller.</summary>
        /// <value>The led count.</value>
        public int LedCount { get; set; }

        /// <summary>Gets or sets the automatic off time of the LED controller.</summary>
        /// <value>The automatic off time.</value>
        public double AutoOffTime { get; set; }

        /// <summary>Gets the default configuration.</summary>
        /// <returns>A default configuration object.</returns>
        public static LuminConfigs GetDefaultConfig() => new LuminConfigs
        {
            Name = DefaultValues.DefaultClientName,
            LedCount = DefaultValues.DefaultLedCount,
            AutoOffTime = DefaultValues.DefaultAutoOffTime
        };
    }
}
