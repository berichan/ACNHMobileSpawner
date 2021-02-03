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

    public int ViewId;

    private UI_MapTerrain callback;
    private bool currentTextureGenerated;

    private static bool touchingThisFrame = false;
    private static bool touchingCheckedThisFrame = false;

    private static List<ComboItem> Recipes = null;
    private static List<ComboItem> Fossils;

    public void SetItem(FieldItemBlock block, Color bg, UI_MapTerrain editorCallback)
    {
        if (Recipes == null)
        {
            Recipes = GameInfo.Strings.CreateItemDataSource(RecipeList.Recipes, false);
            Fossils = GameInfo.Strings.CreateItemDataSource(GameLists.Fossils, false);
        }
        // Manually destroy any cached images
        //if (Image.texture)
            //if (Image.texture != ResourceLoader.GetLeafImage())
                //Destroy(Image.texture);

        callback = editorCallback;
        var rootItem = block.SelectedItem;
        var isNone = rootItem.IsNone;
        OverlayText.text = string.Empty;
        item = block;

        Texture2D tex;
        Color c = Color.white;
        if (rootItem.SystemParam == 0x20)
        {
            // use menuicon for fish/bugs that are "placed"
            var kind = ItemInfo.GetItemKind(rootItem);
            if (kind == ItemKind.Kind_Insect || ItemKindExtensions.IsFish(kind))
                tex = MenuSpriteHelper.CurrentInstance.GetIconTexture(rootItem.ItemId);
            else
                tex = SpriteBehaviour.ItemToTexture2D(rootItem, out c);
        }
        else
            tex = SpriteBehaviour.ItemToTexture2D(rootItem, out c);
        Image.texture = tex;
        Background.color = isNone ? bg : c;
        Image.gameObject.SetActive(!isNone);

        if (tex != null)
        {
            if (tex.name.StartsWith("leaf")) // all defaults are "leaf"
            {
                // default image/no sprites loaded/field item
                OverlayText.text = GetName(rootItem.DisplayItemId);
                Image.gameObject.SetActive(false);
            }
        }

        if (rootItem.ItemId >= 60_000 && !isNone)
            OverlayText.text = GetName(rootItem.DisplayItemId);
        if (rootItem.ItemId == Item.MessageBottle || rootItem.ItemId == Item.DIYRecipe || rootItem.ItemId == Item.MessageBottleEgg)
            OverlayText.text = Recipes.Find(x => x.Value == rootItem.Count).Text;
        if (rootItem.ItemId == UI_SearchWindow.FOSSILITEM)
            OverlayText.text = Fossils.Find(x => x.Value == rootItem.Count).Text;

        ViewId = rootItem.ItemId;
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

    public static string AddNewlinesAfterCapitals(string theString, char newLineChar = '\n')
    {
        StringBuilder builder = new StringBuilder();
        foreach (char c in theString)
        {
            if (Char.IsUpper(c) && builder.Length > 0) builder.Append(newLineChar);
            builder.Append(c);
        }
        return builder.ToString();
    }

    public void HandleTap()
    {
        switch (callback.CurrentSelectMode)
        {
            case TerrainSelectMode.Custom: item.UpdateItem(UI_MapTerrain.ReferenceItem()); SetItem(item, Background.color, callback); break;
            case TerrainSelectMode.Drop: item.UpdateItem(UI_MapTerrain.ReferenceItem(0x20)); SetItem(item, Background.color, callback); break;
            case TerrainSelectMode.Place: item.UpdateItem(UI_MapTerrain.ReferenceItem(0x0)); SetItem(item, Background.color, callback); break;
            case TerrainSelectMode.Delete: item.UpdateItem(new Item(65534)); SetItem(item, Background.color, callback); break;
            case TerrainSelectMode.Load: UI_SearchWindow.LastLoadedSearchWindow.LoadItem(item.SelectedItem); break;
        }

        if (callback.CurrentSelectMode != TerrainSelectMode.Load)
        {
            callback.UpdateLayerImage();
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
