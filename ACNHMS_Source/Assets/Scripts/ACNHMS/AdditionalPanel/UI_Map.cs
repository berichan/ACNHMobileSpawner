using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NHSE.Core;
using System;
using System.Text;
using System.Linq;

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
    SpoiledTurnip,
    Internal
}

public class RefresherLogUnit
{
    public DateTime StartTime;
    public Dictionary<string, int> NameJoinCountDic;

    public RefresherLogUnit() { StartTime = DateTime.Now; NameJoinCountDic = new Dictionary<string, int>(); }

    public void IncrementJoin(string PlayerName)
    {
        if (NameJoinCountDic.ContainsKey(PlayerName))
            NameJoinCountDic[PlayerName] += 1;
        else
            NameJoinCountDic.Add(PlayerName, 1);
    }

    public override string ToString()
    {
        StringBuilder s = new StringBuilder($"[Refresher started: {StartTime.ToString("dddd, dd MMMM yyyy HH:mm:ss")}]\r\n");
        foreach (var kvp in NameJoinCountDic)
            s.Append($"{kvp.Key} x{kvp.Value}\r\n");
        return s.ToString();
    }
}

public class UI_Map : IUI_Additional
{
    private const int YieldCount = 3; // yield for a frame every x loops
    private const int FieldItemLayerSize = MapGrid.MapTileCount32x32 * Item.SIZE;
    public static string MapAddress = OffsetHelper.FieldItemStart.ToString("X"); 
    public static uint CurrentMapAddress { get { return StringUtil.GetHexValue(MapAddress); } }
    public static string ArriverAddress = OffsetHelper.ArriverNameLocAddress.ToString("X");
    public static uint CurrentArriverAddress { get { return StringUtil.GetHexValue(ArriverAddress); } }

    public Item CurrentlyPlacingItem;

    public InputField RAMOffset;
    public Dropdown RemoveItemMode;
    public Text ButtonLabel;
    public Toggle Layer2Affect;

    // Map refresher
    public Button KeepAliveButton, ResendButton;
    public Text LastRefresherTime;
    public Text VisitorLog;

    private List<RefresherLogUnit> Logs = new List<RefresherLogUnit>();
    private RefresherLogUnit CurrentLog;

    private RemovalItem currentRemovalItem = RemovalItem.Weed;

    private bool mapFunctionRunning = false;

