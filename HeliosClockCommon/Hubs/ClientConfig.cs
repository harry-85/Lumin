using HeliosClockCommon.Enumerations;

namespace HeliosClockCommon.Hubs
{
    public class ClientConfig
    {
        /// <summary>Gets or sets the type of the client.</summary>
        /// <value>The type of the client.</value>
        public ClientTypes ClientType { get; set; }

        /// <summary>Gets or sets the name of the client.</summary>
        /// <value>The name of the client.</value>
        public string ClientName { get; set; }
    }
}