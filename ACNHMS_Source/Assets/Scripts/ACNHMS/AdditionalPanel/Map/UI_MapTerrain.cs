using UnityEngine;
using UnityEngine.UI;
using NHSE.Core;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using static UI_MapBulkSpawn.SpawnDirection;
using System.IO;

public enum TerrainSelectMode
{
    Drop,
    Delete,
    Load,
    Custom,
    Place
}

public enum AffectMode // maybe terrain/map tiles eventually
{
    Layer1,
    Layer2
}

public class OffsetData
{
    public uint Offset;
    public byte[] ToSend;
    public OffsetData(uint os, byte[] data) { Offset = os; ToSend = data; }
}

public class UI_MapTerrain : MonoBehaviour
{
    public const string ByteDumpName = "InternalMapBackup";
    public string FilePathDump => Application.persistentDataPath + Path.DirectorySeparatorChar + ByteDumpName + Path.DirectorySeparatorChar;

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

    private const int BuildingSize = 46 * Building.SIZE;

    private byte[] field, terrain, acre_plaza, structure;
    private uint plazaX, plazaY;
    private bool fetched = false;
    private int lastCursorX, lastCursorY;

    private string fieldFile => FilePathDump + nameof(field) + ".bin";
    private string terrainFile => FilePathDump + nameof(terrain) + ".bin";
    private string acre_plazaFile => FilePathDump + nameof(acre_plaza) + ".bin";
    private string structureFile => FilePathDump + nameof(structure) + ".bin";

    private Item[] layerTemplate1, layerTemplate2;

    private FieldItemManager fieldManager;
    private NHSE.Core.TerrainLayer terrainLayer;
    private List<Building> buildings;
    private List<UI_MapItemTile> itemTiles;

    public Dropdown SelectMode;
    public Dropdown AffectingMode;
    public Button WriteButton;
    public Button LoadLastButton;
    public Button SaveButton;
    public Text MapCoords;

    public UI_MapBulkSpawn BulkSpawner;

    private static UI_SearchWindow SearchWindow => UI_SearchWindow.LastLoadedSearchWindow;

    private static Item refItem = Item.NO_ITEM;
    public static Item ReferenceItem(int flag0 = -1) { var ret = SearchWindow.GetAsItem(null); if (flag0 > 0) ret.SystemParam = Convert.ToByte(flag0); return ret; }
    
    public Text CurrentLoadedItemName;

    private float timer = 0f;

    [HideInInspector]
    public TerrainSelectMode CurrentSelectMode { get; private set; } = 0;

    [HideInInspector]
    public AffectMode CurrentAffectMode { get; private set; } = AffectMode.Layer1;

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
        SelectMode.onValueChanged.AddListener(delegate { CurrentSelectMode = (TerrainSelectMode)SelectMode.value; });
        SelectMode.value = 0;
        SelectMode.RefreshShownValue();

        AffectingMode.ClearOptions();
        string[] amChoices = Enum.GetNames(typeof(AffectMode));
        foreach (string am in amChoices)
        {
            Dropdown.OptionData newVal = new Dropdown.OptionData();
            newVal.text = am;
            AffectingMode.options.Add(newVal);
        }
        AffectingMode.onValueChanged.AddListener(delegate { CurrentAffectMode = (AffectMode)AffectingMode.value; UpdateLayerImage(); });
        AffectingMode.value = 0;
        AffectingMode.RefreshShownValue();

        AcreTileMap.PopulateGrid();
        itemTiles = new List<UI_MapItemTile>();
        foreach (var cell in AcreTileMap.SpawnedCells)
            itemTiles.Add(cell.GetComponent<UI_MapItemTile>());
        GridSelector.OnSelectorChanged += updateAcreGrid;
        UnfetchedBlocker.gameObject.SetActive(true);

        SearchWindow.OnNewItemSelected += updateItem;

