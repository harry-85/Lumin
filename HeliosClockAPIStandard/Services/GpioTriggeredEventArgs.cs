using HeliosClockAPIStandard.Enumeration;
using System;
using System.Device.Gpio;

namespace HeliosClockAPIStandard.Services
{
    public class GpioTriggeredEventArgs : EventArgs
    {
        /// <summary>Gets the pin value.</summary>
        /// <value>The pin value.</value>
        public PinValue PinValue { get; init; }

        /// <summary>Gets or sets the input pin.</summary>
        /// <value>The input pin.</value>
        public GpioInputPin InputPin { get; init; }

        /// <summary>Gets or sets the duration of the touch.</summary>
        /// <value>The duration of the touch.</value>
        public long TouchDuration { get; init; }

        /// <summary>Gets or sets the wait for event result.</summary>
        /// <value>The wait for event result.</value>
        public WaitForEventResult WaitForEventResult { get; init; }
    }
}
