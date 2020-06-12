using NHSE.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UI_SetControlFiller : MonoBehaviour
{
	public UI_ACItemGrid ItemGrid;
	public UI_SearchWindow SearchWindow;

	public Button SetCurrent;
	public Button SetFillRow;
	public Button SetFillAll;
	public Button SetFillVariations;
    public Button DeleteItem;

	private int lastItemIndex = -1;
	private Item lastItem;

	private void Start()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		SetCurrent.onClick.AddListener(delegate
		{
			FillSelected(lastItemIndex);
		});
		SetFillRow.onClick.AddListener(delegate
		{
			FillRow(lastItemIndex / 10);
		});
        SetFillAll.onClick.AddListener(delegate
        {
            FillAll();
        });
        DeleteItem.onClick.AddListener(delegate
        {
            DeleteItemAt(lastItemIndex);
        });
    }

	private void Update()
	{
	}

	public void UpdateSelected(int itemIndex, Item item)
	{
		lastItemIndex = itemIndex;
		lastItem = item;
        SetCurrent.GetComponentInChildren<Text>().text = "Set to current \n(" + itemIndex + ")";
        SetFillRow.GetComponentInChildren<Text>().text = "Fill row \n(" + itemIndex / 10 + ")";
        DeleteItem.GetComponentInChildren<Text>().text = "Delete item \n(" + itemIndex + ")";
    }

	public void FillSelected(int index)
	{
        if (SearchWindow.IsNoItemMode)
            return;

        Item asItem = SearchWindow.GetAsItem(lastItem);
		ItemGrid.SetItemAt(asItem, index, setFocus: true);
	}

	public void FillRow(int row)
	{
        if (SearchWindow.IsNoItemMode)
            return;

        int start = row * 10;
        for (int i = start; i < start + 10; ++i)
        {
            lastItem = ItemGrid.GetItemAt(i);
            FillSelected(i);
        }
	}

    public void FillAll()
    {
        if (SearchWindow.IsNoItemMode)
            return;

        for (int i = 0; i < 40; ++i)
        {
            lastItem = ItemGrid.GetItemAt(i);
            FillSelected(i);
        }
    }

    public void DeleteItemAt(int index)
    {
        lastItem = ItemGrid.GetItemAt(index);
        lastItem.Delete();
        ItemGrid.SetItemAt(lastItem, index, true);
    }
}
