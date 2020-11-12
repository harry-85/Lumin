using HeliosClockAPIStandard.Enumeration;
using HeliosClockAPIStandard.Services;
using HeliosClockCommon.Defaults;
using HeliosClockCommon.Enumerations;
using HeliosClockCommon.Interfaces;
using HeliosClockCommon.ServiceOptions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Device.Gpio;
using System.Device.Gpio.Drivers;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace HeliosClockAPIStandard.GPIOListeners
{
    public partial class GPIOService : BackgroundService
    {
        private readonly IHeliosManager heliosManager;
        private readonly ILogger<GPIOService> logger;
        private GpioController gpioController;

        private readonly Stopwatch stopwatchLeft;
        private readonly Stopwatch stopwatchRight;

        private int touchCound = 0;

        private bool isOn = false;
        private bool isLeftOn = false;
        private bool isRightOn = false;
        private bool firstLongToucgOccurred = false;

        PinValue pinLeftOld = PinValue.Low;
        PinValue pinRightOld = PinValue.Low;

        private readonly Color onOffColor = DefaultColors.WarmWhite;

        public LedSide side;

        /// <summary>Initializes a new instance of the <see cref="GPIOService"/> class.</summary>
        /// <param name="manager">The manager.</param>
        public GPIOService(ILogger<GPIOService> logger, IHeliosManager manager)
        {
            logger.LogInformation("Started GPIO Watch initializing ...");
            this.logger = logger;
            heliosManager = manager;
            stopwatchLeft = new Stopwatch();
            stopwatchRight = new Stopwatch();
            logger.LogInformation("Started GPIO Watch initialied ...");
        }

        /// <summary>
        /// This method is called when the <see cref="T:Microsoft.Extensions.Hosting.IHostedService" /> starts. The implementation should return a task that represents
        /// the lifetime of the long running operation(s) being performed.
        /// </summary>
        /// <param name="stoppingToken">Triggered when <see cref="M:Microsoft.Extensions.Hosting.IHostedService.StopAsync(System.Threading.CancellationToken)" /> is called.</param>
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                gpioController = new GpioController(PinNumberingScheme.Logical, new RaspberryPi3Driver());
            }
            catch (Exception ex)
            {
                logger.LogError("Error initializing GPIO Service. Message: {0}", ex.Message);
                return;
            }

            logger.LogInformation("Started GPIO Watch ...");


            try
            {
                // await ExecuteTouchCheckAsync(stoppingToken).ConfigureAwait(false);

                gpioController.OpenPin((int)GpioInputPin.LeftSide, PinMode.InputPullDown);
                gpioController.OpenPin((int)GpioInputPin.RightSide, PinMode.InputPullDown);

                var side = LedSide.Left;

                while (!stoppingToken.IsCancellationRequested)
                {
                    isOn = heliosManager.IsRunning;

                    var input = gpioController.Read(side == LedSide.Left ? (int)GpioInputPin.LeftSide : (int)GpioInputPin.RightSide);
                    await ExecuteTouchWatcher(side, input, side == LedSide.Left ? stopwatchLeft : stopwatchRight, stoppingToken).ConfigureAwait(false);
                    side = side == LedSide.Left ? LedSide.Right : LedSide.Left;
                    await Task.Delay(5).ConfigureAwait(false);
                }

                gpioController.ClosePin((int)GpioInputPin.LeftSide);
                gpioController.ClosePin((int)GpioInputPin.RightSide);
            }
            catch (Exception ex)
            {
                logger.LogError("Error Read Pin GPIO Service. Message: {0}", ex.Message);
            }

            logger.LogInformation("Stopped GPIO Watch ...");
        }


        /// <summary>Executes the touch watcher. Checks if short or long press. On or Off.</summary>
        /// <param name="side">The side.</param>
        /// <param name="stopwatch">The stopwatch.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        private async Task ExecuteTouchWatcher(LedSide side, PinValue input, Stopwatch stopwatch, CancellationToken cancellationToken)
        {
            await Task.Run(async () =>
            {
                long elapsed = 0;

                //Check for flip
                CheckTouchFlipSetOld(side, input);

                if (input == PinValue.High)
                {
                    if (!stopwatch.IsRunning)
                    {
                        stopwatch.Start();
                        touchCound = 1;
                        firstLongToucgOccurred = false;
                    }
                }

                elapsed = stopwatch.ElapsedMilliseconds;

                //If touch is released
                if (input == PinValue.Low && stopwatch.IsRunning)
                {
                    stopwatch.Stop();
                    elapsed = stopwatch.ElapsedMilliseconds;
                    logger.LogInformation("Touch duration: {0} ms ...", elapsed);
                    stopwatch.Reset();
                }

                //If touch is pressed continuously
                if (stopwatch.IsRunning)
                {
                    //Random Color Full
                    if ((elapsed - (touchCound * TouchDefaultValues.MinLongPressDuration)) >= TouchDefaultValues.MinLongPressDuration)
                    {
                        logger.LogInformation("Touch random color ...");

                        touchCound++;

                        await heliosManager.SetRandomColor(cancellationToken).ConfigureAwait(false);
                        return;
                    }

                    //First long press, only white / black in full mode
                    if (elapsed >= TouchDefaultValues.MinLongPressDuration && !firstLongToucgOccurred)
                    {
                        logger.LogInformation("First long press. Mode: {} ...", isOn ? PowerOnOff.Off : PowerOnOff.On);

                        side = LedSide.Full;
                        await heliosManager.SetOnOff(isOn ? PowerOnOff.Off : PowerOnOff.On, side, onOffColor).ConfigureAwait(false);
                        //Flip between on off
                        isOn = !isOn;

                        if (isOn)
                        {
                            isLeftOn = true;
                            isRightOn = true;
                        }
                        else
                        {
                            isLeftOn = false;
                            isRightOn = false;
                        }

                        firstLongToucgOccurred = true;
                    }
                    return;
                }

                if (elapsed >= TouchDefaultValues.MinShortPressDuration && elapsed < TouchDefaultValues.MinLongPressDuration)
                {
                    if (side == LedSide.Left)
                    {
                        await heliosManager.SetOnOff(isLeftOn ? PowerOnOff.Off : PowerOnOff.On, side, onOffColor).ConfigureAwait(false);
                        isLeftOn = !isLeftOn;
                    }
                    if (side == LedSide.Right)
                    {
                        await heliosManager.SetOnOff(isRightOn ? PowerOnOff.Off : PowerOnOff.On, side, onOffColor).ConfigureAwait(false);
                        isRightOn = !isRightOn;
                    }
                }
            }).ConfigureAwait(false);
        }

        /// <summary>Triggered when the application host is performing a graceful shutdown.</summary>
        /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (gpioController.IsPinOpen((int)GpioInputPin.LeftSide))
                    gpioController.ClosePin((int)GpioInputPin.LeftSide);

                if (gpioController.IsPinOpen((int)GpioInputPin.RightSide))
                    gpioController.ClosePin((int)GpioInputPin.RightSide);

                gpioController.Dispose();
            }
            catch (Exception ex)
            {
                logger.LogError("Error in GPIO Service. Message: {0}", ex.Message);
            }
            return Task.CompletedTask;
        }

        /// <summary>Checks the touch flip.</summary>
        /// <param name="side">The side.</param>
        /// <param name="input">The input.</param>
        private bool CheckTouchFlipSetOld(LedSide side, PinValue input)
        {
            if (side == LedSide.Left)
            {
                if (pinLeftOld != input)
                {
                    logger.LogInformation("Switch {0} toggled. Value: {1} ...", side, input);
                    pinLeftOld = input;
                    return true;
                }
            }
            if (side == LedSide.Right)
            {
                if (pinRightOld != input)
                {
                    logger.LogInformation("Switch {0} toggled. Value: {1} ...", side, input);
                    pinRightOld = input;
                    return true;
                }
            }
            return false;
        }
    }
}
