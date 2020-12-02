using HeliosClockCommon.Models;
using System.Collections.Generic;

namespace HeliosClockApp.Services
{
    public class LedClientManager
    {
        /// <summary>Gets the led clients.</summary>
        /// <value>The led clients.</value>
        public Dictionary<string, LedClient> LedClients { get; }

        /// <summary>Initializes a new instance of the <see cref="LedClientManager"/> class.</summary>
        public LedClientManager()
        {
            LedClients = new Dictionary<string, LedClient>();
        }

        /// <summary>Sets the new clients.</summary>
        /// <param name="clients">The clients.</param>
        public void SetNewClients(string clients)
        {
            LedClients.Clear();
            foreach (var ledClient in clients.Split(";"))
            {
                if (ledClient != null && ledClient.Length > 0)
                {
                    var clientData = ledClient.Split("@");

                    if (clientData != null && clientData.Length > 1)
                    {
                        LedClients.Add(ledClient, new LedClient(clientData[1], clientData[0]));
                    }
                }
            }
        }
    }
}
