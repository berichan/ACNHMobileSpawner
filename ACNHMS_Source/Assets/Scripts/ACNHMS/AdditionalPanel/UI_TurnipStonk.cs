using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NHSE.Core;

public class UI_TurnipStonk : IUI_Additional
{
    public static string TurnipValuesAddress = "AB2B0B38";
    public static uint CurrentTurnipAddress { get { return StringUtil.GetHexValue(TurnipValuesAddress); } }

    private TurnipStonk currentStonk;

    // Start is called before the first frame update
    void Start()
    {
        //CurrentConnection = UI_ACItemGrid.LastInstanceOfItemGrid.CurrentSysBot;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GetTurnipValues()
    {
        CurrentConnection = UI_ACItemGrid.LastInstanceOfItemGrid.CurrentSysBot;
        byte[] bytes = new byte[TurnipStonk.SIZE];
        bytes = CurrentConnection.ReadBytes(CurrentTurnipAddress, TurnipStonk.SIZE);
        currentStonk = bytes.ToClass<TurnipStonk>();

        //ok so this is hacky as hell, but I think garbage collection is cleaning this up badly which causes turnip price loss and random null pointer exs being thrown so 
        bytes = new byte[1];
    }
}
