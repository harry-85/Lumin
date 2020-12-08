using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace HeliosClockCommon.Configurator
{
    public interface IConfigureService : IHostedService
    {
        /// <summary>Reads the lumin configuration.</summary>
        Task ReadLuminConfig();
    }
}
