using NHSE.Core;
using UnityEngine;

/// <summary>
/// Unity reimplementation of NHSE.Sprites.MapViewer and TerrainSprite
/// </summary>
public class MapGraphicGenerator 
{
    // No scaling required, let GPU handle it
    private readonly int[] PixelsItemMap;
    private readonly int[] PixelsBackgroundMap1;
    private readonly int[] PixelsBackgroundMapX;

    public Texture2D MapBackgroundImage { get; private set; }

    public MapGraphicGenerator(FieldItemManager items, NHSE.Core.TerrainLayer terrain)
    {
        PixelsItemMap = new int[items.Layer1.MaxWidth * items.Layer2.MaxHeight];
        PixelsBackgroundMap1 = new int[PixelsItemMap.Length / 4];
        PixelsBackgroundMapX = new int[PixelsItemMap.Length];

        System.Drawing.Color[] pixels = new System.Drawing.Color[PixelsBackgroundMap1.Length];
        MapBackgroundImage = new Texture2D(terrain.MaxWidth, terrain.MaxHeight);
        int i = 0;
        for (int y = 0; y < terrain.MaxHeight; y++)
        {
            for (int x = 0; x < terrain.MaxWidth; x++, i++)
            {
                var pxl = terrain.GetTileColorRGB(x, y);
                MapBackgroundImage.SetPixel(x, y, new Color32(pxl.R, pxl.G, pxl.B, pxl.A));
                pixels[i] = pxl;
            }
        }
        

        MapBackgroundImage.filterMode = FilterMode.Point;
        MapBackgroundImage.minimumMipmapLevel = 0;
        MapBackgroundImage.Apply();
    }
}
