using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NHSE.Core;
using UnityEngine.UI;
using System.Linq;
using System.Text;
using System;

public class UI_MapItemTile : MonoBehaviour
{
    private Item mainItem;
    private Item extRightItem;
    private Item extDiagItem;
    private Item extDownItem;

    public RawImage Image;
    public RawImage Background;
    public Text OverlayText;

    private bool currentTextureGenerated;

    public void SetItem(Item main, Item right, Item diag, Item down, Color bg)
    {
        // Manually destroy any cached images
        if (Image.texture)
            if (Image.texture != ResourceLoader.GetLeafImage())
            Destroy(Image.texture);

        var isNone = main.IsNone;
        OverlayText.text = string.Empty;
        mainItem = main;
        extRightItem = right;
        extDiagItem = diag;
        extDownItem = down;
        var tex = SpriteBehaviour.ItemToTexture2D(main, out var c);
        Image.texture = tex;
        Background.color = isNone ? bg : c;
        Image.gameObject.SetActive(!isNone);

        if (tex != null)
        {
            if (tex.name.StartsWith("leaf"))
            {
                // default image/no sprites loaded
                OverlayText.text = GetName(main.DisplayItemId);
                Image.gameObject.SetActive(false);
            }
        }
    }

    string GetName(ushort id)
    {
        string toRet = "???";
        if (id < 60_000)
        {
            var items = GameInfo.Strings.ItemDataSource.ToList();
            var item = items.Find((ComboItem x) => x.Value == id);
            toRet = item.Text;
        }
        else
        {
            if (FieldItemList.Items.TryGetValue(id, out var def))
                toRet = AddNewlinesAfterCapitals(def.Name);
        }
        return toRet;
    }

    string AddNewlinesAfterCapitals(string theString)
    {
        StringBuilder builder = new StringBuilder();
        foreach (char c in theString)
        {
            if (Char.IsUpper(c) && builder.Length > 0) builder.Append('\n');
            builder.Append(c);
        }
        return builder.ToString();
    }
}
