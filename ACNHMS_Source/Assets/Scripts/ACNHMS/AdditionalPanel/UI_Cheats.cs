using NHSE.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Cheats : IUI_Additional
{

    public InputField CheatField;

    public void LoadCheats()
    {
        if (UI_ACItemGrid.LastInstanceOfItemGrid == null)
            return;
        string parseable = CheatField.text;
        ItemArrayEditor<Item> ItemArray = new ItemArrayEditor<Item>(UI_ACItemGrid.LastInstanceOfItemGrid.Items);
        if (parseable != "")
        {
            var bytes = ItemCheatCode.ReadCode(parseable);
            if (bytes.Length % ItemArray.ItemSize == 0)
            {
                ItemArray.ImportItemDataX(bytes, true, 0);
            }
        }

        for (int i = 0; i < ItemArray.Items.Count; ++i)
        {
            UI_ACItemGrid.LastInstanceOfItemGrid.SetItemAt(ItemArray.Items[i], i, i == (ItemArray.Items.Count - 1));
        }
    }
}
