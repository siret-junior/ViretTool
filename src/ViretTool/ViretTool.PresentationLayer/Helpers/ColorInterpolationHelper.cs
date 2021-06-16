using System;
using System.Drawing;
using Colorspace;

namespace ViretTool.PresentationLayer.Helpers
{
    public class ColorInterpolationHelper
    {
        /// <summary>
        /// Interpolates color based in HSV color space.
        /// </summary>
        /// <param name="colorFrom">Color corresponding to interpolation 0.</param>
        /// <param name="colorTo">Color corresponding to interpolation 1.</param>
        /// <param name="interpolation">Values 0 to 1.</param>
        /// <param name="useShortRotation">Whether to rotate in the shorter or the longer path between two hue values.</param>
        /// <returns>Interpolated color.</returns>
        public static Color InterpolateColorHSV(Color colorFrom, Color colorTo, double interpolation, bool useShortRotation = true)
        {
            ColorRGB rgbFrom = new ColorRGB(colorFrom.R / 255.0, colorFrom.G / 255.0, colorFrom.B / 255.0);
            ColorRGB rgbTo = new ColorRGB(colorTo.R / 255.0, colorTo.G / 255.0, colorTo.B / 255.0);

            ColorHSV hsvFrom = new ColorHSV(rgbFrom);
            ColorHSV hsvTo = new ColorHSV(rgbTo);

            (hsvFrom, hsvTo) = FixDesaturatedColors(hsvFrom, hsvTo);

            double resultHue = InterpolateHue(interpolation, hsvFrom, hsvTo, useShortRotation);
            double resultSaturation = InterpolateSaturation(interpolation, hsvFrom, hsvTo);
            double resultValue = InterpolateValue(interpolation, hsvFrom, hsvTo);

            ColorHSV hsvResult = new ColorHSV(resultHue, resultSaturation, resultValue);
            ColorRGB rgbResult = new ColorRGB(hsvResult);

            return Color.FromArgb((int)(rgbResult.R * 255), (int)(rgbResult.G * 255), (int)(rgbResult.B * 255));
        }

        public static System.Windows.Media.Color InterpolateColorHSV(double interpolation, bool useShortRotation = false)
        {
            interpolation = Math.Min(interpolation, 1.0);
            interpolation = Math.Max(interpolation, 0);

            Color colorFrom = Color.Lime;
            Color colorTo = Color.Red;
            ColorRGB rgbFrom = new ColorRGB(colorFrom.R / 255.0, colorFrom.G / 255.0, colorFrom.B / 255.0);
            ColorRGB rgbTo = new ColorRGB(colorTo.R / 255.0, colorTo.G / 255.0, colorTo.B / 255.0);

            ColorHSV hsvFrom = new ColorHSV(rgbFrom);
            ColorHSV hsvTo = new ColorHSV(rgbTo);

            (hsvFrom, hsvTo) = FixDesaturatedColors(hsvFrom, hsvTo);

            double resultHue = InterpolateHue(interpolation, hsvFrom, hsvTo, useShortRotation);
            double resultSaturation = InterpolateSaturation(interpolation, hsvFrom, hsvTo);
            double resultValue = InterpolateValue(interpolation, hsvFrom, hsvTo);

            ColorHSV hsvResult = new ColorHSV(resultHue, resultSaturation, resultValue);
            ColorRGB rgbResult = new ColorRGB(hsvResult);

            return System.Windows.Media.Color.FromRgb((byte)(rgbResult.R * 255), (byte)(rgbResult.G * 255), (byte)(rgbResult.B * 255));
        }

        private static (ColorHSV hsvFrom, ColorHSV hsvTo) FixDesaturatedColors(ColorHSV hsvFrom, ColorHSV hsvTo)
        {
            // both desaturated
            if (double.IsNaN(hsvFrom.H) && double.IsNaN(hsvTo.H))
            {
                return (new ColorHSV(0, 0, hsvFrom.V), new ColorHSV(0, 0, hsvTo.V));
            }

            // one desaturated, copy H value from the other
            else if (double.IsNaN(hsvFrom.H))
            {
                return (new ColorHSV(hsvTo.H, 0, hsvFrom.V), hsvTo);
            }
            else if (double.IsNaN(hsvTo.H))
            {
                return (hsvFrom, new ColorHSV(hsvFrom.H, 0, hsvTo.V));
            }

            // no fix required
            else
            {
                return (hsvFrom, hsvTo);
            }
        }

        private static double InterpolateHue(double interpolation, ColorHSV hsvFrom, ColorHSV hsvTo, bool useShortRotation)
        {
            double hueDifferenceShort = Math.Abs(hsvTo.H - hsvFrom.H);
            double differenceDirection = Math.Sign(hsvTo.H - hsvFrom.H);
            if (hueDifferenceShort >= 0.5)
            {
                hueDifferenceShort = 1 - hueDifferenceShort;
                differenceDirection *= -1;
            }
            double hueDifferenceLong = 1 - hueDifferenceShort;

            double hueDifference = useShortRotation ? hueDifferenceShort : hueDifferenceLong;
            differenceDirection = useShortRotation ? differenceDirection : differenceDirection * -1;

            // interpolate
            double resultHue = hsvFrom.H + (hueDifference * interpolation) * differenceDirection;

            // clip if needed
            if (resultHue > 1)
            {
                resultHue -= 1;
            }
            if (resultHue < 0)
            {
                resultHue += 1;
            }

            return resultHue;
        }

        private static double InterpolateSaturation(double interpolation, ColorHSV hsvFrom, ColorHSV hsvTo)
        {
            double saturationDifference = hsvTo.S - hsvFrom.S;
            double resultSaturation = hsvFrom.S + (saturationDifference * interpolation);
            return resultSaturation;
        }

        private static double InterpolateValue(double interpolation, ColorHSV hsvFrom, ColorHSV hsvTo)
        {
            double valueDifference = hsvTo.V - hsvFrom.V;
            double resultValue = hsvFrom.V + (valueDifference * interpolation);
            return resultValue;
        }
    }
}
