using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NHSE.Core;
using System;

public class UI_Map : IUI_Additional
{
    private const int YieldCount = 3; // yield for a frame every x loops
    private const int FieldItemLayerSize = MapGrid.MapTileCount32x32 * Item.SIZE;
    public static string MapAddress = OffsetHelper.FieldItemStart.ToString("X"); // has storage a bit after it in ram ABA52760 
    public static uint CurrentMapAddress { get { return StringUtil.GetHexValue(MapAddress); } }

    public Item CurrentlyPlacingItem;

    // Start is called before the first frame update
    void Start()
    {
        
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
            }
            if (thisFrameL2Proc > 0)
            {
                byte[] nl2 = itemLayer2.SetArray(Item.SIZE);
                CurrentConnection.WriteBytes(nl2, fieldItemsStart2 + (lastTileIndex * Item.SIZE));
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
}
