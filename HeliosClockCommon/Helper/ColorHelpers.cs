using HeliosClockCommon.Enumerations;
using HeliosClockCommon.ServiceCommons;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace HeliosClockCommon.Helper
{
    public static class ColorHelpers
    {
        public static Color CheckColor(int alpha, int red, int green, int blue)
        {
            if (alpha < 0) alpha = 0;
            if (alpha > 255) alpha = 255;

            if (red < 0) red = 0;
            if (red > 255) red = 255;

            if (green < 0) green = 0;
            if (green > 255) green = 255;

            if (blue < 0) blue = 0;
            if (blue > 255) blue = 255;

            return Color.FromArgb(alpha, red, green, blue);
        }

        public static Color CheckColor(int red, int green, int blue)
        {
            return CheckColor(255, red, green, blue);
        }

        public static void FadeHSLColor(APIServiceSettings settings, Action<Color> stepAction, CancellationToken token)
        {
            HSLColor startHlsColor = Color.Red;
            double step = 1;
            double actHue = 0.0;

            for (int i = 0; i < 360; i++)
            {
                actHue += step;
                var actColor = new HSLColor(actHue, startHlsColor.Saturation, startHlsColor.Luminosity);
                stepAction.Invoke(actColor);
                token.WaitHandle.WaitOne(settings.FadeSpeed);
            }
        }

        public static async Task<List<Color>> ColorGradient(Color startColor, Color endColor, int size, ColorInterpolationMode interpolationMode = ColorInterpolationMode.HueMode)
        {
            if (startColor.R == endColor.R && startColor.G == endColor.G && startColor.B == endColor.B)
            {
                return await Task.Run(() =>
                {
                    List<Color> colors = new List<Color>(size);
                    for (int i = 0; i < size; i++)
                    {
                        colors.Add(startColor);
                    }
                    return colors;
                });
            }

            switch (interpolationMode)
            {
                case ColorInterpolationMode.HueMode:
                    return await CalculateGradientHUE(endColor, startColor, size).ConfigureAwait(false);
                case ColorInterpolationMode.HueNearestMode:
                    return await CalculateGradientHUENearest(endColor, startColor, size).ConfigureAwait(false);
                case ColorInterpolationMode.Linear:
                    return CalculateGradientLinear(endColor, startColor, size);
                case ColorInterpolationMode.LinearCorrected:
                    return CalculateGradientLinearCorrected(endColor, startColor, size);
                default:
                    break;
            }
            return new List<Color>();
        }

        public static async Task<List<Color>> DimColor(Color color, int size, bool reverse = false)
        {
            return await Task.Run(() =>
            {
                HSLColor hslColor = color;

                double decrement = hslColor.Luminosity / ((double)size);
                double actualDecrement = 0;

                List<Color> colors = new List<Color>(size);

                for (int i = 0; i < size; i++)
                {
                    colors.Add(new HSLColor(hslColor.Hue, hslColor.Saturation, hslColor.Luminosity - actualDecrement));
                    actualDecrement += decrement;
                }

                if (reverse)
                    colors.Reverse();

                return colors;
            }).ConfigureAwait(false);
        }

        private static List<Color> CalculateGradientLinear(Color endColor, Color startColor, int size)
        {
            int rMax = startColor.R;
            int rMin = endColor.R;

            int gMax = startColor.G;
            int gMin = endColor.G;

            int bMax = startColor.B;
            int bMin = endColor.B;

            var colorList = new List<Color>();
            for (int i = 0; i < size; i++)
            {
                var rAverage = rMin + (int)((rMax - rMin) * i / size);
                var gAverage = gMin + (int)((gMax - gMin) * i / size);
                var bAverage = bMin + (int)((bMax - bMin) * i / size);
                colorList.Add(Color.FromArgb(rAverage, gAverage, bAverage));
            }
            return colorList;
        }

        private static List<Color> CalculateGradientLinearCorrected(Color startColor, Color endColor, int size)
        {
            List<Color> colors = new List<Color>(size);

            int discreteUnits = size;
            float correctionFactor = 0.0f;
            float correctionFactorStep = 1.0f / discreteUnits;

            for (int i = 0; i < discreteUnits; i++)
            {
                correctionFactor += correctionFactorStep;
                float red = (endColor.R - startColor.R) * correctionFactor + startColor.R;
                float green = (endColor.G - startColor.G) * correctionFactor + startColor.G;
                float blue = (endColor.B - startColor.B) * correctionFactor + startColor.B;
                colors.Add(Color.FromArgb(startColor.A, (int)red, (int)green, (int)blue));
            }
            return colors;
        }

        private static async Task<List<Color>> CalculateGradientHUE(Color startColor, Color endColor, int size)
        {
            return await Task<List<Color>>.Run(() =>
            {
                List<Color> colors = new List<Color>(size);
                HSLColor startHlsColor = startColor;
                HSLColor endHlsColor = endColor;
                int discreteUnits = size;

                for (int i = 0; i < discreteUnits; i++)
                {
                    var hueAverage = endHlsColor.Hue + (int)((startHlsColor.Hue - endHlsColor.Hue) * i / size);
                    var saturationAverage = endHlsColor.Saturation + (int)((startHlsColor.Saturation - endHlsColor.Saturation) * i / size);
                    var luminosityAverage = endHlsColor.Luminosity + (int)((startHlsColor.Luminosity - endHlsColor.Luminosity) * i / size);
                    colors.Add(new HSLColor(hueAverage, saturationAverage, luminosityAverage));
                }
                return colors;
            }).ConfigureAwait(false);
        }

        private static async Task<List<Color>> CalculateGradientHUENearest(Color startColor, Color endColor, int size)
        {
            return await Task.Run(() =>
            {
                bool considerNearest = true;
                List<Color> colors = new List<Color>(size);
                HSLColor startHlsColor = startColor;
                HSLColor endHlsColor = endColor;
                int discreteUnits = size;

                for (int i = 0; i < discreteUnits; i++)
                {
                    double hueAverage;
                    double saturationAverage;
                    double luminosityAverage;

                    if (considerNearest)
                    {
                        var deltaDirection = CalculateDeltaAndDirection(startHlsColor.Hue, endHlsColor.Hue, size);
                        hueAverage = endHlsColor.Hue + deltaDirection.direction * (int)(deltaDirection.delta * i);

                        deltaDirection = CalculateDeltaAndDirection(startHlsColor.Saturation, endHlsColor.Saturation, size);
                        saturationAverage = endHlsColor.Saturation + deltaDirection.direction * (int)(deltaDirection.delta * i);

                        deltaDirection = CalculateDeltaAndDirection(startHlsColor.Luminosity, endHlsColor.Luminosity, size);
                        luminosityAverage = endHlsColor.Luminosity + deltaDirection.direction * (int)(deltaDirection.delta * i);
                    }
                    else
                    {
                        hueAverage = endHlsColor.Hue + (int)((startHlsColor.Hue - endHlsColor.Hue) * i / size);
                        saturationAverage = endHlsColor.Saturation + (int)((startHlsColor.Saturation - endHlsColor.Saturation) * i / size);
                        luminosityAverage = endHlsColor.Luminosity + (int)((startHlsColor.Luminosity - endHlsColor.Luminosity) * i / size);
                    }
                    colors.Add(new HSLColor(hueAverage, saturationAverage, luminosityAverage));
                }
                return colors;
            }).ConfigureAwait(false);
        }

        private static (double delta, int direction) CalculateDeltaAndDirection(double start, double end, int size)
        {
            int direction = 1;
            if (start > end)
                direction = -1;

            double delta = Math.Abs((direction == 1 ? end - start : start - end) / size);

            return (delta, direction);

        }

        public static string HexConverter(Color c)
        {
            return "#" + c.A.ToString("X2")  + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");

        }

        public static String RGBConverter(System.Drawing.Color c)
        {
            return "ARGB(" + c.A.ToString() + "," + c.R.ToString() + "," + c.G.ToString() + "," + c.B.ToString() + ")";
        }

        public static Color FromHex(string hexColor)
        {
            int a = int.Parse(hexColor.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
            int r = int.Parse(hexColor.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
            int g = int.Parse(hexColor.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
            int b = int.Parse(hexColor.Substring(7, 2), System.Globalization.NumberStyles.HexNumber);

            return Color.FromArgb(a, r, g, b);
        }

        public static bool CompareColor(Color colorOne, Color colorTwo)
        {
            return colorOne.R == colorTwo.R && colorOne.G == colorTwo.G && colorOne.B == colorTwo.B && colorOne.A == colorTwo.A;
        }
    }
}