using HeliosClockCommon.Configurator;
using HeliosClockCommon.Enumerations;
using HeliosClockCommon.EventArgs;
using HeliosClockCommon.Helper;
using HeliosClockCommon.Interfaces;
using HeliosClockCommon.LedCommon;
using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace HeliosClockAPIStandard
{
    public class LuminManager : ILuminManager
    {
        private readonly System.Timers.Timer autoOffTmer;
        private CancellationTokenSource cancellationTokenSource;
        private CancellationToken cancellationToken;

        public event EventHandler<NotifyControllerEventArgs> NotifyController;

        public ILedController LedController { get; set; }
        public int RefreshSpeed { get; set; }
        public Color StartColor { get; set; }
        public Color EndColor { get; set; }
        public int Brightness
        {
            get { return LedController.Brightness; }
            set { LedController.Brightness = value; }
        }

        public bool IsRunning => autoOffTmer.Enabled;

        public bool IsModeRunning { get; set; }

        public double AutoOffTime { get; set; }

        /// <summary>Initializes a new instance of the <see cref="HeliosManager"/> class.</summary>
        /// <param name="ledController">The led controller.</param>
        public LuminManager(ILedController ledController)
        {
            RefreshSpeed = 100;
            this.LedController = ledController;

            AutoOffTime = 4 * 60 * 60 * 1000; // milliseconds

            autoOffTmer = new System.Timers.Timer(AutoOffTime);
            autoOffTmer.Elapsed += AutoOffTmer_Elapsed;
        }

        /// <summary>Notifies the controllers.</summary>
        public void NotifyControllers()
        {
            NotifyController?.Invoke(this, new NotifyControllerEventArgs { StartColor = StartColor, EndColor = EndColor });
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
            await SetOnOff(PowerOnOff.Off, LedSide.Full, Color.Black).ConfigureAwait(false);
            Brightness = 255;
        }

        /// <summary>Sets the random color.</summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task SetRandomColor()
        {
            Random rnd = new();
            Color startColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));

            rnd = new Random();
            Color endColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));

            await SetColor(startColor, endColor, ColorInterpolationMode.HueMode).ConfigureAwait(false);
        }

        /// <summary>Sets the color.</summary>
        /// <param name="startColor">The start color.</param>
        /// <param name="endColor">The end color.</param>
        /// <param name="interpolationMode"></param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task SetColor(Color startColor, Color endColor, ColorInterpolationMode interpolationMode)
        {
            autoOffTmer.Stop();
            autoOffTmer.Start();

            var leds = new LedScreen(LedController);

            StartColor = startColor;
            EndColor = endColor;

            //If mode is running, let only the color object change, but do not transfer colors to screen.
            if (IsModeRunning) return;

            var colors = await ColorHelpers.ColorGradient(StartColor, EndColor, LedController.LedCount, interpolationMode).ConfigureAwait(false);

            for (int i = 0; i < LedController.LedCount; i++)
            {
                leds.SetPixel(ref i, colors[i]);
            }

            LedController.IsSmoothing = true;
            await LedController.SendPixels(leds.pixels).ConfigureAwait(false);
            LedController.IsSmoothing = false;
        }

        public async Task StopLedMode()
        {
            cancellationTokenSource?.Cancel();

            while (IsModeRunning && !cancellationToken.IsCancellationRequested)
                await Task.Delay(1).ConfigureAwait(false);

            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;
        }

        /// <summary>Sets the LEDs either On or Off.</summary>
        /// <param name="onOff">The on off.</param>
        /// <param name="side">The side.</param>
        /// <param name="onColor">Color of the on.</param>
        public async Task SetOnOff(PowerOnOff onOff, LedSide side, Color onColor)
        {
            if (onOff == PowerOnOff.On)
            {
                autoOffTmer.Start();
            }
            else
            {
                autoOffTmer.Stop();
                await StopLedMode().ConfigureAwait(false);
            }

            LedController.IsSmoothing = false;

            var leds = new LedScreen(LedController);

            for (int i = 0; i < LedController.LedCount; i++)
            {
                if (side == LedSide.Full)
                {
                    leds.SetPixel(ref i, onOff == PowerOnOff.On ? onColor : Color.Black);
                }
                else if (side == LedSide.Left && i < (int)Math.Round(LedController.LedCount / 2.0))
                {
                    leds.SetPixel(ref i, onOff == PowerOnOff.On ? onColor : Color.Black);
                }
                else if (side == LedSide.Right && i >= (int)Math.Round(LedController.LedCount / 2.0))
                {
                    leds.SetPixel(ref i, onOff == PowerOnOff.On ? onColor : Color.Black);
                }
                else
                {
                    if (LedController.ActualScreen != null)
                        leds.SetPixel(ref i, LedController.ActualScreen[i].LedColor);
                }
            }

            await LedController.SendPixels(leds.pixels).ConfigureAwait(false);

            StartColor = LedController.ActualScreen[0].LedColor;
            EndColor = LedController.ActualScreen[LedController.ActualScreen.Length - 1].LedColor;

        }

        /// <summary>Runs the led mode.</summary>
        /// <param name="mode">The mode.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task RunLedMode(LedMode mode)
        {
            await StopLedMode().ConfigureAwait(false);
            autoOffTmer.Stop();
            autoOffTmer.Start();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            Task.Run(async () =>
            {
                IsModeRunning = true;
                try
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
                }
                catch
                { }
                finally
                {
                    IsModeRunning = false;
                }
            }).ConfigureAwait(false);

#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            await Task.CompletedTask.ConfigureAwait(false);
        }

        /// <summary>Spins the LEDs.</summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        private async Task SpinLeds(CancellationToken cancellationToken)
        {
            var oldOffest = LedController.PixelOffset;

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    LedController.PixelOffset++;
                    await Task.Delay(RefreshSpeed, cancellationToken).ConfigureAwait(false);
                    if (LedController.PixelOffset >= LedController.LedCount)
                    {
                        LedController.PixelOffset = 0;
                    }
                    await LedController.Repaint().ConfigureAwait(false);
                }
            }
            catch
            {
            }
            finally
            {
                LedController.PixelOffset = oldOffest;
                await LedController.Repaint().ConfigureAwait(false);
            }
        }

        private async Task KnightRiderMode(CancellationToken cancellationToken)
        {
            await Task.Run(async () =>
            {
                int ledCount = LedController.LedCount;

                //20% of LEDs used for Knight rider mode
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

                    if (colorCount < knightCount)
                        colorCount++;

                    await LedController.SendPixels(leds.pixels).ConfigureAwait(false);
                    
                    startIndex++;

                    if (startIndex >= ledCount)
                    {
                        colorCount = 1;
                        startIndex = 0;
                        isClockwise = !isClockwise;
                    }

                    await Task.Delay(RefreshSpeed, cancellationToken).ConfigureAwait(false);
                }
            }).ConfigureAwait(false);
        }
    }
}