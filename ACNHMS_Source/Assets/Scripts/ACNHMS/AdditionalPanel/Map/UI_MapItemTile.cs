using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NHSE.Core;
using UnityEngine.UI;
using System.Linq;
using System.Text;
using System;
using UnityEngine.EventSystems;

public class UI_MapItemTile : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    private FieldItemBlock item;

    public RawImage Image;
    public RawImage Background;
    public Text OverlayText;

    private UI_MapTerrain callback;
    private bool currentTextureGenerated;

    private static bool touchingThisFrame = false;
    private static bool touchingCheckedThisFrame = false;

    public void SetItem(FieldItemBlock block, Color bg, UI_MapTerrain editorCallback)
    {
        // Manually destroy any cached images
        if (Image.texture)
            if (Image.texture != ResourceLoader.GetLeafImage())
                Destroy(Image.texture);

        callback = editorCallback;
        var isNone = block.SelectedItem.IsNone;
        OverlayText.text = string.Empty;
        item = block;
        var tex = SpriteBehaviour.ItemToTexture2D(block.SelectedItem, out var c);
        Image.texture = tex;
        Background.color = isNone ? bg : c;
        Image.gameObject.SetActive(!isNone);

        if (tex != null)
        {
            if (tex.name.StartsWith("leaf")) // all defaults are "leaf"
            {
                // default image/no sprites loaded
                OverlayText.text = GetName(block.SelectedItem.DisplayItemId);
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

    public void HandleTap()
    {
        switch (callback.CurrentSelectMode)
        {
            case TerrainSelectMode.Place: item.UpdateItem(UI_MapTerrain.ReferenceItem); SetItem(item, Background.color, callback); break;
            case TerrainSelectMode.Delete: item.UpdateItem(Item.NO_ITEM); break;
            case TerrainSelectMode.Load: UI_SearchWindow.LastLoadedSearchWindow.LoadItem(item.SelectedItem); break;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (touchingThisFrame)
            HandleTap();
    }
    public void OnPointerDown(PointerEventData eventData)
    {
            HandleTap();
    }

    // not ideal
    void Update()
    {
        if (!touchingCheckedThisFrame)
        {
            touchingThisFrame = (Input.touchCount > 0) || Input.GetMouseButton(0);
            touchingCheckedThisFrame = true;
        }
    }

    void LateUpdate()
    {
        touchingCheckedThisFrame = false;
    }
}
