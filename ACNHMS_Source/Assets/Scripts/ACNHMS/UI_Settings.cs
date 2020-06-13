using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using NHSE.Core;

public enum StringSearchMode 
{
    Contains = 0,
    StartsWith = 1
}

public class UI_Settings : MonoBehaviour
{
    public const string SEARCHMODEKEY = "SMODEKEY";
    public const string ITEMLANGMODEKEY = "ITEMLMKEY";

    public Dropdown LanguageField;
    public Dropdown SearchMode;
    public InputField Offset;

    // Start is called before the first frame update
    void Start()
    {
        SearchMode.ClearOptions();
        string[] smChoices = Enum.GetNames(typeof(StringSearchMode));
        foreach(string sm in smChoices)
        {
            Dropdown.OptionData newVal = new Dropdown.OptionData();
            newVal.text = sm;
            SearchMode.options.Add(newVal);
        }
        SearchMode.value = (int)GetSearchMode();
        SearchMode.RefreshShownValue();

        LanguageField.ClearOptions();
        string[] langChoices = GameLanguage.AvailableLanguageCodes;
        foreach(string lm in langChoices)
        {
            Dropdown.OptionData newVal = new Dropdown.OptionData();
            newVal.text = lm;
            LanguageField.options.Add(newVal);
        }
        LanguageField.value = GetLanguage();
        LanguageField.RefreshShownValue();

        Offset.text = SysBotController.CurrentOffset;

        SearchMode.onValueChanged.AddListener(delegate { setSearchMode((StringSearchMode)SearchMode.value); });
        LanguageField.onValueChanged.AddListener(delegate { SetLanguage(LanguageField.value); });
        Offset.onValueChanged.AddListener(delegate { SysBotController.CurrentOffset = Offset.text; });

        SetLanguage(GetLanguage());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static StringSearchMode GetSearchMode(StringSearchMode ssmDefault = StringSearchMode.Contains)
    {
        return (StringSearchMode)PlayerPrefs.GetInt(SEARCHMODEKEY, (int)ssmDefault);
    }

    private static void setSearchMode(StringSearchMode ssm)
    {
        PlayerPrefs.SetInt(SEARCHMODEKEY, (int)ssm);
    }

    public static int GetLanguage(int defLang = 0)
    {
        return PlayerPrefs.GetInt(ITEMLANGMODEKEY, defLang);
    }

    public static void SetLanguage(int nLang)
    {
        PlayerPrefs.SetInt(ITEMLANGMODEKEY, nLang);
        GameInfo.SetLanguage2Char(nLang);
    }
}
