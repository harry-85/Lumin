using HeliosClockCommon.Clients;
using HeliosClockCommon.Enumerations;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using Systems;

namespace HeliosClockApp.Services
{
    public interface IHeliosService
    {
        event EventHandler<EventArgs<Color>> OnStartColorChanged;
        event EventHandler<EventArgs<Color>> OnEndColorChanged;

        public Color StartColor { get; set; }
        public Color EndColor { get; set; }
        HeliosAppClient Client { get; set; }
        Task SendColor();
        Task ConnectToServer();
        Task StartMode(LedMode mode);
        Task Stop();
        Task SetRefreshSpeed(int speed);
    }
}
