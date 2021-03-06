﻿using HeliosClockAPIStandard.Enumeration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HeliosClockAPIStandard.Services
{
    public class GpioService : BackgroundService
    {
        private readonly GpioController gpioController;

        private readonly List<GpioPinWatchItem> pinWatchers;

        public GpioService()
        {
            pinWatchers = new List<GpioPinWatchItem>();
            this.gpioController = new GpioController();
        }

        public void AddWatchPin(GpioInputPin pin)
        {
            var newItem = new GpioPinWatchItem(gpioController, pin);
            newItem.PinTriggeredEvent += NewItem_PinTriggeredEvent;
            newItem.StartWatchingPin();
            pinWatchers.Add(newItem);
        }

        public void RemoveWatchPin(GpioInputPin pin)
        {
            var item = pinWatchers.Where(s => s.Pin == pin).ToList();
            if (item.Count == 0)
                return;

            item[0].PinTriggeredEvent -= NewItem_PinTriggeredEvent;
            pinWatchers.Remove(item[0]);
        }

        private void NewItem_PinTriggeredEvent(object sender, GpioTriggeredEventArgs e)
        {
            if (e.PinValue == PinValue.Low)
            {
                
            }
        }

        ////private async Task TouchedLow()
        ////{
           
        ////}

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }
}
