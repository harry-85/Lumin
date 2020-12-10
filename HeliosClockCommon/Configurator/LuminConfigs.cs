using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using HeliosClockCommon.Defaults;

namespace HeliosClockCommon.Configurator
{
    public class LuminConfigs : ILuminConfiguration, INotifyPropertyChanged
    {
        private string name;
        private int ledCount;
        private double autoOffTime;

        /// <summary>Occurs when [on configuration changed].</summary>
        public event EventHandler<EventArgs<string>> OnConfigurationChanged;
        public event PropertyChangedEventHandler PropertyChanged;

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

            PropertyChanged += LuminConfigs_PropertyChanged;
        }

        /// <summary>Handles the PropertyChanged event of the LuminConfigs control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void LuminConfigs_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnConfigurationChanged?.Invoke(this, new EventArgs<string>(e.PropertyName));
        }

        /// <summary>Initializes a new instance of the <see cref="LuminConfigs"/> class.</summary>
        /// <remarks>Creates a new default configuration.</summary>
        public LuminConfigs()
        {
            CreateDefaultLuminConfigs();
            PropertyChanged += LuminConfigs_PropertyChanged;
        }

        /// <summary>Gets or sets the name of the LED Client.</summary>
        /// <value>The name.</value>
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Gets or sets the led count for the LED Controller.</summary>
        /// <value>The led count.</value>
        public int LedCount
        {
            get => ledCount;
            set
            {
                ledCount = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Gets or sets the automatic off time of the LED controller.</summary>
        /// <value>The automatic off time.</value>
        public double AutoOffTime
        {
            get => autoOffTime;
            set
            {
                autoOffTime = value;
                OnPropertyChanged();
            }
        }

        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

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
