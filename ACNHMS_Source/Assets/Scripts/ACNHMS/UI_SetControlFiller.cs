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
	}

	private void Update()
	{
	}

	public void UpdateSelected(int itemIndex, Item item)
	{
		lastItemIndex = itemIndex;
		lastItem = item;
		SetCurrent.GetComponentInChildren<Text>().text=("Set to current \n(" + itemIndex + ")");
		SetFillRow.GetComponentInChildren<Text>().text=("Fill row \n(" + itemIndex / 10 + ")");
	}

	public void FillSelected(int index)
	{
		Item asItem = SearchWindow.GetAsItem(lastItem);
		ItemGrid.SetItemAt(asItem, index, setFocus: true);
	}

	public void FillRow(int row)
	{
	}
}
