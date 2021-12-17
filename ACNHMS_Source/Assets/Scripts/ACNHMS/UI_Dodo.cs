using NHSE.Injection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class UI_Dodo : MonoBehaviour
{
    private IRAMReadWriter Connection => UI_ACItemGrid.LastInstanceOfItemGrid.GetCurrentlyActiveReadWriter();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void FetchShowDodo()
    {
        try
        {
            tryFetchDodo();
        }
        catch (Exception e)
        {
            PopupHelper.CreateError(e.Message, 3);
        }
    }

    private void tryFetchDodo()
    {
        bool isConnected = Connection.ReadBytes(OffsetHelper.OnlineSessionAddress, 1)[0] == 1;
        if (!isConnected)
        {
            PopupHelper.CreateError("Your gates are not currently open.", 3);
            return;
        }

        var bytesDodo = Connection.ReadBytes(OffsetHelper.DodoAddress, 5);
        var dodoCode = Encoding.ASCII.GetString(bytesDodo);
        UI_Popup.CurrentInstance.CreatePopupChoice($"Current dodo code:\n{dodoCode}", "OK", () => { }, null, "Refetch", () => { FetchShowDodo(); });
    }
}
