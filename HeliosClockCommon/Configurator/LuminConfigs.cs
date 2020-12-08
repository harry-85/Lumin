using System.Text;
using HeliosClockCommon.Defaults;

namespace HeliosClockCommon.Configurator
{
    public class LuminConfigs :ILuminConfiguration
    {
        /// <summary>Initializes a new instance of the <see cref="LuminConfigs"/> class.</summary>
        /// <param name="createDefault">if set to <c>true</c> [create default].</param>
        public LuminConfigs(bool createDefault)
        {
            if (createDefault)
            {
                Name = DefaultValues.DefaultClientName;
                LedCount = DefaultValues.DefaultLedCount;
                AutoOffTime = DefaultValues.DefaultAutoOffTime;
            }
            else
            {
                CreateDefaultLuminConfigs();
            }
        }

        /// <summary>Initializes a new instance of the <see cref="LuminConfigs"/> class.</summary>
        /// <remarks>Creates a new default configuration.</summary>
        public LuminConfigs()
        {
            CreateDefaultLuminConfigs();
        }

        /// <summary>Gets or sets the name of the LED Client.</summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>Gets or sets the led count for the LED Controller.</summary>
        /// <value>The led count.</value>
        public int LedCount { get; set; }

        /// <summary>Gets or sets the automatic off time of the LED controller.</summary>
        /// <value>The automatic off time.</value>
        public double AutoOffTime { get; set; }

        /// <summary>Creates the default lumin configurations.</summary>
        private void CreateDefaultLuminConfigs()
        {
            var defaultConfig = new LuminConfigs(true);
            foreach (var prop in typeof(LuminConfigs).GetProperties())
            {
                if (prop.CanWrite && prop.CanRead)
                {
                    prop.SetValue(this, prop.GetValue(defaultConfig));
                }
            }
        }
    }
}
