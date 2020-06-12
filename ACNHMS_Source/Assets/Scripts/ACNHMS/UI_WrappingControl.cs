using NHSE.Core;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UI_WrappingControl : MonoBehaviour
{
	public Toggle WrapToggle;
	public Toggle ShowItemToggle;
	public Dropdown WrapType;
	public Dropdown WrapColor;

	[HideInInspector]
	public int ItemWrap;
	[HideInInspector]
	public int ItemColor;
	[HideInInspector]
	public bool Flag80;

	private string[] wrapTypeNames;

	private string[] wrapColorNames;

	private void Start()
	{
        //IL_004b: Unknown result type (might be due to invalid IL or missing references)
        //IL_0051: Expected O, but got Unknown
        //IL_00ca: Unknown result type (might be due to invalid IL or missing references)
        //IL_00d1: Expected O, but got Unknown
        WrapToggle.onValueChanged.AddListener(delegate
        {
            setWrapped(WrapToggle.isOn);
        });

        wrapTypeNames = Enum.GetNames(typeof(ItemWrapping));
        WrapType.ClearOptions();
        string[] array = wrapTypeNames;
        foreach (string text in array)
        {
            Dropdown.OptionData val = new Dropdown.OptionData();
	        val.text=(text);
	        WrapType.options.Add(val);
        }
        WrapType.onValueChanged.AddListener(delegate
        {
            ChangeItemWrap(WrapType.value);
        });
        WrapType.RefreshShownValue();

        wrapColorNames = Enum.GetNames(typeof(ItemWrappingPaper));
        WrapColor.ClearOptions();
        array = wrapColorNames;
        foreach (string text2 in array)
        {
            Dropdown.OptionData val2 = new Dropdown.OptionData();
            val2.text=(text2);
	        WrapColor.options.Add(val2);
        }
        WrapColor.onValueChanged.AddListener(delegate
        {
	        ChangeItemColor(WrapColor.value);
        });
        WrapColor.RefreshShownValue();

        setWrapped(WrapToggle.isOn);
	}

	private void setWrapped(bool val)
	{
        WrapType.gameObject.SetActive(val);
        ShowItemToggle.gameObject.SetActive(val);
		if (!val)
		{
            WrapColor.gameObject.SetActive(false);
		}
		if (val && WrapType.value == 1)
		{
            WrapColor.gameObject.SetActive(true);
		}
	}

	public void ChangeItemWrap(int nVal)
	{
		ItemWrap = nVal;
		if (ItemWrap == 1)
		{
            WrapColor.gameObject.SetActive(true);
		}
	}

	public void ChangeItemColor(int nVal)
	{
		ItemColor = nVal;
	}
    
}
