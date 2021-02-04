using UnityEngine.UI;
using UnityEngine;
using NHSE.Core;
using System;

public enum FindAndReplaceOptions : byte
{
    MatchItemId,
    MatchItemAndVariation,
    MatchFully,
}

public class UI_MapFindAndReplace : MonoBehaviour
{
    public FindAndReplaceOptions CurrentOption { get; private set; } = FindAndReplaceOptions.MatchItemId;

    private static UI_SearchWindow SearchWindow => UI_SearchWindow.LastLoadedSearchWindow;

    public Text ReplaceItemText, NewItemText;
    public Dropdown MatchMode;

    public readonly Item ReplaceItem = new Item(Item.NONE);
    public readonly Item NewItem = new Item(Item.NONE);

    private bool replaceItemSelected = true;

    // Start is called before the first frame update
    void Start()
    {
        MatchMode.ClearOptions();
        string[] smChoices = Enum.GetNames(typeof(FindAndReplaceOptions));
        foreach (string sm in smChoices)
        {
            Dropdown.OptionData newVal = new Dropdown.OptionData();
            newVal.text = UI_MapItemTile.AddNewlinesAfterCapitals(sm, ' ');
            MatchMode.options.Add(newVal);
        }
        MatchMode.onValueChanged.AddListener(delegate { CurrentOption = (FindAndReplaceOptions)MatchMode.value; });
        MatchMode.value = 0;
        MatchMode.RefreshShownValue();

        SearchWindow.OnNewItemSelected += updateItem;
        SearchWindow.OnReturnSearchWindow += updateItemVals;
        ResetAll();
    }

    public void BringSearchWindowToFront() => SearchWindow.SetAtFront(true, false);
    public void SetReplace(bool replace) => replaceItemSelected = replace;

    private void updateItem(ushort itemId, string itemNameCurrentLanguage)
    {
        Text toChange = replaceItemSelected ? ReplaceItemText : NewItemText;
        toChange.text = itemNameCurrentLanguage;
        updateItemVals();
    }

    private void updateItemVals()
    {
        Item toChangeItem = replaceItemSelected ? ReplaceItem : NewItem;
        SearchWindow.GetAsItem(toChangeItem);
    }

    private void OnEnable()
    {
        ResetAll();
    }

    private void ResetAll()
    {
        ReplaceItem.CopyFrom(Item.NO_ITEM);
        NewItem.CopyFrom(Item.NO_ITEM);
        ReplaceItemText.text = "No item";
        NewItemText.text = "No item";
    }
}
