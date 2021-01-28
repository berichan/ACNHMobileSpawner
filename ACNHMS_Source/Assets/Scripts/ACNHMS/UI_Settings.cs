using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using NHSE.Core;
using System.IO;

public enum StringSearchMode 
{
    Contains = 0,
    StartsWith = 1
}

public enum InjectionProtocol
{
    Sysbot = 0,
    UsbBot = 1
}

public class UI_Settings : MonoBehaviour
{
    public const string SEARCHMODEKEY = "SMODEKEY";
    public const string ITEMLANGMODEKEY = "ITEMLMKEY";
    public const string VALIDATADATAKEY = "VKEY";
    public const string INJMODEKEY = "INJKEY";
    public const string PLAYERINDEXKEY = "PINDKEY";
    public const string THREADSLEEPKEY = "TSLKEY";
    public const string PREFIXKEY = "PRFXKEY";
    public const string CATALOGKEY = "CTLOGUERKEY";

    public static string[] VillagerPlayerNames;

    public Dropdown LanguageField;
    public Dropdown SearchMode;
    public Dropdown InjectionMode;
    public Dropdown WhichPlayer;
    public InputField Offset;
    public Toggle ValidataData;
    public Button FetchNamesButton;
    public InputField ThreadSleepTime;
    public InputField PrefixSBAC;
    public Toggle CatalogueToggle;

    public Text[] PlayerNamesToChange; // Various places that need player name

    private bool fetchedPlayersFromRam = false;

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
        ThreadSleepTime.text = GetThreadSleepTime().ToString();
        PrefixSBAC.text = GetPrefix().ToString();
        ValidataData.isOn = GetValidateData();
        CatalogueToggle.isOn = GetCatalogueMode();

#if PLATFORM_ANDROID || UNITY_STANDALONE || UNITY_EDITOR
        InjectionMode.ClearOptions();
        string[] injChoices = Enum.GetNames(typeof(InjectionProtocol));
        foreach (string insj in injChoices)
        {
            Dropdown.OptionData newVal = new Dropdown.OptionData();
            newVal.text = insj;
            InjectionMode.options.Add(newVal);
        }
        InjectionMode.value = (int)GetInjectionProtocol();
        InjectionMode.RefreshShownValue();
        InjectionMode.onValueChanged.AddListener(delegate {
            SetInjectionProtocol((InjectionProtocol)InjectionMode.value);
            if (UI_Sysbot.LastLoadedUI_Sysbot != null)
                UI_Sysbot.LastLoadedUI_Sysbot.SetInjectionProtocol((InjectionProtocol)InjectionMode.value);
        });
#else
        InjectionMode.gameObject.SetActive(false);
#endif

        SearchMode.onValueChanged.AddListener(delegate { setSearchMode((StringSearchMode)SearchMode.value); });
        LanguageField.onValueChanged.AddListener(delegate { SetLanguage(LanguageField.value); });
        Offset.onValueChanged.AddListener(delegate { SysBotController.CurrentOffset = Offset.text; });
        ThreadSleepTime.onValueChanged.AddListener(delegate { SetThreadSleepTime(int.Parse(ThreadSleepTime.text)); });
        PrefixSBAC.onValueChanged.AddListener(delegate { SetPrefix(PrefixSBAC.text); });
        ValidataData.onValueChanged.AddListener(delegate { SetValidateData(ValidataData.isOn); });
        CatalogueToggle.onValueChanged.AddListener(delegate { SetCatalogueMode(CatalogueToggle.isOn); });

        // player index
        string[] choices = new string[8];
        for (int i = 0; i < 8; ++i)
            choices[i] = string.Format("Player {0}", (char)((uint)'A' + i)); // 'A' + i
        generatePlayerIndexList(choices, GetPlayerIndex());

