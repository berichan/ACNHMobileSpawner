using NHSE.Core;
using System;
using UnityEngine;

/// <summary>
/// Unity reimplementation of NHSE.Sprites.MapViewer and TerrainSprite
/// </summary>
public class MapGraphicGenerator 
{
    private const int PlazaWidth = 6 * 2;
    private const int PlazaHeight = 5 * 2;

    // No scaling required, let GPU handle it
    private readonly int[] PixelsItemMap;
    private readonly int[] PixelsBackgroundMap1;
    private readonly int[] PixelsBackgroundMapX;

    public Texture2D MapBackgroundImage { get; private set; }

    private Texture2D background;
    private Texture2D[] layerItems = new Texture2D[2];

    public MapGraphicGenerator(FieldItemManager items, NHSE.Core.TerrainLayer terrain, ushort plazaX, ushort plazaY)
    {
        PixelsItemMap = new int[items.Layer1.MaxWidth * items.Layer2.MaxHeight];
        PixelsBackgroundMap1 = new int[PixelsItemMap.Length / 4];
        PixelsBackgroundMapX = new int[PixelsItemMap.Length];

        System.Drawing.Color[] pixels = new System.Drawing.Color[PixelsBackgroundMap1.Length];
        MapBackgroundImage = new Texture2D(terrain.MaxWidth, terrain.MaxHeight);

        // draw rivers + height
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
        
        // draw plaza
        terrain.GetBuildingCoordinate(plazaX, plazaY, 1, out var xp, out var yp);
        FillRect(MapBackgroundImage, xp, yp, PlazaWidth, PlazaHeight, new Color32(188, 143, 143, 255));
        
        MapBackgroundImage = FlipTexture(MapBackgroundImage);
        MapBackgroundImage.filterMode = FilterMode.Point;
        MapBackgroundImage.minimumMipmapLevel = 0;
        Graphics.CopyTexture(MapBackgroundImage, background);


        MapBackgroundImage.Apply();
    }

    private void FillRect(Texture2D pixels, int x, int y, int width, int height, Color32 fillColor)
    {
        for (int i = x; i < x + width; ++i)
            for (int j = y; j < y + height; ++j)
                pixels.SetPixel(i, j, fillColor);
    }

    Texture2D FlipTexture(Texture2D texture)
    {
        int width = texture.width;
        int height = texture.height;
        Texture2D snap = new Texture2D(width, height);
        Color[] pixels = texture.GetPixels();
        Color[] pixelsFlipped = new Color[pixels.Length];

        for (int i = 0; i < height; i++)
        {
            Array.Copy(pixels, i * width, pixelsFlipped, (height - i - 1) * width, width);
        }

        snap.SetPixels(pixelsFlipped);
        return snap;
    }
}
