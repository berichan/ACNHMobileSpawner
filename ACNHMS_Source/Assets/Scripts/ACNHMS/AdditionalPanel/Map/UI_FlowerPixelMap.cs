using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NHSE.Core;

public enum ImageSize
{
    s8x8 = 0,
    s16x16 = 1,
    s32x32 = 2
}

public class UI_FlowerPixelMap : MonoBehaviour
{
    private List<KeyValuePair<Item, Color>> ItemColorMap;
    private UnityColorSolver<Item> ItemColorSolver;

    public Dropdown GridSize;
    public UI_MapGrid FlowerGridGen;
    public UI_MapBulkSpawn BulkSpawner;

    private ImageSize CurrentSize = ImageSize.s16x16;

    private Vector2 startGridSize = Vector2.zero;
    private Vector2 startSpacing;
    private UI_MapItemLite mapItemPrefab;
    private Vector2 startCellSize;

    private List<KeyValuePair<Item, Color>> localGrid;

    // Start is called before the first frame update
    void Start()
    {
        var flowers = new List<Item>(ResourceLoader.GetNormalFlowers().GetArray<Item>(Item.SIZE));
        flowers.AddRange(ResourceLoader.GetHybridFlowers().GetArray<Item>(Item.SIZE));
        ItemColorMap = new List<KeyValuePair<Item, Color>>();
        foreach (var f in flowers)
        {
            if (!f.IsNone)
                ItemColorMap.Add(new KeyValuePair<Item, Color>(f, getMenuIconColor(f)));
        }

        ItemColorSolver = new UnityColorSolver<Item>(ItemColorMap);

        GridSize.onValueChanged.AddListener(delegate { CurrentSize = (ImageSize)GridSize.value; });
        startGridSize = FlowerGridGen.Layout.cellSize;
        startSpacing = FlowerGridGen.Layout.spacing;
        mapItemPrefab = FlowerGridGen.PrefabCell.GetComponent<UI_MapItemLite>();
        var prefabRect = mapItemPrefab.ItemImage.GetComponent<RectTransform>();
        startCellSize = prefabRect.sizeDelta;

        localGrid = new List<KeyValuePair<Item, Color>>();
        int size = toSize(CurrentSize);
        int fsize = size * size;
        for (int i = 0; i < fsize; ++i)
        {
            localGrid.Add(new KeyValuePair<Item, Color>(new Item(Item.NONE), Color.white));
        }

        GenGrid(localGrid, CurrentSize);
    }

    public void GenGrid(List<KeyValuePair<Item, Color>> flowerGrid, ImageSize cSize)
    {
        int size = toSize(cSize);
        Vector2 newCellSize = startGridSize / (size / 8);
        FlowerGridGen.Layout.cellSize = newCellSize;
        Vector2 newSpacing = startSpacing / (size / 8);
        FlowerGridGen.Layout.spacing = new Vector2(Mathf.Max(0.5f, (int)newSpacing.x), Mathf.Max(0.5f, (int)newSpacing.y));

        Vector2 nSize = startCellSize;
        nSize.x /= size/8;
        nSize.y /= size/8;
        mapItemPrefab.ItemImage.GetComponent<RectTransform>().sizeDelta = nSize;

        FlowerGridGen.NumHorizontal = FlowerGridGen.NumVertical = size;
        FlowerGridGen.PopulateGrid();

        for (int i = 0; i < flowerGrid.Count; ++i)
        {
            var cell = FlowerGridGen.SpawnedCells[i].GetComponent<UI_MapItemLite>();
            var kvpItemCol = flowerGrid.ElementAt(i);
            cell.BackgroundImage.color = kvpItemCol.Value;
            if (!kvpItemCol.Key.IsNone)
                cell.ItemImage.texture = MenuSpriteHelper.CurrentInstance.GetIconTexture(kvpItemCol.Key.ItemId);
            else
                cell.ItemImage.gameObject.SetActive(false);
        }
    }

    public void PromptForImage()
    {
        UI_NFSOACNHHandler.LastInstanceOfNFSO.OpenAnyFile(HandleImageLoad);
    }

