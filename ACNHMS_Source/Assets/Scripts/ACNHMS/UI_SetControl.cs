using NHSE.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UI_SetControl : MonoBehaviour
{
    private int MAXSTACKSTAPSNEEDED = 2;
    private float MAXSTACKSECONDSALIVE = 1f;

    public InputField FCount;
	public InputField FUses;
	public InputField FFlagZero;
	public InputField FFlagOne;

	public GameObject RootButtons;
	public GameObject RootFields;

	public Dropdown BCount;
	public Dropdown BUses;

    public Text MaxCountTapsText;
    public Button SpawnVariationsButton;

    private bool inited = false;

    private float maxStackIntervalTimer = -1;
    private int maxStackTapCount = 0;

	private Vector3 bCountStartPos;
	private Vector3 bCountFencePos;
	private bool fenceMode = false;

    public static int CurrentVariationCount = 0;

    private void Start()
	{
		bCountStartPos = BCount.transform.position;
		bCountFencePos = BUses.transform.position;
		BCount.onValueChanged.AddListener(delegate
		{
			CompileCountFromBodyFabric();
            if (UI_SearchWindow.LastLoadedSearchWindow != null)
                UI_SearchWindow.LastLoadedSearchWindow.UpdateSprite();
		});
		BUses.onValueChanged.AddListener(delegate
		{
			CompileCountFromBodyFabric();
            if (UI_SearchWindow.LastLoadedSearchWindow != null)
                UI_SearchWindow.LastLoadedSearchWindow.UpdateSprite();
        });
		BCount.gameObject.SetActive(false);
		BUses.gameObject.SetActive(false);

        inited = true;
	}

    public void IncrementTapCount()
    {
        Item refItem = new Item();
        refItem = UI_SearchWindow.LastLoadedSearchWindow.GetAsItem(refItem);
        if (refItem.ItemId == Item.DIYRecipe)
            return;
        if (ItemInfo.TryGetMaxStackCount(refItem, out _))
        {
            maxStackIntervalTimer = MAXSTACKSECONDSALIVE;
            maxStackTapCount++;
        }
    }

	private void Update()
	{
        if (maxStackIntervalTimer > 0)
        {
            maxStackIntervalTimer -= Time.deltaTime;
            MaxCountTapsText.gameObject.SetActive(true);
#if UNITY_STANDALONE
            var gesture = "clicks";
#else
            var gesture = "swipes";
#endif
            MaxCountTapsText.text = string.Format("Max stack: {0} {1}", MAXSTACKSTAPSNEEDED - maxStackTapCount, gesture);
            if (maxStackTapCount >= MAXSTACKSTAPSNEEDED)
            {
                MaxStack();
                maxStackIntervalTimer = -1;
                maxStackTapCount = 0;
            }
        }
        else
        {
            MaxCountTapsText.gameObject.SetActive(false);
            maxStackIntervalTimer = -1;
            maxStackTapCount = 0;
        }
    }

    private void MaxStack()
    {
        Item refItem = new Item();
        refItem = UI_SearchWindow.LastLoadedSearchWindow.GetAsItem(refItem);
        if (ItemInfo.TryGetMaxStackCount(refItem, out var max))
            FCount.text = (max - 1).ToString();
    }

	public void InitNumbers()
	{
		InputField fCount = FCount;
		InputField fUses = FUses;
		InputField fFlagZero = FFlagZero;
		string text;
		FFlagOne.text=(text = 0.ToString());
		string text2;
		fFlagZero.text=(text2 = text);
		string text3;
		fUses.text=(text3 = text2);
		fCount.text=(text3);
	}

	public void CompileBodyFabricFromCount() // probably unreadable now due to il rebuild, refactor (not a simple modulo function because some item bodies/fabrics are invalid, so need to be built from string)
	{
		if (!BCount.gameObject.activeInHierarchy && !BUses.gameObject.activeInHierarchy)
		{
			return;
		}
		int variation = fenceMode ? int.Parse(FUses.text) : int.Parse(FCount.text);
		int selectedVariation = -1;
		int selectedFabricVariation = -1;
		List<int> list = new List<int>();
        if (BCount.gameObject.activeInHierarchy) // if off, this doesn't have any body values
        {
            for (int i = 0; i < BCount.options.Count; i++)
            {
                int num4 = int.Parse(GetUntilOrEmpty(BCount.options[i].text));
                list.Add(num4);
                if (num4 == variation)
                {
                    selectedVariation = i;
				}
            }
        }
		if (selectedVariation != -1)
		{
			BCount.value=selectedVariation;
			BCount.RefreshShownValue();
		}
		if (!BUses.gameObject.activeSelf)
		{
			return;
		}
		List<int> list2 = new List<int>();
		for (int j = 0; j < BUses.options.Count; j++)
		{
			int num5 = int.Parse(GetUntilOrEmpty(BUses.options[j].text));
			list2.Add(num5);
			if (num5 == variation)
			{
				selectedFabricVariation = j;
			}
		}
		if (selectedFabricVariation != -1)
		{
			BCount.value=0;
			BCount.RefreshShownValue();
			BUses.value=(selectedFabricVariation);
			BUses.RefreshShownValue();
			return;
		}
		int num6 = 0;
		for (int k = 0; k < list2.Count; k++)
		{
			if (list2[k] > num6 && list2[k] < variation)
			{
				num6 = list2[k];
			}
		}
		BUses.value=list2.IndexOf(num6);
		BUses.RefreshShownValue();
        if (num6 == 0)
            BCount.value = variation;
        else
		    BCount.value=(variation % num6);
		BCount.RefreshShownValue();
	}

	public void CompileCountFromBodyFabric()
	{
		int num = 0;
		int result = 0;
		if (int.TryParse(GetUntilOrEmpty(BCount.captionText.text), out result))
		{
			num += result;
		}
		if (BUses.gameObject.activeSelf && int.TryParse(GetUntilOrEmpty(BUses.captionText.text), out result))
		{
			num += result;
		}

		if (!fenceMode)
			FCount.text = num.ToString();
		else
			FUses.text = num.ToString();
	}

	public void CreateBody(string[] values, bool fence)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		fenceMode = fence;
		if (values.Length == 0)
		{
			FCount.gameObject.SetActive(true);
			BCount.gameObject.SetActive(false);
			return;
		}
        values = values.TruncateEndInvalidValues();

		if (!fenceMode)
			FCount.gameObject.SetActive(false);
		else
		{
			FCount.gameObject.SetActive(true);
			FUses.gameObject.SetActive(false);
		}
		
		BCount.gameObject.SetActive(true);
		BCount.ClearOptions();
		foreach (string text in values)
		{
			if (text.Length != 0)
			{
				Dropdown.OptionData val = new Dropdown.OptionData();
				val.text = text.ClearInvalidText();
				BCount.options.Add(val);
			}
		}
		BCount.RefreshShownValue();
		CompileCountFromBodyFabric();

        CurrentVariationCount = BCount.options.Count;
        SpawnVariationsButton.gameObject.SetActive(true);

		BCount.transform.position = fenceMode ? bCountFencePos : bCountStartPos;
	}

	public void CreateFabric(string[] values)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		if (values.Length == 0 || values.IsInvalidFabricArray())
		{
			if (!fenceMode)
				FUses.gameObject.SetActive(true);
			BUses.gameObject.SetActive(false);
			return;
		}
        values = values.TruncateEndInvalidValues();
		FUses.gameObject.SetActive(false);
		BUses.gameObject.SetActive(true);
		BUses.ClearOptions();
		foreach (string text in values)
		{
			if (text.Length != 0)
			{
                Dropdown.OptionData val = new Dropdown.OptionData();
                val.text = text.ClearInvalidText() ;
                BUses.options.Add(val);
			}
		}
		BUses.RefreshShownValue();
		CompileCountFromBodyFabric();
	}

	public string GetUntilOrEmpty(string text, string stopAt = "=")
	{
		if (!string.IsNullOrWhiteSpace(text))
		{
			int num = text.IndexOf(stopAt, StringComparison.Ordinal);
			if (num > 0)
			{
				return text.Substring(0, num);
			}
		}
		return string.Empty;
	}
}

public static class InvalidRemakeStringsUtil
{
    // some string functions to clear out invalids
    public static bool IsInvalidFabricArray(this string[] fa)
    {
        foreach (string f in fa)
            if (!(f.EndsWith("=" + ItemRemakeInfo.InvalidCheck) || f == string.Empty))
                return false;
        return true;
    }

    public static string[] TruncateEndInvalidValues(this string[] sa)
    {
        List<string> ls = new List<string>(sa);
        for (int i = ls.Count-1; i > 0; --i)
        {
            if (ls[i].EndsWith("=" + ItemRemakeInfo.InvalidCheck) || ls[i] == string.Empty)
                ls.RemoveAt(i);
            else
                break;
        }
        if (ls.Count > 0)
            ls[0] = ls[0].Replace("=" + ItemRemakeInfo.InvalidCheck, string.Empty);
        return ls.ToArray();
    }

    public static string ClearInvalidText(this string s) => s.Replace(string.Format("({0})", ItemRemakeInfo.InvalidCheck), string.Empty);
}
