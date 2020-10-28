using HeliosClockCommon.Enumerations;
using HeliosClockCommon.Interfaces;
using HeliosClockCommon.LedCommon;
using System.Threading;
using System.Threading.Tasks;

namespace HeliosClockAPIStandard
{
    public class HeliosManager : IHeliosManager
    {
        public ILedController LedController { get; set; }
        public int RefreshSpeed { get; set; }

        public HeliosManager(ILedController ledController)
        {
            RefreshSpeed = 100;
            this.LedController = ledController;
        }

        public Task RunLedMode(LedMode mode, CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                switch (mode)
                {
                    case LedMode.Spin:
                        await SpinLeds(cancellationToken).ConfigureAwait(false);
                        break;
                }
            }).ConfigureAwait(false);

            return Task.CompletedTask;
        }

        private async Task SpinLeds(CancellationToken cancellationToken)
        {
            var oldOffest = LedController.PixelOffset;

            while (!cancellationToken.IsCancellationRequested)
            {
                LedController.PixelOffset++;
                await Task.Delay(RefreshSpeed).ConfigureAwait(false);
                if (LedController.PixelOffset >= LedController.LedCount)
                {
                    LedController.PixelOffset = 0;
                }
                await LedController.Repaint().ConfigureAwait(false);
            }
            LedController.PixelOffset = oldOffest;
        }
    }
}
