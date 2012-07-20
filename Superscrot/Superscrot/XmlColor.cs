using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Superscrot
{
    public static class XmlColor
    {
        public enum ColorFormat
        {
            NamedColor,
            ARGBColor
        }

        public static string SerializeColor(Color color)
        {
            try
            {
                if (color.IsNamedColor)
                {
                    return string.Format("{0}:{1}", ColorFormat.NamedColor, color.Name);
                }
                else
                {
                    return string.Format("{0}:{1}:{2}:{3}:{4}", ColorFormat.ARGBColor, color.A, color.R, color.G, color.B);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow; Console.WriteLine("Can't serialize color: " + ex.Message); Console.ResetColor();
                return "NamedColor:Transparent";
            }
        }

        public static Color DeserializeColor(string color)
        {
            try
            {
                byte a, r, g, b;
                string[] pieces = color.Split(new char[] { ':' });
                ColorFormat colorType = (ColorFormat)Enum.Parse(typeof(ColorFormat), pieces[0], true);

                switch (colorType)
                {
                    case ColorFormat.NamedColor:
                        return Color.FromName(pieces[1]);
                    case ColorFormat.ARGBColor:
                        a = byte.Parse(pieces[1]);
                        r = byte.Parse(pieces[2]);
                        g = byte.Parse(pieces[3]);
                        b = byte.Parse(pieces[4]);
                        return Color.FromArgb(a, r, g, b);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow; Console.WriteLine("Can't deserialize color: " + ex.Message); Console.ResetColor();
            }
            return Color.Empty;
        }
    }
}
