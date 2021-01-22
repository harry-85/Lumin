using System.Net.Sockets;
using LuminCommon.Configurator;
using Microsoft.Extensions.Logging;

namespace LuminCommon.Discorvery
{
    public class DiscoverFactory
    {
        /// <summary>Gets the UDP client.</summary>
        /// <value>The UDP client.</value>
        public UdpClient UdpClient { get; private set; }

        /// <summary>Initializes a new instance of the <see cref="DiscoverFactory" /> class.</summary>
        /// <param name="luminConfiguration">The lumin configuration.</param>
        /// <param name="logger">The logger.</param>
        public DiscoverFactory(ILuminConfiguration luminConfiguration, ILogger<DiscoverFactory> logger)
        {
            Intitialize(luminConfiguration, logger);
        }

        /// <summary>Initializes a new default instance of the <see cref="DiscoverFactory"/> class.</summary>
        public DiscoverFactory()
        {
            Intitialize(new LuminConfigs(), null);
        }

        private void Intitialize(ILuminConfiguration luminConfiguration, ILogger<DiscoverFactory> logger)
        {
            logger?.LogInformation("Starting Discover Factory. Use Port: {0}...", luminConfiguration.DiscoveryPort);
            UdpClient = new UdpClient(luminConfiguration.DiscoveryPort)
            {
                EnableBroadcast = true
            };
        }
    }
}
