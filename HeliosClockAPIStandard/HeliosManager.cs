using HeliosClockCommon.Enumerations;
using HeliosClockCommon.Helper;
using HeliosClockCommon.Interfaces;
using HeliosClockCommon.LedCommon;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace HeliosClockAPIStandard
{
    public class HeliosManager : IHeliosManager
    {
        public ILedController LedController { get; set; }
        public int RefreshSpeed { get; set; }
        public Color StartColor { get; set; }
        public Color EndColor { get; set; }

        public HeliosManager(ILedController ledController)
        {
            RefreshSpeed = 100;
            this.LedController = ledController;
        }

        public async Task RunLedMode(LedMode mode, CancellationToken cancellationToken)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () =>
            {
                switch (mode)
                {
                    case LedMode.Spin:
                        await SpinLeds(cancellationToken).ConfigureAwait(false);
                        break;
                    /*
                    case LedMode.KnightRider:
                        await KnightRiderMode(cancellationToken).ConfigureAwait(false);
                        break;
                    */
                    default:
                        break;
                }
            }).ConfigureAwait(false);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            await Task.CompletedTask.ConfigureAwait(false);
        }

        /// <summary>Spins the leds.</summary>
        /// <param name="cancellationToken">The cancellation token.</param>
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
            await LedController.Repaint().ConfigureAwait(false);
        }

        public async Task SetOnOff(string onOff)
        {
            LedController.IsSmoothing = false;

            var leds = new LedScreen(LedController);

            for (int i = 0; i < LedController.LedCount; i++)
            {
                leds.SetPixel(ref i, onOff == "on" ? Color.White : Color.Black);
            }

            await LedController.SendPixels(leds.pixels).ConfigureAwait(false);
        }

        private async Task KnightRiderMode(CancellationToken cancellationToken)
        {
            int ledCount = LedController.LedCount;
            int knightCount = (int)Math.Round(((double)ledCount / 100.0 * 10.0), 0);

            var leds = new LedScreen(LedController);

            bool clockWise = true;


            int startPoint = 0;
            int colorCount = 0;
            int fadeInDelta = 0;
            int knightDelta = 0;

            while (!cancellationToken.IsCancellationRequested)
            {
                int fadeOutDelta = 0;
                fadeInDelta = 0;
                fadeOutDelta = 0;

                for (int i = 0; i < LedController.LedCount; i++)
                {
                    var colors = await ColorHelpers.DimColor(StartColor, knightCount).ConfigureAwait(false);

                    int ledIndex = clockWise ? i : LedController.LedCount - 1 - i;

                    if (i >= knightDelta && i < LedController.LedCount && colorCount - fadeInDelta >= 0)
                    {
                        leds.SetPixel(ref ledIndex, colors[colorCount - fadeInDelta]);
                        fadeInDelta++;
                    }
                    else if (startPoint - knightCount + colorCount > LedController.LedCount && fadeOutDelta < knightCount && colorCount - fadeOutDelta >= 0)
                    {
                        leds.SetPixel(ref ledIndex, colors[colorCount - fadeOutDelta]);
                        fadeOutDelta++;
                    }
                    else
                    {
                        leds.SetPixel(ref ledIndex, Color.Black);
                    }

                    
                }

                if (startPoint + 1 % knightCount == 0)
                {
                    knightDelta++;
                }

                await LedController.SendPixels(leds.pixels).ConfigureAwait(false);
                await Task.Delay(RefreshSpeed).ConfigureAwait(false);

                if(colorCount < knightCount - 1)
                    colorCount++;

                startPoint++;

                if (startPoint >= LedController.LedCount + knightCount)
                {
                    startPoint = 0;
                    knightDelta = 0;
                    clockWise = !clockWise;
                }
            }
        }
    }
}
