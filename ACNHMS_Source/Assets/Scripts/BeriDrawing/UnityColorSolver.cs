using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UnityColorSolver<T>
{
    private readonly Dictionary<T, Color> Pallette;
    private readonly Beri.Drawing.Color[] ColorOnlyPallete;

    public UnityColorSolver(Dictionary<T, Color> PossibleColors)
    {
        Pallette = PossibleColors;
        ColorOnlyPallete = new Beri.Drawing.Color[Pallette.Count];
        for (int i = 0; i < Pallette.Count; ++i)
            ColorOnlyPallete[i] = ((Color32)Pallette.Values.ElementAt(i)).ToBeriColor();
    }
    
    public KeyValuePair<T, Color> GetClosest(Color32 c, ColorMatchMethod m)
    {
        Beri.Drawing.Color beriCol = c.ToBeriColor();
        int index = beriCol.GetColorMatch(ColorOnlyPallete, m);
        return Pallette.ElementAt(index);
    }
}