    private Item[] layer1Dump, layer2Dump;
    private uint indexOfItemBeingProcessed = 0;
    private string lastPlayerName = string.Empty;

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
        SetLastComingVisitorName(string.Empty); // refresh last arrival
        CurrentLog = new RefresherLogUnit();
        Logs.Add(CurrentLog);
        StartCoroutine(KeepAliveLoop(alive1, itemFunctionValue, alive2));
    }

    public void UpdateVisitationLog()
    {
        string s = string.Empty;
        foreach (var log in Logs)
            s += log.ToString() + "\r\n";

        VisitorLog.text = s;
    }

    public void SaveVisitationLog()
    {
        UI_NFSOACNHHandler.LastInstanceOfNFSO.SaveFile($"VisitorLog_{DateTime.Now:yyyyMMddHHmmss}.txt", Encoding.Unicode.GetBytes(VisitorLog.text));
    }

    IEnumerator KeepAliveLoop(Func<Item, bool> processItem, ReferenceContainer<float> progress, Func<Item, bool> processItemLayer2 = null)
    {
        bool exitToken = false;
        int refreshRate = UI_Settings.GetThreadSleepTime();
        UI_Popup.CurrentInstance.CreatePopupChoice($"Your map is being continuously refreshed at minimum {refreshRate}ms per acre. You may decrease this value in settings, but the tradeoff may be stability.\r\nTo see replenished items, go in & out of a building.", "Stop refreshing", () => { exitToken = true; });
        while (!exitToken)
        {
            LastRefresherTime.text = $"Last refresher run:\r\n{DateTime.Now:dddd, dd MMMM yyyy HH:mm:ss}";
            StartCoroutine(functionTiles(processItem, progress, processItemLayer2, true));
            mapFunctionRunning = true;
            while (mapFunctionRunning && !exitToken)
                yield return new WaitForSeconds(1f);
        }

        CancelCurrentFunction();
    }

    public void ResendOriginalLayerBytes()
    {
        UI_Popup.CurrentInstance.CreatePopupMessage(0.001f, "Resending original items, this may help if the items on your floor have shifted. This may take a few minutes... \r\nPlease disconnect from the internet, enter a building and stay in there for the duration of this function.", () =>
        {
            var bytes = layer1Dump.SetArray(Item.SIZE);
            CurrentConnection.WriteBytes(bytes, CurrentMapAddress);
            CurrentConnection.WriteBytes(bytes, CurrentMapAddress + (uint)OffsetHelper.BackupSaveDiff);
            if (Layer2Affect.isOn && layer2Dump != null)
            {
                bytes = layer2Dump.SetArray(Item.SIZE);
                CurrentConnection.WriteBytes(bytes, CurrentMapAddress + FieldItemLayerSize);
                CurrentConnection.WriteBytes(bytes, CurrentMapAddress + FieldItemLayerSize + (uint)OffsetHelper.BackupSaveDiff);
            }
        });
    }

    public void DumpTwoLayers()
    {
        layer1Dump = new Item[MapGrid.MapTileCount32x32]; layer2Dump = new Item[MapGrid.MapTileCount32x32];
        Func<Item, bool> dumpL1 = new Func<Item, bool>(x => DumpItemToLayer(x, 0));
        Func<Item, bool> dumpL2 = new Func<Item, bool>(x => DumpItemToLayer(x, 1));
        ReferenceContainer<float> itemFunctionValue = new ReferenceContainer<float>(0f);
        Texture2D itemTex = SpriteBehaviour.ItemToTexture2D(8574, 0, out var _);

        UI_Popup.CurrentInstance.CreateProgressBar("Getting item layout template, please run around but don't place anything on your island...", itemFunctionValue, itemTex, Vector3.up * 180, null, "Cancel", () => { CancelCurrentFunction(); KeepAliveButton.interactable = ResendButton.interactable = false; });

        StartCoroutine(functionTiles(dumpL1, itemFunctionValue, dumpL2));
        KeepAliveButton.interactable = true;
        ResendButton.interactable = true;
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

    IEnumerator functionTiles(Func<Item,bool> processItem, ReferenceContainer<float> progress, Func<Item,bool> processItemLayer2 = null, bool checkArrivals = false)
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
            byte[] bytesLayer2 = Layer2Affect.isOn ? CurrentConnection.ReadBytes(fieldItemsStart2 + (lastTileIndex * Item.SIZE), bytesToCheck) : null;
            Item[] itemLayer1 = Item.GetArray(bytesLayer1);
            Item[] itemLayer2 = Layer2Affect.isOn ? Item.GetArray(bytesLayer2) : null;
            for (uint j = 0; j < itemLayer1.Length; ++j)
            {
                Item i = itemLayer1[j];
                indexOfItemBeingProcessed = lastTileIndex + j;
                if (processItem(i))
                    thisFrameL1Proc++;
            }
            if (Layer2Affect.isOn)
            {
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
            }
            // Check if someone is arriving
            if (checkArrivals)
            {
                if (IsSomeoneArriving(out string coming))
                {
                    CurrentLog.IncrementJoin(coming);
                    UI_Popup.CurrentInstance.UpdateText($"{coming} is arriving!\r\nThe auto-refresher is temporarily paused while their airplane animation occurs.");
                    yield return new WaitForSeconds(74.5f);
                    SetLastComingVisitorName(string.Empty);
                    UI_Popup.CurrentInstance.ResetText();
                }
            }
            if (thisFrameL1Proc > 0)
            {
                byte[] nl1 = itemLayer1.SetArray(Item.SIZE);
                CurrentConnection.WriteBytes(nl1, fieldItemsStart1 + (lastTileIndex * Item.SIZE));
                CurrentConnection.WriteBytes(nl1, fieldItemsStart1 + (lastTileIndex * Item.SIZE) + (uint)OffsetHelper.BackupSaveDiff);
            }
            if (thisFrameL2Proc > 0 && Layer2Affect.isOn)
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

    private bool IsSomeoneArriving(out string coming)
    {
        coming = GetLastComingVisitorName();
        if (coming == string.Empty || string.IsNullOrWhiteSpace(coming))
            return false;
        bool different = coming != lastPlayerName;
        lastPlayerName = coming;
        return different;
    }

    private string GetLastComingVisitorName()
    {
        var nameBytes = CurrentConnection.ReadBytes(CurrentArriverAddress, 0xC);
        var nameBuf = Encoding.Unicode.GetString(nameBytes).TrimEnd('\0');
        return nameBuf;
    }

    private void SetLastComingVisitorName(string val)
    {
        byte[] toSendBytes = new byte[0xC];
        var bytes = Encoding.Unicode.GetBytes(val);
        if (bytes.Length > 0xC)
        {
            throw new Exception("Username limit reached!");
        }
        for (int i = 0; i < bytes.Length; ++i)
            toSendBytes[i] = bytes[i];
        CurrentConnection.WriteBytes(toSendBytes, CurrentArriverAddress);
        lastPlayerName = val;
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

    private void checkNames()
    {
        StartCoroutine(doThing(0.5f, int.MaxValue, () => { }));
    }

    IEnumerator doThing(float seconds, int numLoops, Action toDo)
    {
        for (int i = 0; i < numLoops; ++i)
        {
            toDo();
            yield return new WaitForSeconds(seconds);
        }
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
        if (i == 2642)
            return RemovalItem.SpoiledTurnip;

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