        SetLanguage(GetLanguage());
    }

    // Update is called once per frame
    float counter = 1; //hacky but check every second, OnEnable won't work here! 1 so it check immediately
    void Update()
    {
        counter += Time.deltaTime;

        if (counter > 1)
        {
            var rw = UI_ACItemGrid.LastInstanceOfItemGrid?.GetCurrentlyActiveReadWriter();
            FetchNamesButton.gameObject.SetActive(rw != null);
            counter = 0;
        }
    }

    public static bool GetValidateData(bool defVal = true)
    {
        int ret = PlayerPrefs.GetInt(VALIDATADATAKEY, defVal ? 1 : 0);
        bool toRet = ret == 1 ? true : false;
        return toRet;
    }

    public static void SetValidateData(bool nVal)
    {
        PlayerPrefs.SetInt(VALIDATADATAKEY, nVal ? 1 : 0);
    }

    public static InjectionProtocol GetInjectionProtocol(InjectionProtocol imDefault = InjectionProtocol.Sysbot)
    {
        return (InjectionProtocol)PlayerPrefs.GetInt(INJMODEKEY, (int)imDefault);
    }

    public static void SetInjectionProtocol(InjectionProtocol injp)
    {
        PlayerPrefs.SetInt(INJMODEKEY, (int)injp);
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

    public static int GetThreadSleepTime(int defVal = 100)
    {
        return PlayerPrefs.GetInt(THREADSLEEPKEY, defVal);
    }

    public static void SetThreadSleepTime(int nVal)
    {
        PlayerPrefs.SetInt(THREADSLEEPKEY, nVal);
    }

    public static char GetPrefix(char def = '$')
    {
        return PlayerPrefs.GetString(PREFIXKEY, def.ToString()).ToCharArray()[0];
    }

    public static void SetPrefix(string nVal)
    {
        if (nVal.Length < 1)
            return;

        PlayerPrefs.SetString(PREFIXKEY, nVal);
    }

    public static bool GetCatalogueMode(bool defVal = false)
    {
        int ret = PlayerPrefs.GetInt(CATALOGKEY, defVal ? 1 : 0);
        bool toRet = ret == 1 ? true : false;
        return toRet;
    }

    public static void SetCatalogueMode(bool nVal)
    {
        PlayerPrefs.SetInt(CATALOGKEY, nVal ? 1 : 0);
        if (SysBotACOrderMode.CurrentInstance != null)
            SysBotACOrderMode.CurrentInstance.Execute(!nVal);
    }

    // player index
    public static int GetPlayerIndex(int defPlayer = 0) => PlayerPrefs.GetInt(PLAYERINDEXKEY, defPlayer);

    public static void SetPlayerIndex(int nVal) => PlayerPrefs.SetInt(PLAYERINDEXKEY, nVal);

    public void FetchPlayerNames()
    {
        var rw = UI_ACItemGrid.LastInstanceOfItemGrid.GetCurrentlyActiveReadWriter();
        string[] toPlace;
        if (rw != null)
            toPlace = UI_Player.FetchPlayerNames(rw);
        else
            return;

        generatePlayerIndexList(toPlace, GetPlayerIndex());
        fetchedPlayersFromRam = true;
    }

    private void generatePlayerIndexList(string[] values, int select = 0)
    {
        VillagerPlayerNames = values;
        WhichPlayer.onValueChanged.RemoveAllListeners();
        WhichPlayer.ClearOptions();
        foreach (string sm in values)
        {
            Dropdown.OptionData newVal = new Dropdown.OptionData();
            newVal.text = sm;
            WhichPlayer.options.Add(newVal);
        }
        WhichPlayer.value = Mathf.Min(WhichPlayer.options.Count-1, select);
        WhichPlayer.RefreshShownValue();
        WhichPlayer.onValueChanged.AddListener(delegate {
            SetPlayerIndex(WhichPlayer.value);
            if (PlayerNamesToChange != null)
                foreach (var pn in PlayerNamesToChange)
                    pn.text = VillagerPlayerNames[GetPlayerIndex()];
        });

        if (PlayerNamesToChange != null)
            foreach (var pn in PlayerNamesToChange)
                pn.text = VillagerPlayerNames[GetPlayerIndex()];
    }

    //clear all
    public void AskDeleteEverything()
    {
        UI_Popup.CurrentInstance.CreatePopupChoice("Are you sure you want to clear all cached data and settings? This action is irreversible.", "No", () => { }, Color.red, "Yes, delete it!", () => { deleteAll(); });
    }

    private void deleteAll()
    {
        PlayerPrefs.DeleteAll();
        Directory.Delete(Application.persistentDataPath, true);
        UI_Popup.CurrentInstance.CreatePopupChoice("All cached data deleted. Please restart ACNHMS", "OK", () => { Environment.Exit(-1); });
    }
}
