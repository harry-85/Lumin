using HeliosClockAPIStandard.Enumeration;
using HeliosClockCommon.Defaults;
using HeliosClockCommon.Enumerations;
using HeliosClockCommon.Interfaces;
using HeliosClockCommon.ServiceOptions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace HeliosClockAPIStandard.GPIOListeners
{
    public partial class GPIOService : IHostedService
    {
        private IHeliosManager heliosManager;
        private ILogger<GPIOService> logger;
        private GpioController gpioController;

        private Stopwatch stopwatchLeft;
        private Stopwatch stopwatchRight;

        private int touchCound = 0;

        private bool isOn = false;
        private bool isLeftOn = false;
        private bool isRightOn = false;

        PinValue pinLeftOld = PinValue.High;
        PinValue pinRightOld = PinValue.High;

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

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                gpioController = new GpioController();
            }
            catch (Exception ex)
            {
                logger.LogError("Error initializing GPIO Service. Message: {0}", ex.Message);
                return;
            }

            logger.LogInformation("Started GPIO Watch ...");

            try
            {
                gpioController.OpenPin((int)GpioInputPin.LeftSide, PinMode.Input);
                gpioController.OpenPin((int)GpioInputPin.RightSide, PinMode.Input);

                while (!cancellationToken.IsCancellationRequested)
                {
                    isOn = heliosManager.IsRunning;
                    await ExecuteTouchWatcher(LedSide.Left, stopwatchLeft, cancellationToken).ConfigureAwait(false);
                    await ExecuteTouchWatcher(LedSide.Right, stopwatchRight, cancellationToken).ConfigureAwait(false);
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

        /// <summary>Executes the touch watcher.</summary>
        /// <param name="side">The side.</param>
        /// <param name="stopwatch">The stopwatch.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        private async Task ExecuteTouchWatcher(LedSide side, Stopwatch stopwatch, CancellationToken cancellationToken)
        {
            await Task.Run(async () =>
            {
                long elapsed = 0;

                var input = gpioController.Read((side == LedSide.Left ? (int)GpioInputPin.LeftSide : (int)GpioInputPin.RightSide));

                //Check for flip
                CheckTouchFlip(side, input);

                if (input == PinValue.High)
                {
                    //logger.LogInformation("Switch {0} down ...", side);
                    if (!stopwatch.IsRunning)
                    {
                        stopwatch.Start();
                        touchCound = 1;
                    }
                }

                elapsed = stopwatch.ElapsedMilliseconds;

                //If touch is released
                if (input == PinValue.Low && stopwatch.IsRunning)
                {
                    //logger.LogInformation("Switch {0} High ...", side);
                    stopwatch.Stop();
                    elapsed = stopwatch.ElapsedMilliseconds;
                    logger.LogInformation("Touch duration: {0} ms ...", elapsed);
                    stopwatch.Reset();
                }

                //If touch is pressed continiously
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

                    if (elapsed >= TouchDefaultValues.MinLongPressDuration && (TouchDefaultValues.MinLongPressDuration % elapsed) == 0)
                    {
                        logger.LogInformation("First long press Power: {} ...", isOn ? PowerOnOff.Off : PowerOnOff.On);

                        side = LedSide.Full;
                        await heliosManager.SetOnOff(isOn ? PowerOnOff.Off : PowerOnOff.On, side).ConfigureAwait(false);
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
                    }
                    return;
                }

                if (elapsed >= TouchDefaultValues.MinShortPressDuration && elapsed < TouchDefaultValues.MinLongPressDuration)
                {
                    if (side == LedSide.Left)
                    {
                        await heliosManager.SetOnOff(isLeftOn ? PowerOnOff.Off : PowerOnOff.On, side).ConfigureAwait(false);
                        isLeftOn = !isLeftOn;
                    }
                    if (side == LedSide.Right)
                    {
                        await heliosManager.SetOnOff(isRightOn ? PowerOnOff.Off : PowerOnOff.On, side).ConfigureAwait(false);
                        isRightOn = !isRightOn;
                    }
                }
            }).ConfigureAwait(false);
        }

        /// <summary>Triggered when the application host is performing a graceful shutdown.</summary>
        /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
        public Task StopAsync(CancellationToken cancellationToken)
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
        private bool CheckTouchFlip(LedSide side, PinValue input)
        {
            if (side == LedSide.Left)
            {
                if (pinLeftOld != input)
                {
                    logger.LogInformation("Switch {0} toggeled. Value: {1} ...", side, input);
                    pinLeftOld = input;
                    return true;
                }
            }
            if (side == LedSide.Right)
            {
                if (pinRightOld != input)
                {
                    logger.LogInformation("Switch {0} toggeled. Value: {1} ...", side, input);
                    pinRightOld = input;
                    return true;
                }
            }
            return false;
        }
    }
}
