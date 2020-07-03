using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UI_SearchItem : MonoBehaviour
{
	public Button SelectionButton;

	public Text SelectionText;

    public RawImage ItemImage;

	[HideInInspector]
	public string RawValue;

	[HideInInspector]
	public string ProcessedValue;

	[HideInInspector]
	public int ItemId;

	[HideInInspector]
	public ItemFilter UIFilter;

	private UI_SearchWindow sWindow;

	public void Start()
	{
	}

	public void InitialiseFor(string val, string contains, int itemId, ItemFilter uiFilter, UI_SearchWindow parentWindow)
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Expected O, but got Unknown
		ItemId = itemId;
		UIFilter = uiFilter;
		sWindow = parentWindow;
		RawValue = val;
		ProcessedValue = RawValue;
		int num = val.ToLower().IndexOf(contains);
		int startIndex = num + contains.Length;
		ProcessedValue = ProcessedValue.Insert(startIndex, "</color>");
		ProcessedValue = ProcessedValue.Insert(num, "<color=cyan>");
		SelectionText.text=(ProcessedValue);
		SelectionButton.onClick.RemoveAllListeners();
		SelectionButton.onClick.AddListener(delegate
		{
			SetSelection();
		});

        // Sprite image
        Color c;
        Texture2D t2d = SpriteBehaviour.ItemToTexture2D((ushort)itemId, 0, out c);
        ItemImage.texture = t2d;
        ItemImage.color = c;
	}

	public void SetSelection()
	{
		sWindow.SelectItem(UIFilter, ItemId, this);
	}
}
