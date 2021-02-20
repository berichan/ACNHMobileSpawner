using System;
using System.Globalization;

// C# Implementation of X11 Colors
namespace Beri.Drawing
{
    public class Color
    {
        public byte R, G, B, A;
        public Color(byte r, byte g, byte b, byte a)
        {
            R = r; G = g; B = b; A = a;
        }

        public string ToArgbH() => $"{A:X2}{R:X2}{G:X2}{B:X2}";
        public int ToArgb() => int.Parse(ToArgbH(), NumberStyles.HexNumber);

        public Color(int hex) => ColorFromHex($"{hex:X8}");

        public static Color FromArgb(int hex) => ColorFromHex($"{hex:X8}");

        public static Color FromArgb(byte r, byte g, byte b) => new Color(r, g, b, byte.MaxValue);
        public static Color FromArgb(byte a, byte r, byte g, byte b) => new Color(r, g, b, a);

        public static Color ColorFromHex(string hex)
        {
            if (hex.Length == 6)
                return new Color(
                byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber),
                byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber),
                byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber),
                255);
            else
                return new Color(
                byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber),
                byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber),
                byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber),
                byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber));
        }

        public static Color Transparent => ColorFromHex("00000000");
        public static Color Pink => ColorFromHex("FFC0CB");
        public static Color LightPink => ColorFromHex("FFB6C1");
        public static Color HotPink => ColorFromHex("FF69B4");
        public static Color DeepPink => ColorFromHex("FF1493");
        public static Color PaleVioletRed => ColorFromHex("DB7093");
        public static Color MediumVioletRed => ColorFromHex("C71585");
        public static Color LightSalmon => ColorFromHex("FFA07A");
        public static Color Salmon => ColorFromHex("FA8072");
        public static Color DarkSalmon => ColorFromHex("E9967A");
        public static Color LightCoral => ColorFromHex("F08080");
        public static Color IndianRed => ColorFromHex("CD5C5C");
        public static Color Crimson => ColorFromHex("DC143C");
        public static Color FireBrick => ColorFromHex("B22222");
        public static Color DarkRed => ColorFromHex("8B0000");
        public static Color Red => ColorFromHex("FF0000");
        public static Color OrangeRed => ColorFromHex("FF4500");
        public static Color Tomato => ColorFromHex("FF6347");
        public static Color Coral => ColorFromHex("FF7F50");
        public static Color DarkOrange => ColorFromHex("FF8C00");
        public static Color Orange => ColorFromHex("FFA500");
        public static Color Gold => ColorFromHex("FFD700");
        public static Color Yellow => ColorFromHex("FFFF00");
        public static Color LightYellow => ColorFromHex("FFFFE0");
        public static Color LemonChiffon => ColorFromHex("FFFACD");
        public static Color LightGoldenrodYellow => ColorFromHex("FAFAD2");
        public static Color PapayaWhip => ColorFromHex("FFEFD5");
        public static Color Moccasin => ColorFromHex("FFE4B5");
        public static Color PeachPuff => ColorFromHex("FFDAB9");
        public static Color PaleGoldenrod => ColorFromHex("EEE8AA");
        public static Color Khaki => ColorFromHex("F0E68C");
        public static Color DarkKhaki => ColorFromHex("BDB76B");
        public static Color Cornsilk => ColorFromHex("FFF8DC");
        public static Color BlanchedAlmond => ColorFromHex("FFEBCD");
        public static Color Bisque => ColorFromHex("FFE4C4");
        public static Color NavajoWhite => ColorFromHex("FFDEAD");
        public static Color Wheat => ColorFromHex("F5DEB3");
        public static Color BurlyWood => ColorFromHex("DEB887");
        public static Color Tan => ColorFromHex("D2B48C");
        public static Color RosyBrown => ColorFromHex("BC8F8F");
        public static Color SandyBrown => ColorFromHex("F4A460");
        public static Color Goldenrod => ColorFromHex("DAA520");
        public static Color DarkGoldenrod => ColorFromHex("B8860B");
        public static Color Peru => ColorFromHex("CD853F");
        public static Color Chocolate => ColorFromHex("D2691E");
        public static Color SaddleBrown => ColorFromHex("8B4513");
        public static Color Sienna => ColorFromHex("A0522D");
        public static Color Brown => ColorFromHex("A52A2A");
        public static Color Maroon => ColorFromHex("800000");
        public static Color DarkOliveGreen => ColorFromHex("556B2F");
        public static Color Olive => ColorFromHex("808000");
        public static Color OliveDrab => ColorFromHex("6B8E23");
        public static Color YellowGreen => ColorFromHex("9ACD32");
        public static Color LimeGreen => ColorFromHex("32CD32");
        public static Color Lime => ColorFromHex("00FF00");
        public static Color LawnGreen => ColorFromHex("7CFC00");
        public static Color Chartreuse => ColorFromHex("7FFF00");
        public static Color GreenYellow => ColorFromHex("ADFF2F");
        public static Color SpringGreen => ColorFromHex("00FF7F");
        public static Color MediumSpringGreen => ColorFromHex("00FA9A");
        public static Color LightGreen => ColorFromHex("90EE90");
        public static Color PaleGreen => ColorFromHex("98FB98");
        public static Color DarkSeaGreen => ColorFromHex("8FBC8F");
        public static Color MediumSeaGreen => ColorFromHex("3CB371");
        public static Color SeaGreen => ColorFromHex("2E8B57");
        public static Color ForestGreen => ColorFromHex("228B22");
        public static Color Green => ColorFromHex("008000");
        public static Color DarkGreen => ColorFromHex("006400");
        public static Color MediumAquamarine => ColorFromHex("66CDAA");
        public static Color Aqua => ColorFromHex("00FFFF");
        public static Color Cyan => ColorFromHex("00FFFF");
        public static Color LightCyan => ColorFromHex("E0FFFF");
        public static Color PaleTurquoise => ColorFromHex("AFEEEE");
        public static Color Aquamarine => ColorFromHex("7FFFD4");
        public static Color Turquoise => ColorFromHex("40E0D0");
        public static Color MediumTurquoise => ColorFromHex("48D1CC");
        public static Color DarkTurquoise => ColorFromHex("00CED1");
        public static Color LightSeaGreen => ColorFromHex("20B2AA");
        public static Color CadetBlue => ColorFromHex("5F9EA0");
        public static Color DarkCyan => ColorFromHex("008B8B");
        public static Color Teal => ColorFromHex("008080");
        public static Color LightSteelBlue => ColorFromHex("B0C4DE");
        public static Color PowderBlue => ColorFromHex("B0E0E6");
        public static Color LightBlue => ColorFromHex("ADD8E6");
        public static Color SkyBlue => ColorFromHex("87CEEB");
        public static Color LightSkyBlue => ColorFromHex("87CEFA");
        public static Color DeepSkyBlue => ColorFromHex("00BFFF");
        public static Color DodgerBlue => ColorFromHex("1E90FF");
        public static Color CornflowerBlue => ColorFromHex("6495ED");
        public static Color SteelBlue => ColorFromHex("4682B4");
        public static Color RoyalBlue => ColorFromHex("4169E1");
        public static Color Blue => ColorFromHex("0000FF");
        public static Color MediumBlue => ColorFromHex("0000CD");
        public static Color DarkBlue => ColorFromHex("00008B");
        public static Color Navy => ColorFromHex("000080");
        public static Color MidnightBlue => ColorFromHex("191970");
        public static Color Lavender => ColorFromHex("E6E6FA");
        public static Color Thistle => ColorFromHex("D8BFD8");
        public static Color Plum => ColorFromHex("DDA0DD");
        public static Color Violet => ColorFromHex("EE82EE");
        public static Color Orchid => ColorFromHex("DA70D6");
        public static Color Fuchsia => ColorFromHex("FF00FF");
        public static Color Magenta => ColorFromHex("FF00FF");
        public static Color MediumOrchid => ColorFromHex("BA55D3");
        public static Color MediumPurple => ColorFromHex("9370DB");
        public static Color BlueViolet => ColorFromHex("8A2BE2");
        public static Color DarkViolet => ColorFromHex("9400D3");
        public static Color DarkOrchid => ColorFromHex("9932CC");
        public static Color DarkMagenta => ColorFromHex("8B008B");
        public static Color Purple => ColorFromHex("800080");
        public static Color Indigo => ColorFromHex("4B0082");
        public static Color DarkSlateBlue => ColorFromHex("483D8B");
        public static Color SlateBlue => ColorFromHex("6A5ACD");
        public static Color MediumSlateBlue => ColorFromHex("7B68EE");
        public static Color White => ColorFromHex("FFFFFF");
        public static Color Snow => ColorFromHex("FFFAFA");
        public static Color Honeydew => ColorFromHex("F0FFF0");
        public static Color MintCream => ColorFromHex("F5FFFA");
        public static Color Azure => ColorFromHex("F0FFFF");
        public static Color AliceBlue => ColorFromHex("F0F8FF");
        public static Color GhostWhite => ColorFromHex("F8F8FF");
        public static Color WhiteSmoke => ColorFromHex("F5F5F5");
        public static Color Seashell => ColorFromHex("FFF5EE");
        public static Color Beige => ColorFromHex("F5F5DC");
        public static Color OldLace => ColorFromHex("FDF5E6");
        public static Color FloralWhite => ColorFromHex("FFFAF0");
        public static Color Ivory => ColorFromHex("FFFFF0");
        public static Color AntiqueWhite => ColorFromHex("FAEBD7");
        public static Color Linen => ColorFromHex("FAF0E6");
        public static Color LavenderBlush => ColorFromHex("FFF0F5");
        public static Color MistyRose => ColorFromHex("FFE4E1");
        public static Color Gainsboro => ColorFromHex("DCDCDC");
        public static Color LightGray => ColorFromHex("D3D3D3");
        public static Color Silver => ColorFromHex("C0C0C0");
        public static Color DarkGrey => ColorFromHex("A9A9A9");
        public static Color Grey => ColorFromHex("808080");
        public static Color DimGrey => ColorFromHex("696969");
        public static Color LightSlateGrey => ColorFromHex("778899");
        public static Color SlateGrey => ColorFromHex("708090");
        public static Color DarkSlateGrey => ColorFromHex("2F4F4F");
        public static Color Black => ColorFromHex("000000");

        // https://github.com/zkweb-framework/ZKWeb.System.Drawing/blob/master/src/ZKWeb.System.Drawing/System.Drawing/Color.cs
        public float GetBrightness()
        {
            byte minval = Math.Min(R, Math.Min(G, B));
            byte maxval = Math.Max(R, Math.Max(G, B));

            return (float)(maxval + minval) / 510;
        }

        public float GetSaturation()
        {
            byte minval = (byte)Math.Min(R, Math.Min(G, B));
            byte maxval = (byte)Math.Max(R, Math.Max(G, B));

            if (maxval == minval)
                return 0.0f;

            int sum = maxval + minval;
            if (sum > 255)
                sum = 510 - sum;

            return (float)(maxval - minval) / sum;
        }

        public float GetHue()
        {
            int r = R;
            int g = G;
            int b = B;
            byte minval = (byte)Math.Min(r, Math.Min(g, b));
            byte maxval = (byte)Math.Max(r, Math.Max(g, b));

            if (maxval == minval)
                return 0.0f;

            float diff = (float)(maxval - minval);
            float rnorm = (maxval - r) / diff;
            float gnorm = (maxval - g) / diff;
            float bnorm = (maxval - b) / diff;

            float hue = 0.0f;
            if (r == maxval)
                hue = 60.0f * (6.0f + bnorm - gnorm);
            if (g == maxval)
                hue = 60.0f * (2.0f + rnorm - bnorm);
            if (b == maxval)
                hue = 60.0f * (4.0f + gnorm - rnorm);
            if (hue > 360.0f)
                hue = hue - 360.0f;

            return hue;
        }
    }

    public class BeriDrawing
    {
        
    }
}
