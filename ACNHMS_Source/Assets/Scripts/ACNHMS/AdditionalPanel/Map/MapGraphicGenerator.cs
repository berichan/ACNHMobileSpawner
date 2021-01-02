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
    private readonly Color32 Alpha = new Color32(0, 0, 0, 0);

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

        background = new Texture2D(MapBackgroundImage.width, MapBackgroundImage.height);
        Graphics.CopyTexture(MapBackgroundImage, background);

        layerItems[0] = CreateItemLayerTexture(items.Layer1.Tiles, items.Layer1.MaxWidth, items.Layer1.MaxHeight);
        layerItems[1] = CreateItemLayerTexture(items.Layer2.Tiles, items.Layer2.MaxWidth, items.Layer2.MaxHeight);

        OverlayImage(MapBackgroundImage, layerItems[0]);
        MapBackgroundImage = FlipTexture(MapBackgroundImage);
        MapBackgroundImage.filterMode = FilterMode.Point;
        MapBackgroundImage.minimumMipmapLevel = 0;

        MapBackgroundImage.Apply();
    }

    private void FillRect(Texture2D pixels, int x, int y, int width, int height, Color32 fillColor)
    {
        for (int i = x; i < x + width; ++i)
            for (int j = y; j < y + height; ++j)
                pixels.SetPixel(i, j, fillColor);
    }

    private Texture2D CreateItemLayerTexture(Item[] items, int width, int height)
    {
        Texture2D toRet = new Texture2D(width, height);
        for (int x = 0; x < width; x++)
        {
            var ix = x * height;
            for (int y = 0; y < height; y++)
            {
                var index = ix + y;
                var tile = items[index];
                var pxl = FieldItemColor.GetItemColor(tile);
                toRet.SetPixel(x, y, new Color32(pxl.R, pxl.G, pxl.B, pxl.A));
            }
        }

        toRet.filterMode = FilterMode.Point;
        toRet.Apply();
        toRet = Resize(toRet, width / 2, height / 2);
        return toRet;
    }

    private void OverlayImage(Texture2D main, Texture2D overlay)
    {
        for (int i = 0; i < main.width; ++i)
        {
            for (int j = 0; j < main.height; ++j)
            {
                Color overlayCol = overlay.GetPixel(i, j);
                if (overlayCol.a > 0)
                    main.SetPixel(i, j, overlayCol);
            }
        }
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

    Texture2D Resize(Texture2D texture2D, int targetX, int targetY)
    {
        RenderTexture rt = new RenderTexture(targetX, targetY, 24);
        RenderTexture.active = rt;
        Graphics.Blit(texture2D, rt);
        Texture2D result = new Texture2D(targetX, targetY);
        result.ReadPixels(new Rect(0, 0, targetX, targetY), 0, 0);
        result.filterMode = FilterMode.Point;
        result.Apply();
        return result;
    }
}
