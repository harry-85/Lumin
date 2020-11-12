using HeliosClockAPIStandard.Enumeration;
using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Text;

namespace HeliosClockAPIStandard.Services
{
   public class GpioTriggeredEventArgs : EventArgs
    {
        public PinValue PinValue { get; set; }
        public GpioInputPin InputPin { get; set; }
        public long TouchDuration { get; set; }

        public GpioTriggeredEventArgs(PinValue pinValue, GpioInputPin inputPin, long touchDuration)
        {
            PinValue = pinValue;
            InputPin = inputPin;
            TouchDuration = touchDuration;
        }
    }
}
