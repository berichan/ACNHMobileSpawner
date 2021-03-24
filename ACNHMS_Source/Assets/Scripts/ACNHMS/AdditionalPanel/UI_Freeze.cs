using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NHSE.Core;
using NHSE.Injection;
using System;
using System.Globalization;

public class UI_Freeze : IUI_Additional
{
    private const string SBBUrl = "https://github.com/olliz0r/sys-botbase/releases";

    uint invOffset => (uint)(SysBotController.CurrentOffsetFirstPlayerUInt + PocketInjector.shift + ((uint)OffsetHelper.PlayerSize * UI_Settings.GetPlayerIndex()));

    const int villagerFlagStart = 0x12678;
    const int mapChunkCount = 64;
    const int mapSizeBytes = MapGrid.MapTileCount32x32 * Item.SIZE;
    const string villagerString = "villager flags";
    const string invString = "inventory";
    const string mapString = "map";
    const string moneyString = "wallet";
    const string turnipString = "turnip prices";

    public Text CountLabel;
    public Text VersionLabel;
    public Text FreezeMillisecondLabel;
    public InputField OffsetField;
    public InputField SizeField;
    public Slider FreezeRateSlider;

    public GameObject Blocker;
    
    public void GoToDownloadPage() => Application.OpenURL(SBBUrl);

    private void Start()
    {
        FreezeRateSlider.onValueChanged.AddListener(delegate { FreezeMillisecondLabel.text = FreezeRateSlider.value.ToString(); });
    }

    public void CheckUsability()
    {
        var ver = getVersion().TrimEnd('\0').TrimEnd('\n');
        VersionLabel.text = ver;
        var verLower = ver.ToLower();
        var verDouble = double.TryParse(verLower, out var version);
        if (verDouble && version > 1.699)
        {
            Blocker.SetActive(false);
        }
        else
        {
            PopupHelper.CreateError("Installed version of botbase is outdated. Please use a later version.", 3f);
            return;
        }
        UpdateFreezeCount();
        OffsetField.text = $"{invOffset:X8}";
        SizeField.text = PocketInjector.size.ToString();
    }

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

    public void PauseAll() => CurrentConnection.FreezePause();
    public void UnpauseAll() => CurrentConnection.FreezeUnpause();

    public void SetFreezerate()
    {
        CurrentConnection.Configure("freezeRate", ((int)FreezeRateSlider.value).ToString());
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

    public void SendMoneyFreeze()
    {
        StartCoroutine(createFreezes(new uint[1] { (uint)OffsetHelper.WalletAddress }, UI_MoneyMiles.ENCRYPTIONSIZE, moneyString));
    }

    public void SendTurnipFreeze()
    {
        StartCoroutine(createFreezes(new uint[1] { (uint)OffsetHelper.TurnipAddress }, TurnipStonk.SIZE, turnipString));
    }

    public void SendCustomFreeze()
    {
        bool success = uint.TryParse(OffsetField.text, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var res);
        if (!success)
        {
            PopupHelper.CreateError($"Cannot parse {OffsetField.text} as a hexadecimal value.", 3f);
            return;
        }
        success = int.TryParse(SizeField.text, out var size);
        if (!success)
        {
            PopupHelper.CreateError($"Cannot parse {SizeField.text}.", 3f);
            return;
        }
        StartCoroutine(createFreezes(new uint[1] { res }, size, $"Custom: {SizeField.text}@{OffsetField.text}"));
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

    public void ClearMoneyFreeze()
    {
        CurrentConnection.UnFreezeBytes((uint)OffsetHelper.WalletAddress);
        UpdateFreezeCount();
    }

    public void ClearTurnipFreeze()
    {
        CurrentConnection.UnFreezeBytes((uint)OffsetHelper.TurnipAddress);
        UpdateFreezeCount();
    }

    public void ClearCustomFreeze()
    {
        bool success = uint.TryParse(OffsetField.text, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var res);
        if (!success)
        {
            PopupHelper.CreateError($"Cannot parse {OffsetField.text} as a hexadecimal value.", 3f);
            return;
        }
        CurrentConnection.UnFreezeBytes(res);
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

    private string getVersion() => System.Text.Encoding.UTF8.GetString(CurrentConnection.GetVersion());
    
}
