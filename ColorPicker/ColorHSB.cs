using System;
using System.Windows.Media;

namespace ColorPicker
{
    public class ColorHSB
    {
        private double hue;
        private double saturation;
        private double brightness;

        public double Hue
        {
            get { return hue; }

            set
            {
                hue = value;

                // Make sure hue is between 0 and 359
                if (hue < 0)
                    hue = 0;
                if (hue >= 360)
                    hue = 0;
                if (Double.IsNaN(hue))
                    hue = 0;
            }
        }

        public double Saturation
        {
            get { return saturation; }
            set { saturation = value; }
        }

        public double Brightness
        {
            get { return brightness; }
            set { brightness = value; }
        }

        public ColorHSB()
        {

        }

        public ColorHSB(double h, double s, double b)
        {
            Hue = h;
            Saturation = s;
            Brightness = b;
        }

        /// <summary>
        /// Convert RGB color to HSB color.
        /// </summary>
        /// <param name="color">RGB color</param>
        /// <returns>HSB color</returns>
        public static ColorHSB RGBtoHSB(Color color)
        {
            double R = ((double)color.R / 255.0);
            double G = ((double)color.G / 255.0);
            double B = ((double)color.B / 255.0);

            double max = Math.Max(R, Math.Max(G, B));
            double min = Math.Min(R, Math.Min(G, B));

            double hue = 0.0;

            // Calculate the hue from RGB.
            if (max == R && G >= B)
            {
                hue = 60 * (G - B) / (max - min);
            }
            else if (max == R && G < B)
            {
                hue = 60 * (G - B) / (max - min) + 360;
            }
            else if (max == G)
            {
                hue = 60 * (B - R) / (max - min) + 120;
            }
            else if (max == B)
            {
                hue = 60 * (R - G) / (max - min) + 240;
            }

            double saturation = (max == 0) ? 0.0 : (1.0 - (min / max));

            return new ColorHSB(hue, saturation, max);
        }

        /// <summary>
        /// Converts HSB color to RGB color.
        /// </summary>
        /// <param name="colorHSB">HSB Color</param>
        /// <returns>RGB color</returns>
        public static Color HSBtoRGB(ColorHSB colorHSB)
        {
            double hue = colorHSB.hue;
            double saturation = colorHSB.saturation;
            double brightness = colorHSB.brightness;

            double R = 0;
            double G = 0;
            double B = 0;

            if (saturation == 0)
            {
                R = G = B = brightness;
            }
            else
            {
                // Calculate the color sector.
                double sectorPos = hue / 60.0;
                int sectorNumber = (int)(Math.Floor(sectorPos));
                // Get the fractional.
                double fractionalSector = sectorPos - sectorNumber;

                // Calculate the three axes of color.
                double p = brightness * (1.0 - saturation);
                double q = brightness * (1.0 - (saturation * fractionalSector));
                double t = brightness * (1.0 - (saturation * (1 - fractionalSector)));

                // Assign fractional colors to RGB based on the sector the angle.
                switch (sectorNumber)
                {
                    case 0:
                        R = brightness;
                        G = t;
                        B = p;
                        break;
                    case 1:
                        R = q;
                        G = brightness;
                        B = p;
                        break;
                    case 2:
                        R = p;
                        G = brightness;
                        B = t;
                        break;
                    case 3:
                        R = p;
                        G = q;
                        B = brightness;
                        break;
                    case 4:
                        R = t;
                        G = p;
                        B = brightness;
                        break;
                    case 5:
                        R = brightness;
                        G = p;
                        B = q;
                        break;
                }
            }

            Color color = Color.FromRgb((byte)Double.Parse(String.Format("{0:0.00}", R * 255.0)), (byte)Double.Parse(String.Format("{0:0.00}", G * 255.0)), (byte)Double.Parse(String.Format("{0:0.00}", B * 255.0)));

            return color;
        }
    }
}
