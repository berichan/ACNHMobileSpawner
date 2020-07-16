using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UI_SearchItem : MonoBehaviour
{
    private readonly Color internalColor = new Color32(0xDF, 0x80, 0x80, 0xFF);

	public Button SelectionButton;
	public Text SelectionText;
    public RawImage ItemImage;
    public RawImage MenuIconImage;
    public RawImage WarningImage;

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
        Texture2D t2d = SpriteBehaviour.ItemToTexture2D((ushort)itemId, 0, out c, uiFilter);
        ItemImage.texture = t2d;
        ItemImage.color = c;

        // Menu icon
        t2d = MenuSpriteHelper.CurrentInstance.GetIconTexture((ushort)itemId);
        MenuIconImage.texture = t2d;
        MenuIconImage.color = Color.white;

        if (ItemExtensions.IsInternalItem((ushort)itemId))
        {
            ColorBlock cb = SelectionButton.colors;
            cb.normalColor = cb.normalColor * internalColor;
            cb.highlightedColor = cb.highlightedColor * internalColor;
            cb.pressedColor = cb.pressedColor * internalColor;
            cb.disabledColor = cb.disabledColor * internalColor;
            cb.selectedColor = cb.selectedColor * internalColor;
            SelectionButton.colors = cb;

            WarningImage.gameObject.SetActive(true);
        }
        else
            WarningImage.gameObject.SetActive(false);
    }

	public void SetSelection()
	{
		sWindow.SelectItem(UIFilter, ItemId, this);
	}
}
