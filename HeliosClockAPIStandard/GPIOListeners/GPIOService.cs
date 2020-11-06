﻿using HeliosClockAPIStandard.Enumeration;
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

        private const int MinShortPressDuration = 200; // ms
        private const int MinLongPressDuration = 1500; // ms

        private Stopwatch stopwatchLeft;
        private Stopwatch stopwatchRight;

        private bool isOn = false;

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

            //Run first half in seperate task to simulate parallel checking of sides
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            //  Task.Run(async () => await ExecuteTouchWatcher(LedSide.Left, stopwatchLeft, cancellationToken).ConfigureAwait(false));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            //Wait here until second half is finished, first half runs in seperate task to simulate parallel checking of sides

            logger.LogInformation("Started GPIO Watch ...");

            try
            {
                gpioController.OpenPin((int)GpioInputPin.LeftSide, PinMode.Input);
                gpioController.OpenPin((int)GpioInputPin.RightSide, PinMode.Input);

                while (!cancellationToken.IsCancellationRequested)
                {
                    //await Task.Delay(50).ConfigureAwait(false);
                    await ExecuteTouchWatcher(LedSide.Left, stopwatchLeft, cancellationToken).ConfigureAwait(false);
                    //await Task.Delay(50).ConfigureAwait(false);
                    await ExecuteTouchWatcher(LedSide.Right, stopwatchRight, cancellationToken).ConfigureAwait(false);
                }

                gpioController.ClosePin((int)GpioInputPin.LeftSide);
               // gpioController.ClosePin((int)GpioInputPin.RightSide);
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
                var input = gpioController.Read((side == LedSide.Left ? (int)GpioInputPin.LeftSide : (int)GpioInputPin.RightSide));

                GpioPressDuration duration = GpioPressDuration.None;

                if (input == PinValue.Low)
                {
                    logger.LogInformation("Switch {0} down ...", side);
                    if (!stopwatch.IsRunning)
                        stopwatch.Start();
                    return;
                }

                if (!stopwatch.IsRunning)
                    return;

                if (input == PinValue.High)
                {
                    stopwatch.Stop();

                    if (stopwatch.ElapsedMilliseconds >= MinShortPressDuration)
                    {
                        duration = GpioPressDuration.Short;
                    }
                    if (stopwatch.ElapsedMilliseconds >= MinLongPressDuration)
                    {
                        duration = GpioPressDuration.Long;
                    }
                    stopwatch.Reset();
                }

                if (duration == GpioPressDuration.None)
                    return;
                if (duration == GpioPressDuration.Long)
                    side = LedSide.Full;

                await heliosManager.SetOnOff(isOn ? PowerOnOff.Off : PowerOnOff.On, side).ConfigureAwait(false);

                    //Flip between on off
                    isOn = !isOn;

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
    }
}
