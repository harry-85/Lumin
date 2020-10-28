using HeliosClockCommon.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeliosClockCommon.Hubs
{
    public class HeliosHub : Hub<IHeliosHub>
    {
        public async Task SetColorString(string startColor, string endColor)
        {
            await Clients.All.SetColorString(startColor, endColor).ConfigureAwait(false);
        }

        public async Task StartMode(string mode)
        {
            await Clients.All.StartMode(mode).ConfigureAwait(false);
        }

        public async Task Stop()
        {
            await Clients.All.Stop().ConfigureAwait(false);
        }


        public async Task SetRefreshSpeed(string speed)
        {
            await Clients.All.SetRefreshSpeed(speed).ConfigureAwait(false);
        }


        public override Task OnConnectedAsync()
        {
            Console.WriteLine("New Client Connected ...");
            return base.OnConnectedAsync();
        }
    }
}
