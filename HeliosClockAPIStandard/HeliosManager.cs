using HeliosClockCommon.Enumerations;
using HeliosClockCommon.Helper;
using HeliosClockCommon.Interfaces;
using HeliosClockCommon.LedCommon;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace HeliosClockAPIStandard
{
    public class HeliosManager : IHeliosManager
    {
        private System.Timers.Timer autoOffTmer;

        public ILedController LedController { get; set; }
        public int RefreshSpeed { get; set; }
        public Color StartColor { get; set; }
        public Color EndColor { get; set; }

        public double AutoOffTime { get; set; }

        public HeliosManager(ILedController ledController)
        {
            RefreshSpeed = 100;
            this.LedController = ledController;

            AutoOffTime = 4 * 60 * 60 * 1000; // milliseconds

            autoOffTmer = new System.Timers.Timer(AutoOffTime);
            autoOffTmer.Elapsed += AutoOffTmer_Elapsed;
        }

        private async void AutoOffTmer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            await SetOnOff("off").ConfigureAwait(false);
        }

        public async Task SetColor(Color startColor, Color endColor, ColorInterpolationMode interpolationMode, CancellationToken cancellationToken)
        {
            autoOffTmer.Stop();
            autoOffTmer.Start();

            var leds = new LedScreen(LedController);

            StartColor = startColor;
            EndColor = endColor;

            var colors = await ColorHelpers.ColorGradient(StartColor, EndColor, LedController.LedCount, interpolationMode).ConfigureAwait(false);

            for (int i = 0; i < LedController.LedCount; i++)
            {
                leds.SetPixel(ref i, colors[i]);
            }

            //LedController.IsSmoothing = true;
            await LedController.SendPixels(leds.pixels).ConfigureAwait(false);
            //LedController.IsSmoothing = false;
        }

        public async Task RunLedMode(LedMode mode, CancellationToken cancellationToken)
        {
            autoOffTmer.Stop();
            autoOffTmer.Start();
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
            if (onOff == "on")
            {
                autoOffTmer.Stop();
                autoOffTmer.Start();
            }

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


            int startPoint = -1 * knightCount;
            int knightRoundCount = 0;
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

                    if (startPoint + i >= 0)
                    {
                        if (startPoint + i >= knightCount)
                        {
                        
                        }


                        leds.SetPixel(ref ledIndex, colors[i - (i % knightCount) * knightCount]);
                    }
                    else
                        leds.SetPixel(ref ledIndex, Color.Black);

                }

                startPoint++;

                if (startPoint >= LedController.LedCount + 2 * knightCount)
                {
                    startPoint = 0;
                    clockWise = !clockWise;
                }
            }
        }
    }
}