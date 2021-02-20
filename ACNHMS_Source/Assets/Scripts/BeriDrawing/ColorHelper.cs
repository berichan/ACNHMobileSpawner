using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Beri.Drawing;

public enum ColorMatchMethod
{
    Hues = 0,
    Distance = 1,
    Weight = 2
}

// https://stackoverflow.com/a/27375621/1508779
public static class ColorHelper
{
    const float factorSat = 0.5f;
    const float factorBri = 0.5f;
    
    /// <returns>index</returns>
    public static int GetColorMatch(this Color col, IReadOnlyCollection<Color> colors, ColorMatchMethod method)
    {
        switch (method)
        {
            case ColorMatchMethod.Hues: return ClosestColor1(colors, col);
            case ColorMatchMethod.Distance: return ClosestColor2(colors, col);
            case ColorMatchMethod.Weight: return ClosestColor3(colors, col);
            default: return ClosestColor2(colors, col);
        }
    }

    // closed match for hues only:
    public static int ClosestColor1(IReadOnlyCollection<Color> colors, Color target)
    {
        var hue1 = target.GetHue();
        var diffs = colors.Select(n => GetHueDistance(n.GetHue(), hue1));
        var diffMin = diffs.Min(n => n);
        return diffs.ToList().FindIndex(n => n == diffMin);
    }

    // closed match in RGB space
    public static int ClosestColor2(IReadOnlyCollection<Color> colors, Color target)
    {
        var colorDiffs = colors.Select(n => ColorDiff(n, target)).Min(n => n);
        return colors.ToList().FindIndex(n => ColorDiff(n, target) == colorDiffs);
    }

    // weighed distance using hue, saturation and brightness
    public static int ClosestColor3(IReadOnlyCollection<Color> colors, Color target)
    {
        float hue1 = target.GetHue();
        var num1 = ColorNum(target);
        var diffs = colors.Select(n => Math.Abs(ColorNum(n) - num1) +
                                       GetHueDistance(n.GetHue(), hue1));
        var diffMin = diffs.Min(x => x);
        return diffs.ToList().FindIndex(n => n == diffMin);
    }

    // color brightness as perceived:
    public static float GetBrightness(Color c)
    { return (c.R * 0.299f + c.G * 0.587f + c.B * 0.114f) / 256f; }

    // distance between two hues:
    public static float GetHueDistance(float hue1, float hue2)
    {
        float d = Math.Abs(hue1 - hue2); return d > 180 ? 360 - d : d;
    }

    //  weighed only by saturation and brightness (from my trackbars)
    public static float ColorNum(Color c)
    {
        return c.GetSaturation() * factorSat +
                    GetBrightness(c) * factorBri;
    }

    // distance in RGB space
    public static int ColorDiff(Color c1, Color c2)
    {
        return (int)Math.Sqrt((c1.R - c2.R) * (c1.R - c2.R)
                               + (c1.G - c2.G) * (c1.G - c2.G)
                               + (c1.B - c2.B) * (c1.B - c2.B));
    }

    public static UnityEngine.Color ToUnityColor(this Color col)
    {
        return new UnityEngine.Color32(col.R, col.G, col.B, col.A);
    }

    public static Color ToBeriColor(this UnityEngine.Color32 col)
    {
        return new Color(col.r, col.g, col.b, col.a);
    }
}
