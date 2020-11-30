using HeliosClockCommon.Enumerations;
using HeliosClockCommon.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeliosClockCommon.Hubs
{
    public static class UserHandler
    {
        public static Dictionary<string, ClientConfig> ConnectedIds = new Dictionary<string, ClientConfig>();
    }

    public class HeliosHub : Hub<IHeliosHub>
    {
        private readonly ILogger<IHeliosHub> _logger;

        /// <summary>Initializes a new instance of the <see cref="HeliosHub"/> class.</summary>
        /// <param name="logger">The logger.</param>
        public HeliosHub(ILogger<IHeliosHub> logger)
        {
            this._logger = logger;
        }

        public async Task RegisterAsController(string clientId)
        {
            _logger.LogDebug("Register {0} as {1} ...", clientId, ClientTypes.Controller);
            UserHandler.ConnectedIds[clientId].ClientTyype = ClientTypes.Controller;

            await Groups.AddToGroupAsync(Context.ConnectionId, ClientTypes.Controller.ToString()).ConfigureAwait(false);
            await DistributeLedClients(Clients.Caller).ConfigureAwait(false);
        }

        public async Task RegisterAsLedClient(string clientId)
        {
            _logger.LogDebug("Register {0} as {1} ...", clientId, ClientTypes.LedClient);
            UserHandler.ConnectedIds[clientId].ClientTyype = ClientTypes.LedClient;

            await Groups.AddToGroupAsync(Context.ConnectionId, ClientTypes.LedClient.ToString()).ConfigureAwait(false);
            await DistributeLedClients(Clients.Group(ClientTypes.Controller.ToString())).ConfigureAwait(false);
        }

        public async Task SetColorString(string startColor, string endColor, string interpolationMode)
        {
            await Clients.All.SetColorString(startColor, endColor, interpolationMode).ConfigureAwait(false);
        }

        public async Task SetOnoff(string onOff, string side)
        {
            await Clients.All.SetOnOff(onOff, side).ConfigureAwait(false);
        }

        public async Task StartMode(string mode)
        {
            await Clients.All.StartMode(mode).ConfigureAwait(false);
        }

        public async Task Stop()
        {
            await Clients.All.Stop().ConfigureAwait(false);
        }

        public async Task SetBrightness(string brightness)
        {
            await Clients.All.SetBrightness(brightness).ConfigureAwait(false);
        }

        public async Task SetRefreshSpeed(string speed)
        {
            await Clients.All.SetRefreshSpeed(speed).ConfigureAwait(false);
        }

        public async Task SetRandomColor()
        {
            await Clients.All.SetRandomColor().ConfigureAwait(false);
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            UserHandler.ConnectedIds.Remove(Context.ConnectionId);
            _logger.LogInformation("Client Disconnected. Id: {0} | Client Count: {1} ...", Context.ConnectionId, UserHandler.ConnectedIds.Count);
            return base.OnDisconnectedAsync(exception);
        }

        public override async Task OnConnectedAsync()
        {
            UserHandler.ConnectedIds.Add(Context.ConnectionId, new ClientConfig { ClientTyype = Enumerations.ClientTypes.Unregistered });
            _logger.LogInformation("New Client Connected. Id: {0} | Client Count: {1} ...", Context.ConnectionId, UserHandler.ConnectedIds.Count);

            await base.OnConnectedAsync().ConfigureAwait(false);
        }

        private async Task DistributeLedClients(IHeliosHub client)
        {
            //Distribute the LedClient list to the controllers
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var ledClient in UserHandler.ConnectedIds.Where(s => s.Value.ClientTyype == ClientTypes.LedClient))
            {
                stringBuilder.Append(ledClient.Key);
                stringBuilder.Append(";");
            }

            await client.LedClientChanged(stringBuilder.ToString()).ConfigureAwait(false);
        }
    }
}
