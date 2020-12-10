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

    private RemovalItem currentRemovalItem = RemovalItem.Weed;

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

    IEnumerator functionTiles(Func<Item,bool> processItem, ReferenceContainer<float> progress)
    {
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
            foreach (Item i in itemLayer1)
                if (processItem(i))
                    thisFrameL1Proc++;
            foreach (Item i in itemLayer2)
                if (processItem(i))
                    thisFrameL2Proc++;
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
