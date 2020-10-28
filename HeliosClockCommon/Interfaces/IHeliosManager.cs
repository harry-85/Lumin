using HeliosClockCommon.Enumerations;
using HeliosClockCommon.LedCommon;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HeliosClockCommon.Interfaces
{
    public interface IHeliosManager
    {
        ILedController LedController { get; set; }

        int RefreshSpeed { get; set; }
        Task RunLedMode(LedMode mode, CancellationToken cancellationToken);
    }
}
