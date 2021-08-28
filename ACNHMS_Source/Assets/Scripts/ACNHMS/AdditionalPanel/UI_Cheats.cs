using NHSE.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class UI_Cheats : IUI_Additional
{
    public InputField CheatField;
    public Toggle EmptySpacesOnly;
    public Dropdown EnteredValueParseMode;
    public Dropdown SaveMode;

    public void LoadItems()
    {
        try
        {
            LoadCheats();
        }
        catch (Exception e)
        {
            string msg = e.Message;
            if (e.InnerException != null)
                msg += $": {e.InnerException.Message}";
            UI_Popup.CurrentInstance.CreatePopupChoice(msg, "OK", () => { }, null, "Copy to clipboard", () => { GUIUtility.systemCopyBuffer = msg; });
        }
    }

    public void LoadCheats()
    {
        string parseable = CheatField.text;
        ItemArrayEditor<Item> ItemArray;
        if (SaveMode.value == 0)
            ItemArray = new ItemArrayEditor<Item>(UI_ACItemGrid.LastInstanceOfItemGrid.Items);
        else
            ItemArray = new ItemArrayEditor<Item>(GetNoItems(40));

        byte[] allItemBytes = null;

        if (EnteredValueParseMode.value == 0) // 0 == cheat
        {
            if (parseable != "")
            {
                var bytes = ItemCheatCode.ReadCode(parseable);
                ItemArray = new ItemArrayEditor<Item>(UI_ACItemGrid.LastInstanceOfItemGrid.Items);
                if (bytes.Length % ItemArray.ItemSize == 0)
                {
                    ItemArray.ImportItemDataX(bytes, EmptySpacesOnly.isOn, 0);
                }
                allItemBytes = bytes;
            }
        }
        else // 1 == hexdata
        {
            if (parseable != "")
            {
                List<byte[]> loadedItems = new List<byte[]>();
                var split = parseable.Split(new[] { " ", "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < split.Length; ++i)
                {
                    var itemText = split[i];
                    var item = itemText.Trim();
                    var asBytes = GetBytesFromString(item);
                    loadedItems.Add(asBytes);
                }

                while (loadedItems.Count < 40)
                    loadedItems.Add(Item.NO_ITEM.ToBytesClass());

                allItemBytes = new byte[loadedItems.Count * Item.SIZE];
                for (int i = 0; i < loadedItems.Count; ++i)
                    Array.Copy(loadedItems[i], 0, allItemBytes, i * Item.SIZE, Item.SIZE);

                ItemArray.ImportItemDataX(allItemBytes, EmptySpacesOnly.isOn, 0);
            }
        }


        if (SaveMode.value == 0) // 0 == inventory
        {
            if (UI_ACItemGrid.LastInstanceOfItemGrid == null)
                return;

            for (int i = 0; i < 40; ++i)
            {
                UI_ACItemGrid.LastInstanceOfItemGrid.SetItemAt(ItemArray.Items[i], i, i == (ItemArray.Items.Count - 1));
            }
        }
        else
        {
            if (allItemBytes != null)
                UI_NFSOACNHHandler.LastInstanceOfNFSO.SaveFile("NHIItems.nhi", allItemBytes);
            else
                throw new Exception("No valid items.");
        }
    }

    // Semi-adapted from SysBot.AnimalCrossing

    private static byte[] GetBytesFromString(string text)
    {
        if (!ulong.TryParse(text, Input.GetKey(KeyCode.LeftShift) ? NumberStyles.Integer : NumberStyles.AllowHexSpecifier, CultureInfo.CurrentCulture, out var val))
            return Item.NONE.ToBytes();
        return BitConverter.GetBytes(val);
    }

    private static Item CreateItem(byte[] convert, int i, bool placeFloor)
    {
        Item item;
        try
        {
            item = convert.ToClass<Item>();
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to convert item {i}: {ex.Message}");
        }

        if (!placeFloor)
            if (convert.Length != Item.SIZE)
                throw new Exception($"Unsupported item: {i}");

        if (placeFloor)
            item.SystemParam = 0x20;
        return item;
    }

    private static Item[] GetNoItems(int count)
    {
        Item[] toRet = new Item[count];
        for (int i = 0; i < count; ++i) 
        {
            toRet[i] = new Item();
        }
        return toRet;
    }
}
