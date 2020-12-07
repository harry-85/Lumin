using System.Threading.Tasks;

namespace HeliosClockCommon.Configurator
{
    public interface IConfigureService
    {
        LuminConfigs Config { get; set; }
        /// <summary>Reads the lumin configuration.</summary>
        Task ReadLuminConfig();
    }
}
