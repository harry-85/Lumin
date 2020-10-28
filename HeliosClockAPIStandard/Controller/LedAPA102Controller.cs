using System;
using System.Collections.Generic;
using System.Device.Spi;
using HeliosClockCommon.Helper;
using HeliosClockCommon.LedCommon;

namespace HeliosClockAPIStandard.Controller
{
    public class LedAPA102Controller : AbstractLedController
    {
        // <summary>Line the SPI library uses to signal chip select.</summary>
        /// <remarks>The APA102 doesn't actually support this line, so safe to ignore this.</remarks>
        private const int SpiChipSelectLine = 0;

        /// <summary>Object for communicating with the LED strip.</summary>
        private readonly SpiDevice spiDevice;

        /// <summary>Gets a value representing the count of pixels in the LED strip.</summary>
        public byte[] EndFrame { get; private set; }

        /// <summary>The start frame</summary>
        private readonly byte[] startFrame = { 0, 0, 0, 0 };

        /// <summary>The connection settings.</summary>
        private readonly SpiConnectionSettings connectionSettings;

        /// <summary>Constructor.</summary>
        /// <param name="numLeds">Number of LEDs in the strip</param>
        public LedAPA102Controller() : base()
        {
            // The actual logic here is that we need to send a zero to be sent for every 16 pixels in the strip, to signify the end
            // of the color data and reset the addressing.
            int endFrameSize = (LedCount + 14) / 16;

            // By initializing an int array of that specific length, it gets initialized with ints of default value (0).  :)
            this.EndFrame = new byte[endFrameSize];
            
            ClockSpeedHz = 10_000_000;

            connectionSettings = new SpiConnectionSettings(0, SpiChipSelectLine)
            {
                // SPI clock speed in Hz.  Super brief testing worked fine anywhere as low as 40khz (below about 200khz was noticeably slow), all the way
                // up to 16mhz (above 16mhz started to get corrupted data towards the end of the strip).  10mhz is probably a good baseline value to use
                // unless you run in to problems.  :)
                ClockFrequency = ClockSpeedHz,
                // APA102/DotStar uses SPI mode 3, CPOL = 1 (clock is high when inactive), CPHA = 1 (data is valid on the clock's trailing edge)
                Mode = SpiMode.Mode3,
                DataBitLength = 8
            };

            spiDevice = GetSpiDevice();
        }

        /// <summary>Sends the pixels.</summary>
        /// <param name="pixels">The pixels.</param>
        public override void SendPixels(LedPixel[] pixels)
        {
            List<byte> spiDataBytes = new List<byte>();
            spiDataBytes.AddRange(startFrame);

            for (int i = 0; i < pixels.Length; i++)
            {
                int realIndex = PixelHelper.CalculateRealPixel(i, LedCount, PixelOffset);

                if (pixels[realIndex] == null) pixels[realIndex] = new LedPixel();
                // Global brightness.  Not implemented currently.  0xE0 (binary 11100000) specifies the beginning of the pixel's
                // color data.  0x1F (binary 00011111) specifies the global brightness.  If you want to actually use this functionality
                // comment out this line and uncomment the next one.  Then the pixel's RGB value will get scaled based on the alpha
                // channel value from the Color.
                //spiDataBytes.Add(0xE0 | 0x1F);
                spiDataBytes.Add((byte)(0xE0 | (byte)(pixels[realIndex].LedColor.A >> 3)));

                // APA102/DotStar leds take the color data in Blue, Green, Red order.  Weirdly, according to the spec these are supposed
                // to take a 0-255 value for R/G/B.  However, on the ones I have they only seem to take 0-126.  Specifying 127-255 doesn't
                // break anything, but seems to show the same exact value 0-126 would have (i.e. 127 is 0 brightness, 255 is full brightness).
                // Discarding the lowest bit from each to make the value fit in 0-126.
                spiDataBytes.Add((byte)(pixels[realIndex].LedColor.B >> 1));
                spiDataBytes.Add((byte)(pixels[realIndex].LedColor.G >> 1));
                spiDataBytes.Add((byte)(pixels[realIndex].LedColor.R >> 1));
            }

            spiDataBytes.AddRange(this.EndFrame);
            try
            {
                spiDevice.Write(spiDataBytes.ToArray());
                ActualScreen = pixels;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>Gets the SpiDevice handle</summary>
        /// <returns>Task of type SpiDevice, whose result will be the SpiDevice requested if successful</returns>
        private SpiDevice GetSpiDevice()
        {
            var device = SpiDevice.Create(connectionSettings);
            return device;
        }

        /// <summary>Sets the screen colors.</summary>
        /// <param name="pixels">The pixels.</param>
        public override void SetScreenColors(LedScreen pixels) => SendPixels(pixels.pixels);

        /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                spiDevice?.Dispose();
            }

            disposed = true;
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
