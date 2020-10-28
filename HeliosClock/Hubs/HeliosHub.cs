using HeliosClock.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeliosClock.Hubs
{
    public class HeliosHub : Hub<IHeliosHub> 
    {
        public async Task SignalClient(string user, string message)
        {
            await Clients.All.SignalClient(user, message);
        }

       
    }
}
