using System;
using System.Drawing;

namespace HeliosClockCommon.LedCommon
{ 
    public class LedScreen
    {
        public LedPixel[] pixels;
        private readonly ILedController controller;
        
        public LedScreen(ILedController controller)
        {
            this.controller = controller;

            if (controller == null)
                throw new Exception("Led Controller musst not be Null in LedScreen");

            pixels = new LedPixel[controller.LedCount];
        }

        public void SetPixel(ref int index, Color color)
        {
            CheckIndexOverflw(ref index);

            if (pixels[index] == null)
                pixels[index] = new LedPixel();

            pixels[index].LedColor = color;
            pixels[index].Index = index;
        }

        public void SetColorToScreen(Color color)
        {
            for (int i = 0; i < controller.LedCount; i++)
            {
                pixels[i] = new LedPixel
                {
                    LedColor = color
                };
            }
        }

        private void CheckIndexOverflw(ref int index)
        {
            if (index >= controller.LedCount)
                index = 0;
        }
    }

}
