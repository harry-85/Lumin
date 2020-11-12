using HeliosClockAPIStandard.Enumeration;
using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace HeliosClockAPIStandard.Services
{
    public class GpioPinWatchItem
    {
        public GpioInputPin Pin { get; }
        public event EventHandler<GpioTriggeredEventArgs> PinTriggeredEvent;
        
        private CancellationTokenSource tokenSource;
        private CancellationToken token;
        private GpioController gpioController;
        private Stopwatch touchDurationWatch;

        private Task fallingTask;
        private Task risingTask;

        private Stopwatch stopwatch;

        public GpioPinWatchItem(GpioController gpioController, GpioInputPin pin)
        {
            this.Pin = pin;
            this.gpioController = gpioController;

            
            tokenSource = new CancellationTokenSource();
            token = tokenSource.Token;
            touchDurationWatch = new Stopwatch();
        }

        public void StartWatchingPinAsync()
        {
            stopwatch = new Stopwatch();
            stopwatch.Start();
            gpioController.OpenPin((int)Pin, PinMode.Input);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            //Watch fallign edge
            fallingTask = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    Console.WriteLine("Falling Edge Detection Started .........................................................................................");
                    var valueTask = await gpioController.WaitForEventAsync((int)Pin, PinEventTypes.Falling, token).ConfigureAwait(false);

                    var duration = touchDurationWatch.ElapsedMilliseconds;
                    PinTriggeredEvent?.Invoke(this, new GpioTriggeredEventArgs(PinValue.Low, Pin, duration));

                    Console.WriteLine("Falling Edge Detected ... {0} .......................................................................................", stopwatch.ElapsedMilliseconds);

                    touchDurationWatch.Stop();
                    touchDurationWatch.Reset();
                    touchDurationWatch.Start();
                }
            });

            //Watch rising edge
            risingTask = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    Console.WriteLine("Rising Edge Detection Started .........................................................................................");

                    var valueTask = await gpioController.WaitForEventAsync((int)Pin, PinEventTypes.Rising, token).ConfigureAwait(false);

                    Console.WriteLine("Rising Edge Detected ...{0} .......................................................................................", stopwatch.ElapsedMilliseconds);

                    var duration = touchDurationWatch.ElapsedMilliseconds;
                    PinTriggeredEvent?.Invoke(this, new GpioTriggeredEventArgs(PinValue.High, Pin, duration));
                    touchDurationWatch.Stop();
                    touchDurationWatch.Reset();
                    touchDurationWatch.Start();
                }
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public Task StopAsync()
        {
            tokenSource.Cancel();
            return Task.CompletedTask;            
        }
    }
}
