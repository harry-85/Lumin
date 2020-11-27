using HeliosClockCommon.Enumerations;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeliosClockCommon.Interfaces
{
    public interface IHeliosHub
    {
        Task SignalClient(string user, string message);
        Task SetAlarm(DateTime alarmTime);
        Task SetColor(Color startColor, Color endColor);

        Task SetRandomColor();

        Task SetColorString(string startColor, string endColor, string interpolationMode);

        Task StartMode(string mode);

        Task Stop();

        Task SetRefreshSpeed(string speed);

        Task SetOnOff(string onOff, string side);

        Task SetBrightness(string brightness);
    }
}
