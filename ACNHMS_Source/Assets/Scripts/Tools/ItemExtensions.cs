using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NHSE.Core;
using System;

public static class ItemExtensions
{
    private static List<ushort> itemsInternal;
    
    public static bool IsInternalItem(this Item i)
    {
        checkInternalList();
        return itemsInternal.Contains(i.ItemId);
    }

    public static bool IsInternalItem(ushort itemid)
    {
        checkInternalList();
        return itemsInternal.Contains(itemid);
    }

    private static void checkInternalList()
    {
        if (itemsInternal == null)
        {
            itemsInternal = new List<ushort>();
            string[] hexes = ResourceLoader.GetInternalHexes().text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string h in hexes)
                itemsInternal.Add(ushort.Parse(h, System.Globalization.NumberStyles.HexNumber));
        }
    }
}
