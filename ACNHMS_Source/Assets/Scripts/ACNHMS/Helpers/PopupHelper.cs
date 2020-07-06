using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupHelper
{
    public static void CreateError(string message, float length)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidUSBUtils.CurrentInstance.DebugToast(message);
#else
        UI_Popup.CurrentInstance.CreatePopupMessage(length, "Error: "+message, null, Color.red);
#endif
    }
}
