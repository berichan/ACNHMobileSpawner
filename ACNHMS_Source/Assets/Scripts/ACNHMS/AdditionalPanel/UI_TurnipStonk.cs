using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using NHSE.Core;

public class UI_TurnipStonk : IUI_Additional
{
    public static string TurnipValuesAddress = OffsetHelper.TurnipAddress.ToString("X");
    public static uint CurrentTurnipAddress { get { return StringUtil.GetHexValue(TurnipValuesAddress); } }

    //info
    public Dropdown PatternDropdown;
    public InputField FeverStart;
    //buy
    public InputField BuyPrice;
    //sell
    public InputField MonAM, MonPM, TueAM, TuePM, WedAM, WedPM, ThuAM, ThuPM, FriAM, FriPM, SatAM, SatPM, SunAM, SunPM;
    //setall
    public InputField SetAll;

    //other
    public InputField RAMOffset;

    private TurnipStonk currentStonk;

    // Start is called before the first frame update
    void Start()
    {
        currentStonk = new TurnipStonk();
        PatternDropdown.ClearOptions();
        string[] patChoices = Enum.GetNames(typeof(TurnipPattern));
        foreach (string pt in patChoices)
        {
            Dropdown.OptionData newVal = new Dropdown.OptionData();
            newVal.text = pt;
            PatternDropdown.options.Add(newVal);
        }
        PatternDropdown.RefreshShownValue();

        PatternDropdown.onValueChanged.AddListener(delegate { currentStonk.Pattern = (TurnipPattern)PatternDropdown.value; });
        FeverStart.onValueChanged.AddListener(delegate { currentStonk.FeverStart = uint.Parse(FeverStart.text); });

        BuyPrice.onValueChanged.AddListener(delegate { currentStonk.BuyPrice = uint.Parse(BuyPrice.text); });

        MonAM.onValueChanged.AddListener(delegate { currentStonk.SellMondayAM = uint.Parse(MonAM.text); });
        MonPM.onValueChanged.AddListener(delegate { currentStonk.SellMondayPM = uint.Parse(MonPM.text); });
        TueAM.onValueChanged.AddListener(delegate { currentStonk.SellTuesdayAM = uint.Parse(TueAM.text); });
        TuePM.onValueChanged.AddListener(delegate { currentStonk.SellTuesdayPM = uint.Parse(TuePM.text); });
        WedAM.onValueChanged.AddListener(delegate { currentStonk.SellWednesdayAM = uint.Parse(WedAM.text); });
        WedPM.onValueChanged.AddListener(delegate { currentStonk.SellWednesdayPM = uint.Parse(WedPM.text); });
        ThuAM.onValueChanged.AddListener(delegate { currentStonk.SellThursdayAM = uint.Parse(ThuAM.text); });
        ThuPM.onValueChanged.AddListener(delegate { currentStonk.SellThursdayPM = uint.Parse(ThuPM.text); });
        FriAM.onValueChanged.AddListener(delegate { currentStonk.SellFridayAM = uint.Parse(FriAM.text); });
        FriPM.onValueChanged.AddListener(delegate { currentStonk.SellFridayPM = uint.Parse(FriPM.text); });
        SatAM.onValueChanged.AddListener(delegate { currentStonk.SellSaturdayAM = uint.Parse(SatAM.text); });
        SatPM.onValueChanged.AddListener(delegate { currentStonk.SellSaturdayPM = uint.Parse(SatPM.text); });
        SunAM.onValueChanged.AddListener(delegate { currentStonk.SellSundayAM = uint.Parse(SunAM.text); });
        SunPM.onValueChanged.AddListener(delegate { currentStonk.SellSundayPM = uint.Parse(SunPM.text); });

        RAMOffset.text = TurnipValuesAddress;
        RAMOffset.onValueChanged.AddListener(delegate { TurnipValuesAddress = RAMOffset.text; });

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetTurnipValues()
    {
        try
        {
            byte[] bytes = new byte[TurnipStonk.SIZE];
            bytes = currentStonk.ToBytesClass();
            CurrentConnection.WriteBytes(bytes, CurrentTurnipAddress);

            if (UI_ACItemGrid.LastInstanceOfItemGrid != null)
                UI_ACItemGrid.LastInstanceOfItemGrid.PlayHappyParticles();
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            PopupHelper.CreateError(e.Message, 2f);
        }
    }

    public void GetTurnipValues()
    {
        try
        {
            byte[] bytes = new byte[TurnipStonk.SIZE];
            bytes = CurrentConnection.ReadBytes(CurrentTurnipAddress, TurnipStonk.SIZE);
            currentStonk = bytes.ToClass<TurnipStonk>();

            stonkToUI();
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            PopupHelper.CreateError(e.Message, 2f);
        }
    }

    public void SetAllTurnipValues()
    {
        string nVal = SetAll.text;
        if (string.IsNullOrWhiteSpace(nVal))
            return;

        MonAM.text = MonPM.text = TueAM.text = TuePM.text = WedAM.text = WedPM.text = ThuAM.text = ThuPM.text = FriAM.text = FriPM.text = SatAM.text = SatPM.text = nVal;
    }

    void stonkToUI()
    {
        PatternDropdown.value = (int)currentStonk.Pattern;
        FeverStart.text = currentStonk.FeverStart.ToString();

        BuyPrice.text = currentStonk.BuyPrice.ToString();

        MonAM.text = currentStonk.SellMondayAM.ToString();
        MonPM.text = currentStonk.SellMondayPM.ToString();
        TueAM.text = currentStonk.SellTuesdayAM.ToString();
        TuePM.text = currentStonk.SellTuesdayPM.ToString();
        WedAM.text = currentStonk.SellWednesdayAM.ToString();
        WedPM.text = currentStonk.SellWednesdayPM.ToString();
        ThuAM.text = currentStonk.SellThursdayAM.ToString();
        ThuPM.text = currentStonk.SellThursdayPM.ToString();
        FriAM.text = currentStonk.SellFridayAM.ToString();
        FriPM.text = currentStonk.SellFridayPM.ToString();
        SatAM.text = currentStonk.SellSaturdayAM.ToString();
        SatPM.text = currentStonk.SellSaturdayPM.ToString();
        SunAM.text = currentStonk.SellSundayAM.ToString();
        SunPM.text = currentStonk.SellSundayPM.ToString();
    }
}
