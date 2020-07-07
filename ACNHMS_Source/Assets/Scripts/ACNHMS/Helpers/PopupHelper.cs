using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupHelper
{
    public static void CreateError(string message, float length, bool forceSpawnerPopup = false)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!forceSpawnerPopup)
            AndroidUSBUtils.CurrentInstance.DebugToast(message);
        else
            UI_Popup.CurrentInstance.CreatePopupMessage(length, "Error: " + message, null, Color.red);
#else
        UI_Popup.CurrentInstance.CreatePopupMessage(length, "Error: "+message, null, Color.red);
#endif
    }
}