        LoadLastButton.gameObject.SetActive(checkAndLoadForExistingFiles());
        updatePositionText();
    }

    private void updatePositionText()
    {
        MapCoords.text = $"X:{lastCursorX:000}, Y:{lastCursorY:000}";
    }

    private bool checkAndLoadForExistingFiles()
    {
        if (!File.Exists(fieldFile))
            return false;
        else
            field = File.ReadAllBytes(fieldFile);

        if (!File.Exists(terrainFile))
            return false;
        else
            terrain = File.ReadAllBytes(terrainFile);

        if (!File.Exists(acre_plazaFile))
            return false;
        else
            acre_plaza = File.ReadAllBytes(acre_plazaFile);

        if (!File.Exists(structureFile))
            return false;
        else
            structure = File.ReadAllBytes(structureFile);

        return true;
    }

    private void saveAll()
    {
        if (!Directory.Exists(FilePathDump))
            Directory.CreateDirectory(FilePathDump);
        if (field != null)
        {
            if (File.Exists(fieldFile))
                File.Delete(fieldFile);
            var listLayer1 = fieldManager.Layer1.Tiles.SetArray(Item.SIZE);
            var listLayer2 = fieldManager.Layer2.Tiles.SetArray(Item.SIZE);
            var allField = listLayer1.Concat(listLayer2).ToArray();
            File.WriteAllBytes(fieldFile, allField);
        }
        if (terrain != null)
        {
            if (File.Exists(terrainFile))
                File.Delete(terrainFile);
            File.WriteAllBytes(terrainFile, terrain);
        }
        if (acre_plaza != null)
        {
            if (File.Exists(acre_plazaFile))
                File.Delete(acre_plazaFile);
            File.WriteAllBytes(acre_plazaFile, acre_plaza);
        }
        if (structure != null)
        {
            if (File.Exists(structureFile))
                File.Delete(structureFile);
            File.WriteAllBytes(structureFile, structure);
        }
    }

    private void Update() // periodically save everything
    {
        timer += Time.deltaTime;
        if (timer > 60f)
        {
            timer = 0f;
            saveAll();
        }
    }

    public void BringSearchWindowToFront() => SearchWindow.SetAtFront(true, false);

    private void updateItem(ushort id, string itemName)
    {
        CurrentLoadedItemName.text = itemName;
    }

    public void GenerateMap() => fetchIndex(0);
    public void RefetchItems() => fetchIndex(0, true);
    public void WriteChanges() => sendNewBytes();

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

        var layer = CurrentAffectMode == AffectMode.Layer1 ? fieldManager.Layer1 : fieldManager.Layer2;
        var tiles = fieldManager.Layer1.Tiles;
        int index = 0;
        for (int i = startX; i < startX + 16; i += 2)
        {
            var ix = i * MapItemsHeightMax;
            for (int j = startY; j < startY + 16; j += 2)
            {
                var indexTile = ((index * 8) % 64) + Mathf.FloorToInt(index / 8f);
                var bgColor = graphicGenerator.GetBackgroudPixel(i/2, j/2);
                var block = new FieldItemBlock(layer, i, j);
                itemTiles[indexTile].SetItem(block, bgColor, this);
                index++;
            }
        }

        lastCursorX = startX;
        lastCursorY = startY;
        updatePositionText();
    }

    public void BulkSpawn()
    {
        try
        {
            bulkSpawn();
        }
        catch (Exception e)
        {
            UI_Popup.CurrentInstance.CreatePopupChoice($"Error: {e.Message}", "OK", () => { }, Color.red);
        }
        finally
        {
            UpdateLayerImage();
            updateGrid(lastCursorX, lastCursorY);
        }
    }

    private void bulkSpawn()
    {
        var itemsToSpawn = BulkSpawner.GetItemsOfCurrentPreset();
        if (itemsToSpawn.Length < 1)
            return;

        int x = lastCursorX;
        int y = lastCursorY;
        float widthMultiplier = BulkSpawner.RectWidthDimension / BulkSpawner.RectHeightDimension;
        int mWidth = Mathf.RoundToInt(Mathf.Sqrt(itemsToSpawn.Length * widthMultiplier));
        var spawnDir = BulkSpawner.CurrentSpawnDirection;
        int dirX = (spawnDir == SouthEast || spawnDir == NorthEast) ? 2 : -2;
        int dirY = (spawnDir == NorthWest || spawnDir == NorthEast) ? -2 : 2;
        int xLimit = x + (dirX * mWidth);
        var layer = CurrentAffectMode == AffectMode.Layer1 ? fieldManager.Layer1 : fieldManager.Layer2;

        bool spawnComplete = false;
        int currentItemIndex = 0;
        int skipCount = 0;

        while (!spawnComplete)
        {
            if (x >= layer.MaxWidth || x < 0 || y >= layer.MaxHeight || y < 0)
            {
                spawnComplete = true;
                Debug.Log($"Skipped {skipCount} times.");
                throw new Exception("Reached the bounds of your map without enough space. Some items did not spawn.");
            }

            Item currentTile = layer.GetTile(x, y);
            var isClear = layer.IsOccupied(currentTile, x, y) == PlacedItemPermission.NoCollision;
            // auto set to root tiles thanks to updateGrid
            if ((!BulkSpawner.OverwriteTiles && !isClear) || !graphicGenerator.IsGroundTile(x/2,y/2))
            {
                // do nothing
                skipCount++;
            }
            else 
            {
                if (!isClear) // may break but try/catch should catch it outside of bounds
                {
                    // delete current item (square only)
                    var tileToDelete = currentTile;
                    if (tileToDelete.IsExtension)
                    {
                        var l = layer;
                        var rx = Math.Max(0, Math.Min(l.MaxWidth - 1, x - tileToDelete.ExtensionX));
                        var ry = Math.Max(0, Math.Min(l.MaxHeight - 1, y - tileToDelete.ExtensionY));
                        var redir = l.GetTile(rx, ry);
                        if (redir.IsRoot && redir.ItemId == tileToDelete.ExtensionItemId)
                            tileToDelete = redir;

                        layer.DeleteExtensionTiles(tileToDelete, rx, ry);
                    }
                    layer.DeleteExtensionTiles(tileToDelete, x, y);
                }

                // place item
                var itemToPlace = itemsToSpawn[currentItemIndex];
                if (itemToPlace.IsNone)
                {
                    layer.DeleteExtensionTiles(currentTile, x, y);
                    currentTile.Delete();
                }
                else
                {
                    currentTile.CopyFrom(itemToPlace);
                    layer.SetExtensionTiles(currentTile, x, y);
                }
                currentItemIndex++;
            }

            x += dirX;
            if (x == xLimit)
            {
                x = lastCursorX;
                y += dirY;
            }

            if (currentItemIndex >= itemsToSpawn.Length)
                spawnComplete = true;
        }
    }

    public void UpdateLayerImage()
    {
        if (graphicGenerator == null)
            return;

        int layer = (int)CurrentAffectMode;
        graphicGenerator.UpdateImageForLayer(layer);
        MapImage.texture = graphicGenerator.MapBackgroundImage;
    }

    public void GenAllWithLoadedBytes() => generateAll();

    void generateAll()
    {
        Item[] itemLayer1 = Item.GetArray(field.Take(MapGrid.MapTileCount32x32 * Item.SIZE).ToArray());
        Item[] itemLayer2 = Item.GetArray(field.Slice(MapGrid.MapTileCount32x32 * Item.SIZE, MapGrid.MapTileCount32x32 * Item.SIZE).ToArray());

        // create templates for pushing bytes back
        layerTemplate1 = CloneItemArray(itemLayer1);
        layerTemplate2 = CloneItemArray(itemLayer2);

        fieldManager = new FieldItemManager(itemLayer1, itemLayer2);
        terrainLayer = new NHSE.Core.TerrainLayer(TerrainTile.GetArray(terrain), acre_plaza.Slice(0, AcreSizeAll));
        buildings = new List<Building>(Building.GetArray(structure));

        plazaX = BitConverter.ToUInt32(acre_plaza, AcreSizeAll + 4);
        plazaY = BitConverter.ToUInt32(acre_plaza, AcreSizeAll + 8);

        if (graphicGenerator != null) graphicGenerator.ReleaseAllResources();
        graphicGenerator = new MapGraphicGenerator(fieldManager, terrainLayer, (ushort)plazaX, (ushort)plazaY, buildings.ToArray());
        MapImage.texture = graphicGenerator.MapBackgroundImage;
        MapImage.color = Color.white;
        fetched = true;

        updateGrid(lastCursorX, lastCursorY);
        UnfetchedBlocker.gameObject.SetActive(false);
        AffectingMode.interactable = true;
        RefetchItemsButton.interactable = true;
        WriteButton.interactable = true;
        SaveButton.interactable = true;
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
                createFetchPopup("Fetching acre (3 of 3)...", 2, (uint)OffsetHelper.OutsideFieldStart, AcrePlusAdditionalParams, () => { fetchIndex(3, refetch); });
                break;
            case 3 when !refetch:
                createFetchPopup("Placing buildings and generating map...", 3, (uint)OffsetHelper.MainFieldStructurStart, BuildingSize, () => { fetchIndex(4, refetch); saveAll(); });
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
            case 0: field = pop; break;
            case 1: terrain = pop; break;
            case 2: acre_plaza = pop; break;
            case 3: structure = pop; break;
        }
    }

    public void SaveLayer(int l)
    {
        Item[] tiles = l == 0 ? fieldManager.Layer1.Tiles : fieldManager.Layer2.Tiles;
        var bytes = tiles.SetArray(Item.SIZE);
        UI_NFSOACNHHandler.LastInstanceOfNFSO.SaveFile($"{DateTime.Now.ToString("yyyyddMM_HHmmss")}_Layer{l + 1}.nhl", bytes);
    }

    public void LoadLayer(int l) => UI_NFSOACNHHandler.LastInstanceOfNFSO.OpenFile("nhl", (x) => { loadBytes(l, x); });

    private void loadBytes(int l, byte[] data)
    {
        Array.Copy(data, 0, field, l * MapGrid.MapTileCount32x32 * Item.SIZE, MapGrid.MapTileCount32x32 * Item.SIZE);
        generateAll();
    }

    void sendNewBytes()
    {
        const int acreSizeItems = 32 * 32;
        const uint acreSizeBytes = acreSizeItems * Item.SIZE;

        // convert all to lists
        var listLayer1 = new List<Item>(fieldManager.Layer1.Tiles);
        var listLayer2 = new List<Item>(fieldManager.Layer2.Tiles);
        var templateLayer1 = new List<Item>(layerTemplate1);
        var templateLayer2 = new List<Item>(layerTemplate2);

        // split everything into acres
        var splitl1 = listLayer1.ChunkBy(acreSizeItems);
        var splitl2 = listLayer2.ChunkBy(acreSizeItems);
        var splitlt1 = templateLayer1.ChunkBy(acreSizeItems);
        var splitlt2 = templateLayer2.ChunkBy(acreSizeItems);

        var offset = (uint)OffsetHelper.FieldItemStart;
        var offsetl2 = offset + (MapGrid.MapTileCount32x32 * Item.SIZE);
        var dataSendList = new List<OffsetData>();

        // layer 1
        for (uint i = 0; i < splitl1.Count; ++i)
        {
            int ix = (int)i;
            if (splitl1[ix].IsDifferent(splitlt1[ix]))
                dataSendList.Add(new OffsetData(offset + (i * acreSizeBytes), splitl1[ix].SetArray(Item.SIZE)));
        }

        // layer 2
        for (uint i = 0; i < splitl2.Count; ++i)
        {
            int ix = (int)i;
            if (splitl2[ix].IsDifferent(splitlt2[ix]))
                dataSendList.Add(new OffsetData(offsetl2 + (i * acreSizeBytes), splitl2[ix].SetArray(Item.SIZE)));
        }

        var layerBytes = new List<byte>(listLayer1.SetArray(Item.SIZE));
        layerBytes.AddRange(listLayer2.SetArray(Item.SIZE));

        ReferenceContainer<float> progressValue = new ReferenceContainer<float>(0f);
        Texture2D itemTex = SpriteBehaviour.ItemToTexture2D(5984, 0, out var _); // large star fragment
        UI_Popup.CurrentInstance.CreateProgressBar("Placing new acres... Go in then out of a building to view changes.", progressValue, itemTex, Vector3.up * 180, null, "Cancel", () => { StopAllCoroutines(); });
        StartCoroutine(writeData(dataSendList.ToArray(), progressValue, () => { field = layerBytes.ToArray(); generateAll(); }));
    }

    IEnumerator writeData(OffsetData[] toSend, ReferenceContainer<float> progress, Action onEnd)
    {
        for (int i = 0; i < toSend.Length; ++i)
        {
            var chunk = toSend[i];
            MapParent.CurrentConnection.WriteBytes(chunk.ToSend, chunk.Offset);
            MapParent.CurrentConnection.WriteBytes(chunk.ToSend, chunk.Offset + (uint)OffsetHelper.BackupSaveDiff);
            float currentProgress = (i + 1) / (float)toSend.Length;
            progress.UpdateValue(currentProgress);
            yield return null;
        }


        onEnd?.Invoke();
        yield return null;
        progress.UpdateValue(1.01f);
        UI_ACItemGrid.LastInstanceOfItemGrid.PlayHappyParticles();
    }

    public static Item[] CloneItemArray(Item[] source)
    {
        Item[] items = new Item[source.Length];
        for (int i = 0; i < source.Length; ++i)
        {
            var bytes = source[i].ToBytesClass();
            items[i] = bytes.ToClass<Item>();
        }
        return items;
    }
}

public static class ItemListExtensions
{
    public static List<List<T>> ChunkBy<T>(this List<T> source, int chunkSize)
    {
        return source
            .Select((x, i) => new { Index = i, Value = x })
            .GroupBy(x => x.Index / chunkSize)
            .Select(x => x.Select(v => v.Value).ToList())
            .ToList();
    }

    public static bool IsDifferent(this List<Item> items, List<Item> toCompare)
    {
        for(int i = 0; i < items.Count; ++i)
        {
            if (items[i].IsDifferentTo(toCompare[i]))
                return true;
        }

        return false;
    }
}
