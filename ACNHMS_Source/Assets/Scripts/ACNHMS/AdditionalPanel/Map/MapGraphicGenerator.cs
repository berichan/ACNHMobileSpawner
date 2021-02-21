using NHSE.Core;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Unity reimplementation of NHSE.Sprites.MapViewer and TerrainSprite
/// </summary>
public class MapGraphicGenerator 
{
    private const int PlazaWidth = 6 * 2;
    private const int PlazaHeight = 5 * 2;
    private readonly Color32 Alpha = new Color32(0, 0, 0, 0);
    private readonly Color32 PlazaCol = new Color32(188, 143, 143, 255);
    private readonly Color32 BuildingCol = Color.yellow;

    // No scaling required, let GPU handle it
    private readonly int[] PixelsItemMap;
    private readonly int[] PixelsBackgroundMap1;
    private readonly int[] PixelsBackgroundMapX;
    private readonly FieldItemManager ItemManager;
    private readonly NHSE.Core.TerrainLayer Terrain;

    public Texture2D MapBackgroundImage { get; private set; }

    private Texture2D background;
    private Texture2D[] layerItems = new Texture2D[2];

    public MapGraphicGenerator(FieldItemManager items, NHSE.Core.TerrainLayer terrain, ushort plazaX, ushort plazaY, Building[] buildings)
    {
        PixelsItemMap = new int[items.Layer1.MaxWidth * items.Layer2.MaxHeight];
        PixelsBackgroundMap1 = new int[PixelsItemMap.Length / 4];
        PixelsBackgroundMapX = new int[PixelsItemMap.Length];
        ItemManager = items;
        Terrain = terrain;

        Beri.Drawing.Color[] pixels = new Beri.Drawing.Color[PixelsBackgroundMap1.Length];
        MapBackgroundImage = new Texture2D(Terrain.MaxWidth, Terrain.MaxHeight);

        // draw rivers + height
        int i = 0;
        for (int y = 0; y < Terrain.MaxHeight; y++)
        {
            for (int x = 0; x < Terrain.MaxWidth; x++, i++)
            {
                var pxl = Terrain.GetTileColorRGB(x, y);
                MapBackgroundImage.SetPixel(x, y, new Color32(pxl.R, pxl.G, pxl.B, pxl.A));
                pixels[i] = pxl;
            }
        }

        // draw buildings
        PlaceBuildings(Terrain, MapBackgroundImage, buildings);

        // draw plaza
        Terrain.GetBuildingCoordinate(plazaX, plazaY, 1, out var xp, out var yp);
        FillRect(MapBackgroundImage, xp, yp, PlazaWidth, PlazaHeight, PlazaCol);

        background = new Texture2D(MapBackgroundImage.width, MapBackgroundImage.height);
        Graphics.CopyTexture(MapBackgroundImage, background);
        //background = FlipTexture(background); // no need to flip for backgroud pixels of acre

        UpdateImageForLayer(0);
    }

    public Color GetBackgroudPixel(int x, int y) => background.GetPixel(x, y);
    public Beri.Drawing.Color UnityColorToSystemColor(Color32 c) => Beri.Drawing.Color.FromArgb(c.a, c.r, c.g, c.b);
    public bool CompareUnityColorToSystemColor(Color32 c, Beri.Drawing.Color cs)
    {
        return c.a == cs.A && c.r == cs.R && c.g == cs.G && c.b == cs.B;
    }
    public bool IsGroundTile(int x, int y)
    {
        Color pixelCol = GetBackgroudPixel(x, y);
        if (pixelCol == PlazaCol || pixelCol == BuildingCol)
            return false;
        return Terrain.IsTileColorSafe(x, y);
    }
    
    
    public void UpdateImageForLayer(int layer)
    {
        var fil = layer == 0 ? ItemManager.Layer1 : ItemManager.Layer2;
        layerItems[layer] = CreateItemLayerTexture(fil.Tiles, fil.MaxWidth, fil.MaxHeight);

        Graphics.CopyTexture(background, MapBackgroundImage);
        OverlayImage(MapBackgroundImage, layerItems[layer]);
        MapBackgroundImage = FlipTexture(MapBackgroundImage);
        MapBackgroundImage.filterMode = FilterMode.Point;
        MapBackgroundImage.minimumMipmapLevel = 0;

        MapBackgroundImage.Apply();
    }

    public void ReleaseAllResources()
    {
        if (background != null)
            UnityEngine.Object.Destroy(background);
        if (MapBackgroundImage != null)
            UnityEngine.Object.Destroy(MapBackgroundImage);
        if (layerItems[0] != null)
            UnityEngine.Object.Destroy(layerItems[0]);
        if (layerItems[1] != null)
            UnityEngine.Object.Destroy(layerItems[1]);
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

    private Texture2D PlaceBuildings(NHSE.Core.TerrainLayer terrain, Texture2D tex, Building[] buildings)
    {
        for (int i = 0; i < buildings.Length; i++)
        {
            var b = buildings[i];
            if (b.BuildingType == 0)
                continue;
            terrain.GetBuildingCoordinate(b.X, b.Y, 1, out var x, out var y);

            var pen = BuildingCol;
            FillRect(tex, x - 2, y - 2, 4, 4, pen);
        }
        return tex;
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

    public static Texture2D FlipTexture(Texture2D texture)
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

    public static Texture2D Resize(Texture2D texture2D, int targetX, int targetY)
    {
        RenderTexture rt = new RenderTexture(targetX, targetY, 24);
        RenderTexture.active = rt;
        Graphics.Blit(texture2D, rt);
        Texture2D result = new Texture2D(targetX, targetY);
        result.ReadPixels(new Rect(0, 0, targetX, targetY), 0, 0);
        result.filterMode = FilterMode.Point;
        result.Apply();
        UnityEngine.Object.Destroy(rt);
        return result;
    }
}
