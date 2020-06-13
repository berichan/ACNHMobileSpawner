using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UI_SetControl : MonoBehaviour
{
	public InputField FCount;
	public InputField FUses;
	public InputField FFlagZero;
	public InputField FFlagOne;

	public GameObject RootButtons;
	public GameObject RootFields;

	public Dropdown BCount;
	public Dropdown BUses;

	private void Start()
	{
		BCount.onValueChanged.AddListener(delegate
		{
			CompileCountFromBodyFabric();
		});
		BUses.onValueChanged.AddListener(delegate
		{
			CompileCountFromBodyFabric();
		});
		BCount.gameObject.SetActive(false);
		BUses.gameObject.SetActive(false);
	}

	private void Update()
	{
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
		if (!BCount.gameObject.activeSelf && !BUses.gameObject.activeSelf)
		{
			return;
		}
		int num = int.Parse(FCount.text);
		int num2 = -1;
		int num3 = -1;
		List<int> list = new List<int>();
		for (int i = 0; i < BCount.options.Count; i++)
		{
			int num4 = int.Parse(GetUntilOrEmpty(BCount.options[i].text));
			list.Add(num4);
			if (num4 == num)
			{
				num2 = i;
			}
		}
		if (num2 != -1)
		{
			BCount.value=(num2);
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
			if (num5 == num)
			{
				num3 = j;
			}
		}
		if (num3 != -1)
		{
			BCount.value=(0);
			BCount.RefreshShownValue();
			BUses.value=(num3);
			BUses.RefreshShownValue();
			return;
		}
		int num6 = 0;
		for (int k = 0; k < list2.Count; k++)
		{
			if (list2[k] > num6 && list2[k] < num)
			{
				num6 = list2[k];
			}
		}
		BUses.value=list2.IndexOf(num6);
		BUses.RefreshShownValue();
		BCount.value=(num % num6);
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
		FCount.text=(num.ToString());
	}

	public void CreateBody(string[] values)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		if (values.Length == 0)
		{
			FCount.gameObject.SetActive(true);
			BCount.gameObject.SetActive(false);
			return;
		}
		FCount.gameObject.SetActive(false);
		BCount.gameObject.SetActive(true);
		BCount.ClearOptions();
		foreach (string text in values)
		{
			if (text.Length != 0)
			{
				Dropdown.OptionData val = new Dropdown.OptionData();
				val.text =text;
				BCount.options.Add(val);
			}
		}
		BCount.RefreshShownValue();
		CompileCountFromBodyFabric();
	}

	public void CreateFabric(string[] values)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		if (values.Length == 0)
		{
			FUses.gameObject.SetActive(true);
			BUses.gameObject.SetActive(false);
			return;
		}
		FUses.gameObject.SetActive(false);
		BUses.gameObject.SetActive(true);
		BUses.ClearOptions();
		foreach (string text in values)
		{
			if (text.Length != 0)
			{
                Dropdown.OptionData val = new Dropdown.OptionData();
                val.text = text;
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
