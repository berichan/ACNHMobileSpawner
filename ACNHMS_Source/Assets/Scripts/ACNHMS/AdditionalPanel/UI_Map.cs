using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NHSE.Core;
using System;

public enum RemovalItem
{
    Weed = 0,
    TreeBranch,
    Fence,
    Bush,
    Flower,
    Tree,
    Stone,
    Wood,
    RockMaterial,
    Internal
}

public class UI_Map : IUI_Additional
{
    private const int YieldCount = 3; // yield for a frame every x loops
    private const int FieldItemLayerSize = MapGrid.MapTileCount32x32 * Item.SIZE;
    public static string MapAddress = OffsetHelper.FieldItemStart.ToString("X"); 
    public static uint CurrentMapAddress { get { return StringUtil.GetHexValue(MapAddress); } }

    public Item CurrentlyPlacingItem;

    public InputField RAMOffset;
    public Dropdown RemoveItemMode;
    public Text ButtonLabel;
    public Button KeepAliveButton;

    private RemovalItem currentRemovalItem = RemovalItem.Weed;

    private bool mapFunctionRunning = false;

    private Item[] layer1Dump, layer2Dump;
    private uint indexOfItemBeingProcessed = 0;

    // Start is called before the first frame update
    void Start()
    {
        RAMOffset.text = MapAddress;
        RAMOffset.onValueChanged.AddListener(delegate { MapAddress = RAMOffset.text; });

        ButtonLabel.text = $"Remove every {currentRemovalItem.ToString().ToLower()}";

        RemoveItemMode.ClearOptions();
        string[] riChoices = Enum.GetNames(typeof(RemovalItem));
        foreach (string ri in riChoices)
        {
            Dropdown.OptionData newVal = new Dropdown.OptionData();
            newVal.text = ri;
            RemoveItemMode.options.Add(newVal);
        }

        RemoveItemMode.onValueChanged.AddListener(delegate { currentRemovalItem = (RemovalItem)RemoveItemMode.value; ButtonLabel.text = $"Remove every {currentRemovalItem.ToString().ToLower()}"; });
        RemoveItemMode.value = 0;
        RemoveItemMode.RefreshShownValue();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlaceNMT()
    {
        Item nmt = new Item(5851);
        nmt.Count = 9;
        CurrentlyPlacingItem = nmt;

        Func<Item, bool> itemPlacer = new Func<Item, bool>(PlaceItemIfEmpty);
        ReferenceContainer<float> itemFunctionValue = new ReferenceContainer<float>(0f);
        Texture2D itemTex = SpriteBehaviour.ItemToTexture2D(5851, 0, out var _);
        UI_Popup.CurrentInstance.CreateProgressBar("Placing items, please run around but don't place anything on your island...", itemFunctionValue, itemTex, Vector3.up * 180, null, "Cancel", () => { CancelCurrentFunction(); });

        StartCoroutine(functionTiles(itemPlacer, itemFunctionValue));
    }

    public void KeepItemsAlive()
    {
        Func<Item, bool> alive1 = new Func<Item, bool>(x => CopyItemFromDumpIfRequired(x, 0));
        Func<Item, bool> alive2 = new Func<Item, bool>(x => CopyItemFromDumpIfRequired(x, 1));
        ReferenceContainer<float> itemFunctionValue = new ReferenceContainer<float>(0f);
        StartCoroutine(KeepAliveLoop(alive1, itemFunctionValue, alive2));
    }

    IEnumerator KeepAliveLoop(Func<Item, bool> processItem, ReferenceContainer<float> progress, Func<Item, bool> processItemLayer2 = null)
    {
        bool exitToken = false;
        int refreshRate = UI_Settings.GetThreadSleepTime();
        UI_Popup.CurrentInstance.CreatePopupChoice($"Your map is being continuously refreshed at minimum {refreshRate}ms per acre. You may decrease this value in settings, but the tradeoff may be stability.\r\nTo see replenished items, go in & out of a building.", "Stop refreshing", () => { exitToken = true; });
        while (!exitToken)
        {
            StartCoroutine(functionTiles(processItem, progress, processItemLayer2));
            mapFunctionRunning = true;
            while (mapFunctionRunning && !exitToken)
                yield return new WaitForSeconds(1f);
        }

        CancelCurrentFunction();
    }

    public void DumpTwoLayers()
    {
        layer1Dump = new Item[MapGrid.MapTileCount32x32]; layer2Dump = new Item[MapGrid.MapTileCount32x32];
        Func<Item, bool> dumpL1 = new Func<Item, bool>(x => DumpItemToLayer(x, 0));
        Func<Item, bool> dumpL2 = new Func<Item, bool>(x => DumpItemToLayer(x, 1));
        ReferenceContainer<float> itemFunctionValue = new ReferenceContainer<float>(0f);
        Texture2D itemTex = SpriteBehaviour.ItemToTexture2D(8574, 0, out var _);

        UI_Popup.CurrentInstance.CreateProgressBar("Getting item layout template, please run around but don't place anything on your island...", itemFunctionValue, itemTex, Vector3.up * 180, null, "Cancel", () => { CancelCurrentFunction(); KeepAliveButton.interactable = false; });

        StartCoroutine(functionTiles(dumpL1, itemFunctionValue, dumpL2));
        KeepAliveButton.interactable = true;
    }

    public void ClearItems()
    {
        Func<Item, bool> deleter = new Func<Item, bool>(x => DeleteType(x, currentRemovalItem));
        ReferenceContainer<float> itemFunctionValue = new ReferenceContainer<float>(0f);
        Texture2D imgToShow;
        string progressString;
        Vector3 rot;
        switch (currentRemovalItem)
        {
            case RemovalItem.Bush:
            case RemovalItem.Flower:
            case RemovalItem.Weed:
                imgToShow = SpriteBehaviour.ItemToTexture2D(338, 0, out var _); // red lawnmover
                progressString = "Mowing lawn, please run around but don't place anything on your island...";
                rot = Vector3.up * 180;
                break;
            case RemovalItem.Fence:
            case RemovalItem.Tree:
            case RemovalItem.Stone:
            case RemovalItem.TreeBranch:
                imgToShow = SpriteBehaviour.ItemToTexture2D(9617, 0, out var _); // golden axe
                progressString = $"Chopping up every {currentRemovalItem.ToString().ToLower()}, please run around but don't place anything on your island...";
                rot = Vector3.up * 180;
                break;
            default:
                imgToShow = ResourceLoader.GetAngryIsabelle(); // \_/
                progressString = "Isabelle is cleaning up your mess, please run around but don't place anything on your island...";
                rot = Vector3.up * 180;
                break;
        }

        UI_Popup.CurrentInstance.CreateProgressBar(progressString, itemFunctionValue, imgToShow, rot, null, "Cancel", () => { CancelCurrentFunction(); });

        StartCoroutine(functionTiles(deleter, itemFunctionValue));
    }

    public void ClearItemsOld()
    {
        Func<Item, bool> weedDeleter = new Func<Item, bool>(DeleteWeed);
        ReferenceContainer<float> itemFunctionValue = new ReferenceContainer<float>(0f);
        Texture2D lawnMower = SpriteBehaviour.ItemToTexture2D(338, 0, out var _);
        UI_Popup.CurrentInstance.CreateProgressBar("Mowing lawn, please run around but don't place anything on your island...", itemFunctionValue, lawnMower, Vector3.up * 180, null, "Cancel", () => { CancelCurrentFunction(); });

        StartCoroutine(functionTiles(weedDeleter, itemFunctionValue));
    }

    public void CancelCurrentFunction()
    {
        StopAllCoroutines();
    }

    IEnumerator functionTiles(Func<Item,bool> processItem, ReferenceContainer<float> progress, Func<Item,bool> processItemLayer2 = null)
    {
        mapFunctionRunning = true;
        int maxtransferSizeRemainder = CurrentConnection.MaximumTransferSize % Item.SIZE;
        int maxtransferSize = CurrentConnection.MaximumTransferSize - maxtransferSizeRemainder;

        uint fieldItemsStart1 = CurrentMapAddress;
        uint fieldItemsStart2 = CurrentMapAddress + FieldItemLayerSize;
        uint itemsPerFrame = (uint)maxtransferSize / Item.SIZE;
        uint lastTileIndex = 0;
        
        int byteCheckCount = 0;
        int yieldCheckCount = 0;
        while (lastTileIndex < MapGrid.MapTileCount32x32)
        {
            int thisFrameL1Proc = 0;
            int thisFrameL2Proc = 0;
            int bytesToCheck = (int)Math.Min((int)itemsPerFrame * Item.SIZE, FieldItemLayerSize - (lastTileIndex * Item.SIZE));
            byte[] bytesLayer1 = CurrentConnection.ReadBytes(fieldItemsStart1 + (lastTileIndex * Item.SIZE), bytesToCheck);
            byte[] bytesLayer2 = CurrentConnection.ReadBytes(fieldItemsStart2 + (lastTileIndex * Item.SIZE), bytesToCheck);
            Item[] itemLayer1 = Item.GetArray(bytesLayer1);
            Item[] itemLayer2 = Item.GetArray(bytesLayer2);
            for (uint j = 0; j < itemLayer1.Length; ++j)
            {
                Item i = itemLayer1[j];
                indexOfItemBeingProcessed = lastTileIndex + j;
                if (processItem(i))
                    thisFrameL1Proc++;
            }
            for (uint j = 0; j < itemLayer2.Length; ++j)
            {
                Item i = itemLayer2[j];
                indexOfItemBeingProcessed = lastTileIndex + j;
                if (processItemLayer2 == null)
                {
                    if (processItem(i))
                        thisFrameL2Proc++;
                }
                else
                {
                    if (processItemLayer2(i))
                        thisFrameL2Proc++;
                }
            }
            if (thisFrameL1Proc > 0)
            {
                byte[] nl1 = itemLayer1.SetArray(Item.SIZE);
                CurrentConnection.WriteBytes(nl1, fieldItemsStart1 + (lastTileIndex * Item.SIZE));
                CurrentConnection.WriteBytes(nl1, fieldItemsStart1 + (lastTileIndex * Item.SIZE) + (uint)OffsetHelper.BackupSaveDiff);
            }
            if (thisFrameL2Proc > 0)
            {
                byte[] nl2 = itemLayer2.SetArray(Item.SIZE);
                CurrentConnection.WriteBytes(nl2, fieldItemsStart2 + (lastTileIndex * Item.SIZE));
                CurrentConnection.WriteBytes(nl2, fieldItemsStart2 + (lastTileIndex * Item.SIZE) + (uint)OffsetHelper.BackupSaveDiff);
            }
            byteCheckCount += bytesToCheck;
            //Debug.Log(string.Format("Currently read byte {0}/{1}.", byteCheckCount, FieldItemLayerSize));
            lastTileIndex += itemsPerFrame;
            yieldCheckCount++;

            if (yieldCheckCount % YieldCount == 0)
            {
                float progressNow = (float)byteCheckCount / FieldItemLayerSize;
                progress.UpdateValue(progressNow);
                yield return null;
            }
        }

        progress.UpdateValue(0.99f);
        yield return null;
        progress.UpdateValue(1.01f);
        mapFunctionRunning = false;
    }

    private bool DeleteType(Item i, RemovalItem toRemove)
    {
        ushort toSearch = i.ItemId;
        if (i.ItemId == Item.EXTENSION)
            toSearch = i.ExtensionItemId;
        var removable = toSearch.GetRemovalItemType();
        if (removable.HasValue)
        {
            if (removable.Value == toRemove)
            {
                i.Delete();
                return true;
            }
        }

        return false;
    }

    private bool DeleteWeed(Item i)
    {
        FieldItemList.Items.TryGetValue(i.ItemId, out var def);
        if (def != null)
        {
            if (def.Kind.IsWeed())
            {
                i.Delete();
                return true;
            }
        }
        if (i.ItemId == Item.EXTENSION)
        {
            FieldItemList.Items.TryGetValue(i.ExtensionItemId, out var def2);
            if (def2 != null)
            {
                if (def2.Kind.IsWeed())
                {
                    i.Delete();
                    return true;
                }
            }
        }
        return false;
    }

    private bool PlaceItemIfEmpty(Item i)
    {
        if (i.ItemId == Item.NONE)
        {
            i.CopyFrom(CurrentlyPlacingItem);
            return true;
        }
        return false;
    }

    private bool DumpItemToLayer(Item i, byte layer)
    {
        if (layer == 0)
            layer1Dump[(int)indexOfItemBeingProcessed] = i;
        else
            layer2Dump[(int)indexOfItemBeingProcessed] = i;

        return false; // we did not edit the item
    }

    private bool CopyItemFromDumpIfRequired(Item i, byte layer, bool onlyReplaceEmpties = true)
    {
        if (onlyReplaceEmpties && i.ItemId != Item.NONE)
            return false;

        Item[] toUse;
        if (layer == 0)
            toUse = layer1Dump;
        else
            toUse = layer2Dump;

        var templateItem = toUse[(int)indexOfItemBeingProcessed];
        if (templateItem.ItemId != Item.EXTENSION && templateItem.ItemId >= 60_000)
            return false;
        if (i == templateItem)
            return false;

        i.CopyFrom(templateItem);
        Debug.Log($"{templateItem.ItemId} copied.");
        return true;
    }

    //testing
    private bool ReplaceTreeItemWithFurniture(Item i)
    {
        FieldItemList.Items.TryGetValue(i.ItemId, out var def);
        if (def != null)
        {
            if (def.Kind == FieldItemKind.PltTreeOak)
            {
                //var bytes = i.ToBytesClass();
                Debug.Log(string.Format("Found {0}, Count: {1}", i.ItemId, i.Count));
                i.Count = 2;

                return true;
            }
        }
        return false;
    }
}

public static class RemovalItemExt
{
    public static RemovalItem? GetRemovalItemType(this ushort i)
    {
        if (i == 2500)
            return RemovalItem.TreeBranch;
        if (ItemExtensions.IsInternalItem(i))
            return RemovalItem.Internal;
        if (i >= 2767 && i <= 2769)
            return RemovalItem.Wood;
        if (i == 2511 || i == 2502 || i == 3090)
            return RemovalItem.RockMaterial;

        FieldItemList.Items.TryGetValue(i, out var def);
        if (def != null)
        {
            if (def.Kind.IsWeed())
                return RemovalItem.Weed;
            if (def.Kind.IsBush())
                return RemovalItem.Bush;
            if (def.Kind.IsFence())
                return RemovalItem.Fence;
            if (def.Kind.IsFlower())
                return RemovalItem.Flower;
            if (def.Kind.IsStone())
                return RemovalItem.Stone;
            if (def.Kind.IsTree())
                return RemovalItem.Tree;
        }

        return null;
    }
}
