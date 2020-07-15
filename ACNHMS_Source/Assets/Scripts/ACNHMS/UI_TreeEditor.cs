using System.Collections;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using NHSE.Core;

public class UI_TreeEditor : MonoBehaviour
{
    public RawImage Item1, Item2, Item3;
    public ItemIconSelector IconSelect;
    public Toggle IsBearingFruit;

    private string currentSelected = "";

    private Item currentlySelectedItem;
    private Action<Item> endTreeControl;

    private StaticSpriteHelperBase localHSB;
    private StaticSpriteHelperBase cSHB { get {
            if (localHSB == null)
                localHSB = IconSpriteHelper.CurrentInstance;
            return localHSB;
        } }

    void Start()
    {
        IsBearingFruit.onValueChanged.AddListener(delegate { currentlySelectedItem.UseCount = IsBearingFruit.isOn ? (ushort)32 : (ushort)0; });
    }

    public void InitialiseWithItem(Item i, Action<Item> onEnd)
    {
        currentlySelectedItem = i;
        IconSelect.Initialize(cSHB.GetCurrentParser(), UpdateSelectedItem, cSHB);
        endTreeControl = onEnd;
        IsBearingFruit.isOn = currentlySelectedItem.UseCount >= 32;
        Item1.color = Item2.color = Item3.color = Color.clear;

        StartCoroutine(selectOn2Frame());
    }

    IEnumerator selectOn2Frame() // to allow scroll view objects to update
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        IconSelect.SelectItemGlobal(currentlySelectedItem.Count.ToString("X"));
    }

    public void UpdateSelectedItem(string u)
    {
        currentSelected = cSHB.GetCurrentParser().SpritePointerTable.FirstOrDefault(x => x.Value == u).Key;
        currentlySelectedItem.Count = ushort.Parse(currentSelected, System.Globalization.NumberStyles.HexNumber);
        Texture2D t2d = cSHB.GetIconTexture(currentSelected);
        Item1.texture = Item2.texture = Item3.texture = t2d;
        Item1.color = Item2.color = Item3.color = Color.white;
    }

    public void Done()
    {
        endTreeControl(currentlySelectedItem);
    }
}

public static class ItemTreeExtensions
{
    public static bool IsMoneyTree(this Item i)
    {
        return (i.ItemId >= 4436 && i.ItemId <= 4439) || i.ItemId == 4426;
    }
}