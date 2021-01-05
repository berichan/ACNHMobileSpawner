using UnityEngine;
using UnityEngine.UI;
using NHSE.Core;
using System;
using System.Linq;
using System.Collections.Generic;

public enum TerrainSelectMode
{
    Place,
    Delete,
    Load
}

public class UI_MapTerrain : MonoBehaviour
{
    public UI_Map MapParent;

    public RawImage MapImage;
    public Button RefetchItemsButton;

    public UI_MapSelector GridSelector;
    public UI_MapGrid AcreTileMap;

    public GameObject UnfetchedBlocker;

    private MapGraphicGenerator graphicGenerator;

    // acre data from NHSE.Core.MainSave.cs
    public const int AcreWidth = 7 + (2 * 1); // 1 on each side cannot be traversed
    private const int AcreHeight = 6 + (2 * 1); // 1 on each side cannot be traversed
    private const int AcreMax = AcreWidth * AcreHeight;
    private const int AcreSizeAll = AcreMax * 2;
    private const int AcrePlusAdditionalParams = AcreSizeAll + 2 + 4 + 8 + sizeof(uint); // MainFieldParamUniqueID + EventPlazaLeftUpX + EventPlazaLeftUpZ
    private const int MapItemCount = MapGrid.MapTileCount32x32 / 4; // Ignore extensions
    private const int MapItemsWidthMax = MapGrid.AcreWidth * 32;
    private const int MapItemsHeightMax = MapGrid.AcreHeight * 32;
    private const int MapEndX = (MapGrid.AcreWidth * 32) - 16; // End of selectable 8x8 corner
    private const int MapEndY = (MapGrid.AcreHeight * 32) - 16; // End of selectable 8x8 corner

    private const int FieldSize = MapGrid.MapTileCount32x32 * 2 * Item.SIZE;
    private const int TerrainSize = MapGrid.MapTileCount16x16 * TerrainTile.SIZE;

    private byte[] field, terrain, acre_plaza;
    private uint plazaX, plazaY;
    private bool fetched = false;

    private FieldItemManager fieldManager;
    private NHSE.Core.TerrainLayer terrainLayer;
    private List<UI_MapItemTile> itemTiles;

    public Dropdown SelectMode;

    [HideInInspector]
    public TerrainSelectMode CurrentSelectMode { get; private set; } = TerrainSelectMode.Place;

    // Start is called before the first frame update
    void Start()
    {
        SelectMode.ClearOptions();
        string[] smChoices = Enum.GetNames(typeof(TerrainSelectMode));
        foreach (string sm in smChoices)
        {
            Dropdown.OptionData newVal = new Dropdown.OptionData();
            newVal.text = sm;
            SelectMode.options.Add(newVal);
        }
        SelectMode.value = 0;
        SelectMode.RefreshShownValue();
        SelectMode.onValueChanged.AddListener(delegate { CurrentSelectMode = (TerrainSelectMode)SelectMode.value; });

        AcreTileMap.PopulateGrid();
        itemTiles = new List<UI_MapItemTile>();
        foreach (var cell in AcreTileMap.SpawnedCells)
            itemTiles.Add(cell.GetComponent<UI_MapItemTile>());
        GridSelector.OnSelectorChanged += updateAcreGrid;
        UnfetchedBlocker.gameObject.SetActive(true);
    }

    public void GenerateMap() => fetchIndex(0);
    public void RefetchItems() => fetchIndex(0, true);

    void updateAcreGrid(Vector2 selectorPos)
    {
        if (!fetched) return;
        int itemStartX = Mathf.RoundToInt(Mathf.Lerp(0, MapEndX, selectorPos.x));
        int itemStartY = Mathf.RoundToInt(Mathf.Lerp(0, MapEndY, selectorPos.y));
        updateGrid(itemStartX, itemStartY);
    }

    void updateGrid(int startX, int startY)
    {
        // Go up 1 if these are odd nums (ext tiles)
        if (startX % 2 != 0)
            startX--;
        if (startY % 2 != 0)
            startY--;
        
        var tiles = fieldManager.Layer1.Tiles;
        int index = 0;
        for (int i = startX; i < startX + 16; i += 2)
        {
            var ix = i * MapItemsHeightMax;
            for (int j = startY; j < startY + 16; j += 2)
            {
                var indexItem = ix + j;
                var indexTile = ((index * 8) % 64) + Mathf.FloorToInt(index / 8f);
                var bgColor = graphicGenerator.GetBackgroudPixel(i/2, j/2);
                itemTiles[indexTile].SetItem(new FieldItemBlock(fieldManager.Layer1, i, j), bgColor, this);
                index++;
            }
        }
    }

    void generateAll()
    {
        Item[] itemLayer1 = Item.GetArray(field.Take(MapGrid.MapTileCount32x32 * Item.SIZE).ToArray());
        Item[] itemLayer2 = Item.GetArray(field.Slice(MapGrid.MapTileCount32x32 * Item.SIZE, MapGrid.MapTileCount32x32 * Item.SIZE).ToArray());
        fieldManager = new FieldItemManager(itemLayer1, itemLayer2);
        terrainLayer = new NHSE.Core.TerrainLayer(TerrainTile.GetArray(terrain), acre_plaza.Slice(0, AcreSizeAll));

        plazaX = BitConverter.ToUInt32(acre_plaza, AcreSizeAll + 4);
        plazaY = BitConverter.ToUInt32(acre_plaza, AcreSizeAll + 8);

        graphicGenerator = new MapGraphicGenerator(fieldManager, terrainLayer, (ushort)plazaX, (ushort)plazaY);
        MapImage.texture = graphicGenerator.MapBackgroundImage;
        MapImage.color = Color.white;
        fetched = true;

        GridSelector.ResetPosition();
        UnfetchedBlocker.gameObject.SetActive(false);
        RefetchItemsButton.interactable = true;
    }

    // loops between map layers to fetch everything
    void fetchIndex(int index, bool refetch = false)
    {
        switch (index)
        {
            case 0:
                createFetchPopup($"Fetching field{(refetch ? string.Empty : " (1 of 3)")}...\r\nThis may take a long time if your thread sleep time is above 20ms", 0, (uint)OffsetHelper.FieldItemStart, FieldSize, () => { fetchIndex(1, refetch); });
                break;
            case 1 when !refetch:
                createFetchPopup("Fetching terrain (2 of 3)...", 1, (uint)OffsetHelper.LandMakingMapStart, TerrainSize, () => { fetchIndex(2, refetch); });
                break;
            case 2 when !refetch:
                createFetchPopup("Fetching acre and generating map (3 of 3)...", 2, (uint)OffsetHelper.OutsideFieldStart, AcrePlusAdditionalParams, () => { fetchIndex(3, refetch); });
                break;
            default:
                generateAll();
                break;
        }
    }

    void createFetchPopup(string msg, int index, uint offset, int size, Action next)
    {
        byte[] toPopulate = new byte[0];
        UI_Popup.CurrentInstance.CreatePopupMessage(0.001f, msg, () => { toPopulate = MapParent.CurrentConnection.ReadBytes(offset, size); setFieldArray(index, toPopulate); next.Invoke(); });
    }

    void setFieldArray(int index, byte[] pop)
    {
        switch (index)
        {
            case 0:
                field = pop;
                break;
            case 1:
                terrain = pop;
                break;
            case 2:
                acre_plaza = pop;
                break;
        }
    }
}
