using HeliosClockCommon.Enumerations;
using HeliosClockCommon.Helper;
using HeliosClockCommon.Interfaces;
using HeliosClockCommon.LedCommon;
using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace HeliosClockAPIStandard
{
    public class HeliosManager : IHeliosManager
    {
        private readonly System.Timers.Timer autoOffTmer;

        public ILedController LedController { get; set; }
        public int RefreshSpeed { get; set; }
        public Color StartColor { get; set; }
        public Color EndColor { get; set; }
        public int Brightness { get { return LedController.Brightness; } set { LedController.Brightness = value; } }

        public bool IsRunning { get => autoOffTmer.Enabled; }

        public double AutoOffTime { get; set; }

        /// <summary>Initializes a new instance of the <see cref="HeliosManager"/> class.</summary>
        /// <param name="ledController">The led controller.</param>
        public HeliosManager(ILedController ledController)
        {
            RefreshSpeed = 100;
            this.LedController = ledController;

            AutoOffTime = 4 * 60 * 60 * 1000; // milliseconds

            autoOffTmer = new System.Timers.Timer(AutoOffTime);
            autoOffTmer.Elapsed += AutoOffTmer_Elapsed;
        }

        /// <summary>Refreshes the screen.</summary>
        public async Task RefreshScreen()
        {
            await LedController.Repaint().ConfigureAwait(false);
        }

        /// <summary>Handles the Elapsed event of the AutoOffTmer control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Timers.ElapsedEventArgs"/> instance containing the event data.</param>
        private async void AutoOffTmer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            await SetOnOff(PowerOnOff.Off, LedSide.Full, Color.White).ConfigureAwait(false);
        }

        /// <summary>Sets the random color.</summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task SetRandomColor(CancellationToken cancellationToken)
        {
            Random rnd = new Random();
            Color startColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
            //Color startColor = Color.FromArgb(RandomNumberGenerator.Between(0, 255), RandomNumberGenerator.Between(0, 255), RandomNumberGenerator.Between(0, 255));

            rnd = new Random();
            Color endColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
            //Color endColor = Color.FromArgb(RandomNumberGenerator.Between(0, 255), RandomNumberGenerator.Between(0, 255), RandomNumberGenerator.Between(0, 255));

            await SetColor(startColor, endColor, ColorInterpolationMode.HueMode, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Sets the color.</summary>
        /// <param name="startColor">The start color.</param>
        /// <param name="endColor">The end color.</param>
        /// <param name="interpolationMode"></param>
        /// <param name="cancellationToken">The cancellation token.</param>
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

            LedController.IsSmoothing = true;
            await LedController.SendPixels(leds.pixels).ConfigureAwait(false);
            LedController.IsSmoothing = false;
        }

        /// <summary>Runs the led mode.</summary>
        /// <param name="mode">The mode.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
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
                    
                    case LedMode.KnightRider:
                        await KnightRiderMode(cancellationToken).ConfigureAwait(false);
                        break;
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

        public async Task SetOnOff(PowerOnOff onOff, LedSide side, Color color)
        {
            if (onOff == PowerOnOff.On)
            {
                autoOffTmer.Start();
            }
            else
            {
                autoOffTmer.Stop();
            }
            
            LedController.IsSmoothing = false;

            var leds = new LedScreen(LedController);

            for (int i = 0; i < LedController.LedCount; i++)
            {
                if (side == LedSide.Full)
                {
                    leds.SetPixel(ref i, onOff == PowerOnOff.On ? Color.White : Color.Black);
                }
                else if (side == LedSide.Left && i < (int)Math.Round(LedController.LedCount / 2.0))
                {
                    leds.SetPixel(ref i, onOff == PowerOnOff.On ? Color.White : Color.Black);
                }
                else if (side == LedSide.Right && i >= (int)Math.Round(LedController.LedCount / 2.0))
                {
                    leds.SetPixel(ref i, onOff == PowerOnOff.On ? Color.White : Color.Black);
                }
                else
                {
                    if(LedController.ActualScreen != null)
                        leds.SetPixel(ref i, LedController.ActualScreen[i].LedColor);
                }
            }

            await LedController.SendPixels(leds.pixels).ConfigureAwait(false);
        }

        private async Task KnightRiderMode(CancellationToken cancellationToken)
        {
            await Task.Run(async () =>
            {
                int ledCount = LedController.LedCount;
                int knightCount = (int)Math.Round(((double)ledCount / 100.0 * 20.0), 0);

                var leds = new LedScreen(LedController);

                int colorCount = 1;
                int startIndex = 0;

                bool isClockwise = true;

                while (!cancellationToken.IsCancellationRequested)
                {
                    var colors = await ColorHelpers.DimColor(StartColor, knightCount, true).ConfigureAwait(false);

                    for (int i = 0; i < ledCount; i++)
                    {
                        int index = isClockwise ? i : ledCount - i - 1;
                        if (i >= startIndex && i < startIndex + colorCount)
                            leds.SetPixel(ref index, colors[i - startIndex]);
                        else
                            leds.SetPixel(ref index, Color.Black);

                    }

                    if (colorCount < knightCount - 1)
                        colorCount++;

                    await LedController.SendPixels(leds.pixels).ConfigureAwait(false);
                    
                    startIndex++;

                    if (startIndex >= ledCount)
                    {
                        colorCount = 1;
                        startIndex = 0;
                        isClockwise = !isClockwise;
                    }

                    await Task.Delay(RefreshSpeed).ConfigureAwait(false);
                }
            }).ConfigureAwait(false);
        }
    }
}