    public void HandleImageLoad(byte[] raw)
    {
        int sz = toSize(CurrentSize);
        var grid = TextureToFlowers(raw, sz, ColorMatchMethod.Distance);
        GenGrid(grid, CurrentSize);

        Item[] toLoad = new Item[grid.Count];
        int i;
        for (i = 0; i < grid.Count; ++i)
        {
            var itemHere = new Item();
            itemHere.CopyFrom(grid[i].Key);
            if (!itemHere.IsNone)
            {
                var plantedId = FieldItemList.Items.FirstOrDefault(x => x.Value.Dig == itemHere.ItemId);
                if (plantedId.Key != 0)
                    itemHere.ItemId = plantedId.Key;
            }
            toLoad[i] = itemHere;
        }

        BulkSpawner.SetLoadedItems(toLoad, false);
        BulkSpawner.RectWidth.text = sz.ToString();
        BulkSpawner.RectHeight.text = sz.ToString();
    }

    public List<KeyValuePair<Item, Color>> TextureToFlowers(byte[] texture, int size, ColorMatchMethod m)
    {
        try
        {
            Texture2D t2d = new Texture2D(1, 1);
            t2d.LoadImage(texture);
            if (t2d.width < 2)
                throw new Exception("Specified image is not valid.");
            return TextureToFlowers(t2d, size, m);
        }
        catch (Exception e)
        {
            PopupHelper.CreateError(e.Message, 3f, false);
        }

        return null;
    }

    public List<KeyValuePair<Item, Color>> TextureToFlowers(Texture2D texture, int size, ColorMatchMethod m)
    {
        int count = size * size;
        Texture2D internalImg = MapGraphicGenerator.Resize(texture, size, size);
        internalImg = MapGraphicGenerator.FlipTexture(internalImg);
        var colors = internalImg.GetPixels32();
        var toRet = new List<KeyValuePair<Item, Color>>();
        for (int i = 0; i < count; ++i)
        {
            Color32 currentCol = colors[i];
            if (currentCol.a > 50)
            {
                var c = ItemColorSolver.GetClosest(currentCol, m);
                toRet.Add(new KeyValuePair<Item, Color>(c.Key, c.Value));
            }
            else
                toRet.Add(new KeyValuePair<Item, Color>(new Item(Item.NONE), new Color()));
        }

        return toRet;
    }

    Color getMenuIconColor(Item itm)
    {
        Texture2D t2d = MenuSpriteHelper.CurrentInstance.GetIconTexture(itm.ItemId);
        Color32[] colors = t2d.GetPixels32();
        const int tolerance = 10;
        int r = 0; int g = 0; int b = 0;
        int monor = 0; int monog = 0; int monob = 0;
        int loadedCount = 0; int monoCount = 0;
        int start = (int)(colors.Length * 0.6f);
        for (int i = start; i < colors.Length; ++i) // flower images usually have the good stuff at the top of the image, plus this is faster
        {
            if (colors[i].a < 5) // ignore
                continue;

            if (!isMonotone(colors[i], tolerance))
            {
                r += colors[i].r;
                g += colors[i].g;
                b += colors[i].b;
                loadedCount++;
            }
            else
            {
                monor += colors[i].r;
                monog += colors[i].g;
                monob += colors[i].b;
                monoCount++;
            }
        }

        if (!areIntegersClose(monoCount, loadedCount, (int)(colors.Length * 0.1f)))
        {
            r += monor;
            g += monog;
            b += monob;
            loadedCount += monoCount;
        }
        else
        {
            r = monor;
            g = monog;
            b = monob;
            loadedCount = monoCount;
        }

        float rb = ((float)r / (float)loadedCount);
        float gb = ((float)g / (float)loadedCount);
        float bb = ((float)b / (float)loadedCount);
        return new Color32((byte)rb, (byte)gb, (byte)bb, byte.MaxValue);
    }

    bool isMonotone(Color32 c, int tolerance)
    {
        return areIntegersClose(c.r, c.g, tolerance)
            && areIntegersClose(c.r, c.b, tolerance)
            && areIntegersClose(c.g, c.b, tolerance);
    }

    bool areIntegersClose(int a, int b, int tolerance)
    {
        return Math.Abs(a - b) < tolerance;
    }

    int toSize(ImageSize iS) => (int)(Mathf.Pow(2, (int)iS) * 8);
}
