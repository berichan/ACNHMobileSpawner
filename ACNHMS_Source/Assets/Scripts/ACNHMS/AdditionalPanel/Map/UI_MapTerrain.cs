using UnityEngine;
using UnityEngine.UI;
using NHSE.Core;
using System;
using System.Linq;
using System.Collections.Generic;

public class UI_MapTerrain : MonoBehaviour
{
    public UI_Map MapParent;

    public RawImage MapImage;

    public UI_MapSelector GridSelector;
    public UI_MapGrid AcreTileMap;

    private MapGraphicGenerator graphicGenerator;

    // acre data from NHSE.Core.MainSave.cs
    public const int AcreWidth = 7 + (2 * 1); // 1 on each side cannot be traversed
    private const int AcreHeight = 6 + (2 * 1); // 1 on each side cannot be traversed
    private const int AcreMax = AcreWidth * AcreHeight;
    private const int AcreSizeAll = AcreMax * 2;
    private const int AcrePlusAdditionalParams = AcreSizeAll + 2 + 4 + 8 + sizeof(uint); // MainFieldParamUniqueID + EventPlazaLeftUpX + EventPlazaLeftUpZ
    private const int MapItemCount = MapGrid.MapTileCount32x32 / 4; // Ignore extensions
    private const int MapEndX = (MapGrid.AcreWidth * 32) - 32; // End of selectable 8x8 corner
    private const int MapEndY = (MapGrid.AcreHeight * 32) - 32; // End of selectable 8x8 corner

    private const int FieldSize = MapGrid.MapTileCount32x32 * 2 * Item.SIZE;
    private const int TerrainSize = MapGrid.MapTileCount16x16 * TerrainTile.SIZE;

    private byte[] field, terrain, acre_plaza;
    private uint plazaX, plazaY;
    private bool fetched = false;

    private FieldItemManager fieldManager;
    private NHSE.Core.TerrainLayer terrainLayer;
    private List<UI_MapItemTile> itemTiles;

    // Start is called before the first frame update
    void Start()
    {
        AcreTileMap.PopulateGrid();
        itemTiles = new List<UI_MapItemTile>();
        foreach (var cell in AcreTileMap.SpawnedCells)
            itemTiles.Add(cell.GetComponent<UI_MapItemTile>());
        GridSelector.OnSelectorChanged += updateAcreGrid;
    }

    public void GenerateMap()
    {
        fetchIndex(0);
    }

    void updateAcreGrid(Vector2 selectorPos)
    {
        if (!fetched) return;
        int itemStartX = Mathf.RoundToInt(Mathf.Lerp(0, MapEndX, selectorPos.x));
        int itemStartY = Mathf.RoundToInt(Mathf.Lerp(0, MapEndY, selectorPos.y));
        updateGrid(itemStartX, itemStartY);
    }

    void updateGrid(int startX, int startY)
    {
        int index = 0;
        for (int i = startY; i < startY + 32; i += 2)
        {
            for (int j = startX; j < startX + 32; j += 2)
            {
                itemTiles[index].SetItem(fieldManager.Layer1.Tiles[i + j], 
                    fieldManager.Layer1.Tiles[i + j + 1], 
                    fieldManager.Layer1.Tiles[i + j + 1 + (MapGrid.AcreWidth * 32)], 
                    fieldManager.Layer1.Tiles[i + j + (MapGrid.AcreWidth * 32)]);
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
    }

    // loops between map layers to fetch everything
    void fetchIndex(int index)
    {
        switch (index)
        {
            case 0:
                createFetchPopup("Fetching field (1 of 3)...", 0, (uint)OffsetHelper.FieldItemStart, FieldSize, () => { fetchIndex(1); });
                break;
            case 1:
                createFetchPopup("Fetching terrain (2 of 3)...", 1, (uint)OffsetHelper.LandMakingMapStart, TerrainSize, () => { fetchIndex(2); });
                break;
            case 2:
                createFetchPopup("Fetching acre and generating map (3 of 3)...", 2, (uint)OffsetHelper.OutsideFieldStart, AcrePlusAdditionalParams, () => { fetchIndex(3); });
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
