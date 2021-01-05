using NHSE.Core;
using UnityEngine;
using UnityEngine.UI;

public class UI_MapEditor : MonoBehaviour
{
    private static UI_SearchWindow SearchWindow => UI_SearchWindow.LastLoadedSearchWindow;

    private static Item refItem = Item.NO_ITEM;
    public static Item ReferenceItem { get => SearchWindow.GetAsItem(null); }

    public UI_MapTerrain TerrainControl;
    public Text CurrentLoadedItemName;

    // Start is called before the first frame update
    void Start()
    {
        SearchWindow.OnNewItemSelected += updateItem;
    }

    public void BringSearchWindowToFront() => SearchWindow.SetAtFront(true, false);

    private void updateItem(ushort id, string itemName)
    {
        CurrentLoadedItemName.text = itemName;
    }
}
