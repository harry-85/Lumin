using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuminCommon.Defaults;
using LuminCommon.Enumerations;
using LuminCommon.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace LuminCommon.Hubs
{
    public static class UserHandler
    {
        /// <summary>The connected clients.</summary>
        public static Dictionary<string, ClientConfig> ConnectedIds = new Dictionary<string, ClientConfig>();
    }

    public class HeliosHub : Hub<IHeliosHub>
    {
        /// <summary>The logger.</summary>
        private readonly ILogger<IHeliosHub> _logger;

        /// <summary>Initializes a new instance of the <see cref="HeliosHub"/> class.</summary>
        /// <param name="logger">The logger.</param>
        public HeliosHub(ILogger<IHeliosHub> logger)
        {
            this._logger = logger;
        }

        /// <summary>Notifies the controller.</summary>
        /// <param name="startColor">The start color.</param>
        /// <param name="endColor">The end color.</param>
        public async Task NotifyController(string startColor, string endColor)
        {
            _logger.LogDebug("Notify all Controllers...");
            await Clients.Group(ClientTypes.Controller.ToString()).NotifyController(startColor, endColor).ConfigureAwait(false);
        }

        /// <summary>Registers as controller.</summary>
        /// <param name="clientId">The client identifier.</param>
        public async Task RegisterAsController(string clientId)
        {
            if (clientId == null)
            {
                _logger.LogError("ClientId is null! Unknown registering device! ... ");
                return;
            }

            _logger.LogDebug("Register {0} as {1} ...", clientId, ClientTypes.Controller);
            UserHandler.ConnectedIds[clientId].ClientType = ClientTypes.Controller;

            await Groups.AddToGroupAsync(Context.ConnectionId, ClientTypes.Controller.ToString()).ConfigureAwait(false);
            await DistributeLedClients(Clients.Caller).ConfigureAwait(false);
        }

        /// <summary>Registers as led client.</summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="name">The name.</param>
        public async Task RegisterAsLedClient(string clientId, string name)
        {
            _logger.LogDebug("Register {0} with ID {1} as {2} ...", name, clientId, ClientTypes.LedClient);
            ClientConfig clientConfig = UserHandler.ConnectedIds[clientId];
            clientConfig.ClientType = ClientTypes.LedClient;
            clientConfig.ClientName = name;

            await Groups.AddToGroupAsync(Context.ConnectionId, ClientTypes.LedClient.ToString()).ConfigureAwait(false);
            await DistributeLedClients(Clients.Group(ClientTypes.Controller.ToString())).ConfigureAwait(false);
        }

        /// <summary>Sets the color string.</summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="startColor">The start color.</param>
        /// <param name="endColor">The end color.</param>
        /// <param name="interpolationMode">The interpolation mode.</param>
        public async Task SetColorString(string clientId, string startColor, string endColor, string interpolationMode)
        {
            await GetHubToForward(clientId).SetColorString(startColor, endColor, interpolationMode).ConfigureAwait(false);
        }

        /// <summary>Sets the ON/OFF.</summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="onOff">The on off.</param>
        /// <param name="side">The side.</param>
        public async Task SetOnoff(string clientId, string onOff, string side)
        {
            await GetHubToForward(clientId).SetOnOff(onOff, side).ConfigureAwait(false);
        }

        /// <summary>Starts the mode.</summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="mode">The mode.</param>
        public async Task StartMode(string clientId, string mode)
        {
            await GetHubToForward(clientId).StartMode(mode).ConfigureAwait(false);
        }

        /// <summary>Stops the specified client identifier.</summary>
        /// <param name="clientId">The client identifier.</param>
        public async Task Stop(string clientId)
        {
            await GetHubToForward(clientId).Stop().ConfigureAwait(false);
        }

        /// <summary>Sets the brightness.</summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="brightness">The brightness.</param>
        public async Task SetBrightness(string clientId, string brightness)
        {
            await GetHubToForward(clientId).SetBrightness(brightness).ConfigureAwait(false);
        }

        /// <summary>Sets the refresh speed.</summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="speed">The speed.</param>
        public async Task SetRefreshSpeed(string clientId, string speed)
        {
            await GetHubToForward(clientId).SetRefreshSpeed(speed).ConfigureAwait(false);
        }

        /// <summary>Sets the random color.</summary>
        /// <param name="clientId">The client identifier.</param>
        public async Task SetRandomColor(string clientId)
        {
            await GetHubToForward(clientId).SetRandomColor().ConfigureAwait(false);
        }

        /// <summary>Called when a connection with the hub is terminated.</summary>
        /// <param name="exception"></param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous disconnect.</returns>
        public override Task OnDisconnectedAsync(Exception exception)
        {
            UserHandler.ConnectedIds.Remove(Context.ConnectionId);
            _logger.LogInformation("Client Disconnected. Id: {0} | Client Count: {1} ...", Context.ConnectionId, UserHandler.ConnectedIds.Count);
            return base.OnDisconnectedAsync(exception);
        }

        /// <summary>Called when a new connection is established with the hub.</summary>
        public override async Task OnConnectedAsync()
        {
            UserHandler.ConnectedIds.Add(Context.ConnectionId, new ClientConfig { ClientType = Enumerations.ClientTypes.Unregistered });
            _logger.LogInformation("New Client Connected. Id: {0} | Client Count: {1} ...", Context.ConnectionId, UserHandler.ConnectedIds.Count);

            await base.OnConnectedAsync().ConfigureAwait(false);
        }

        /// <summary>Distributes the led clients.</summary>
        /// <param name="client">The client.</param>
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
