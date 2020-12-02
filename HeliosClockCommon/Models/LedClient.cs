using HeliosClockCommon.Defaults;

namespace HeliosClockCommon.Models
{
    public class LedClient
    {
        public string Name { get; set; }
        public string Id { get; }

        /// <summary>Initializes a new instance of the <see cref="LedClient"/> class.</summary>
        /// <param name="name">The name.</param>
        /// <param name="id">The identifier.</param>
        public LedClient(string name, string id)
        {
            Name = name;
            Id = id;
        }

        public static LedClient AllClients => new LedClient(DefaultValues.AllClients, DefaultValues.AllClients);
    }
}
