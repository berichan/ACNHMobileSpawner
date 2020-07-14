using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NHSE.Core;

public class UI_TreeEditor : MonoBehaviour
{
    public RawImage Item1, Item2, Item3;
    public ItemIconSelector IconSelect;
    public Toggle IsBearingFruit;

    ushort currentSelected = 0;

    Item currentlySelectedItem;

    void Start()
    {
        IsBearingFruit.onValueChanged.AddListener(delegate { currentlySelectedItem.UseCount = IsBearingFruit.isOn ? (ushort)32 : (ushort)0; });

        Item i = new Item(4439);
        i.Count = 0;
        i.UseCount = 32;
        InitialiseWithItem(i);
    }

    public void InitialiseWithItem(Item i)
    {
        currentlySelectedItem = i;
        IconSelect.Initialize(MenuItemSpriteHelper.CurrentParser, UpdateSelectedItem);
        
        IsBearingFruit.isOn = i.UseCount >= 32;

        StartCoroutine(selectOn2Frame());
    }

    IEnumerator selectOn2Frame() // to allow scroll view objects to update
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        IconSelect.SelectItemGlobal(currentlySelectedItem.Count);
    }

    public void UpdateSelectedItem(ushort u)
    {
        currentSelected = u;
        currentlySelectedItem.Count = u;
        Texture2D t2d = MenuItemSpriteHelper.GetIconTexture(u);
        Item1.texture = Item2.texture = Item3.texture = t2d;
    }

    public void Done()
    {

    }
}

public static class ItemTreeExtensions
{
    public static bool IsMoneyTree(this Item i)
    {
        return (i.ItemId > 4436 && i.ItemId < 4439) || i.ItemId == 4426;
    }
}