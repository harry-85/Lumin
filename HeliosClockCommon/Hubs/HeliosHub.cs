using HeliosClockCommon.Defaults;
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

        public async Task NotifyController(string startColor, string endColor)
        {
            _logger.LogDebug("Notify all Controllers...");
            await Clients.Group(ClientTypes.Controller.ToString()).NotifyController(startColor, endColor).ConfigureAwait(false);
        }

        public async Task RegisterAsController(string clientId)
        {
            _logger.LogDebug("Register {0} as {1} ...", clientId, ClientTypes.Controller);
            UserHandler.ConnectedIds[clientId].ClientType = ClientTypes.Controller;

            await Groups.AddToGroupAsync(Context.ConnectionId, ClientTypes.Controller.ToString()).ConfigureAwait(false);
            await DistributeLedClients(Clients.Caller).ConfigureAwait(false);
        }

        public async Task RegisterAsLedClient(string clientId, string name)
        {
            _logger.LogDebug("Register {0} with ID {1} as {2} ...", name, clientId, ClientTypes.LedClient);
            ClientConfig clientConfig = UserHandler.ConnectedIds[clientId];
            clientConfig.ClientType = ClientTypes.LedClient;
            clientConfig.ClientName = name;

            await Groups.AddToGroupAsync(Context.ConnectionId, ClientTypes.LedClient.ToString()).ConfigureAwait(false);
            await DistributeLedClients(Clients.Group(ClientTypes.Controller.ToString())).ConfigureAwait(false);
        }

        public async Task SetColorString(string clientId, string startColor, string endColor, string interpolationMode)
        {
            await GetHubToForward(clientId).SetColorString(startColor, endColor, interpolationMode).ConfigureAwait(false);
        }

        public async Task SetOnoff(string clientId, string onOff, string side)
        {
            await GetHubToForward(clientId).SetOnOff(onOff, side).ConfigureAwait(false);
        }

        public async Task StartMode(string clientId, string mode)
        {
            await GetHubToForward(clientId).StartMode(mode).ConfigureAwait(false);
        }

        public async Task Stop(string clientId)
        {
            await GetHubToForward(clientId).Stop().ConfigureAwait(false);
        }

        public async Task SetBrightness(string clientId, string brightness)
        {
            await GetHubToForward(clientId).SetBrightness(brightness).ConfigureAwait(false);
        }

        public async Task SetRefreshSpeed(string clientId, string speed)
        {
            await GetHubToForward(clientId).SetRefreshSpeed(speed).ConfigureAwait(false);
        }

        public async Task SetRandomColor(string clientId)
        {
            await GetHubToForward(clientId).SetRandomColor().ConfigureAwait(false);
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            UserHandler.ConnectedIds.Remove(Context.ConnectionId);
            _logger.LogInformation("Client Disconnected. Id: {0} | Client Count: {1} ...", Context.ConnectionId, UserHandler.ConnectedIds.Count);
            return base.OnDisconnectedAsync(exception);
        }

        public override async Task OnConnectedAsync()
        {
            UserHandler.ConnectedIds.Add(Context.ConnectionId, new ClientConfig { ClientType = Enumerations.ClientTypes.Unregistered });
            _logger.LogInformation("New Client Connected. Id: {0} | Client Count: {1} ...", Context.ConnectionId, UserHandler.ConnectedIds.Count);

            await base.OnConnectedAsync().ConfigureAwait(false);
        }

        private async Task DistributeLedClients(IHeliosHub client)
        {
            //Distribute the LedClient list to the controllers
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var ledClient in UserHandler.ConnectedIds.Where(s => s.Value.ClientType == ClientTypes.LedClient))
            {
                stringBuilder.Append(string.Format("{0}@{1};", ledClient.Key, ledClient.Value.ClientName));
            }

            await client.LedClientChanged(stringBuilder.ToString()).ConfigureAwait(false);
        }

        /// <summary>Gets the hub to forward.</summary>
        /// <param name="clientId">The client identifier.</param>
        private IHeliosHub GetHubToForward(string clientId)
        {
            if (clientId == DefaultValues.AllClients)
                return Clients.All;

            return Clients.Group(clientId);
        }
    }
}
