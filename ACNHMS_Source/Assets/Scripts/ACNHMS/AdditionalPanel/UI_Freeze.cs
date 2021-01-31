using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NHSE.Core;
using NHSE.Injection;

public class UI_Freeze : IUI_Additional
{
    private const string SBBUrl = "https://github.com/berichan/sys-botbase/releases";

    const uint invOffset = (uint)((uint)OffsetHelper.InventoryOffset + PocketInjector.shift);

    const int villagerFlagStart = 0x1267c;
    const int mapChunkCount = 64;
    const int mapSizeBytes = (MapGrid.MapTileCount32x32 * Item.SIZE);
    const string villagerString = "villager flags";
    const string invString = "inventory";
    const string mapString = "map";

    public Text CountLabel;
    
    public void GoToDownloadPage() => Application.OpenURL(SBBUrl);

    public void UpdateFreezeCount()
    {
        byte ct = CurrentConnection.GetFreezeCount();
        CountLabel.text = $"{ct}/255";
    }

    public void UnFreezeAll()
    {
        CurrentConnection.UnfreezeAll();
        UpdateFreezeCount();
    }

    public void SendVillagerFlagFreezes()
    {
        uint[] offsets = getOffsets((uint)OffsetHelper.VillagerAddress, villagerFlagStart, Villager2.SIZE, 10);
        StartCoroutine(createFreezes(offsets, 0x100, villagerString));
    }

    public void SendInventoryFreeze()
    {
        StartCoroutine(createFreezes(new uint[1] { invOffset }, PocketInjector.size, invString));
    }

    public void SendMapFreeze()
    {
        var chunkSize = mapSizeBytes / mapChunkCount;
        uint[] offsets = getUnsafeOffsetsByChunkCount((uint)OffsetHelper.FieldItemStart, mapSizeBytes, mapChunkCount);
        StartCoroutine(createFreezes(offsets, chunkSize, mapString));
    }

    public void ClearVillagerFreezes()
    {
        uint[] offsets = getOffsets((uint)OffsetHelper.VillagerAddress, villagerFlagStart, Villager2.SIZE, 10);
        foreach (var o in offsets)
            CurrentConnection.UnFreezeBytes(o);
        UpdateFreezeCount();
    }

    public void ClearInventoryFreeze()
    {
        CurrentConnection.UnFreezeBytes(invOffset);
        UpdateFreezeCount();
    }

    public void ClearMapFreeze()
    {
        uint[] offsets = getUnsafeOffsetsByChunkCount((uint)OffsetHelper.FieldItemStart, mapSizeBytes, mapChunkCount);
        foreach (var o in offsets)
            CurrentConnection.UnFreezeBytes(o);
        UpdateFreezeCount();
    }

    IEnumerator createFreezes(uint[] offsets, int size, string creating)
    {
        int count = offsets.Length;
        List<byte[]> chunks = new List<byte[]>();

        UI_Popup.CurrentInstance.CreatePopupMessage(0.001f, $"Fetching {creating}", () => { });
        yield return null;
        for (int i = 0; i < count; ++i)
            chunks.Add(CurrentConnection.ReadBytes(offsets[i], size));

        UI_Popup.CurrentInstance.CreatePopupMessage(0.001f, $"Creating freezers for {creating}", () => { });
        yield return null;
        for (int i = 0; i < chunks.Count; ++i)
            CurrentConnection.FreezeBytes(chunks[i], offsets[i]);

        UI_Popup.CurrentInstance.CreatePopupMessage(2f, $"Successfully created freezers for {creating}!", () => { });
        UpdateFreezeCount();
    }

    private static uint[] getOffsets(uint startOffset, uint startData, uint size, uint count)
    {
        var offsets = new uint[count];
        for (uint i = 0; i < count; ++i)
            offsets[i] = (startOffset + startData) + (size * i);
        return offsets;
    }

    private static uint[] getUnsafeOffsetsByChunkCount(uint startOffset, uint size, uint chunkCount)
    {
        var offsets = new uint[chunkCount];
        var chunkSize = size / chunkCount;
        for (uint i = 0; i < chunkCount; ++i)
            offsets[i] = startOffset + (i * chunkSize);
        return offsets;
    }
}
