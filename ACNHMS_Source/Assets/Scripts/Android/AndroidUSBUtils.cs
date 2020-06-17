using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

public class AndroidUSBUtils
{
    private static AndroidJavaClass unityPlayer;
    private static AndroidJavaObject activity;
    private static AndroidJavaObject context;
    private static AndroidJavaObject usbClass;

    private static AndroidUSBUtils currentInstance;
    public static AndroidUSBUtils CurrentInstance
    {
        get
        {
            if (currentInstance == null)
                currentInstance = new AndroidUSBUtils();
            return currentInstance;
        }
    }

    public AndroidUSBUtils()
    {
        unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        context = activity.Call<AndroidJavaObject>("getApplicationContext");

        usbClass = new AndroidJavaObject("com.berichan.usbunitylibrary.USBUtil");
        usbClass.Call("Init", (object)context);
    }

    public void ConnectUSB()
    {
        usbClass.Call("ConnectUSB");
    }

    public void WriteToEndpoint(byte[] buffer)
    {
        uint pack = (uint)buffer.Length + 2;
        byte[] byteCount = BitConverter.GetBytes(pack);
        usbClass.Call("SendData", (object)byteCount, (object)buffer);
    }

    public void DebugToast(string message)
    {
        if (message == "")
            return;
        object[] param = new object[2];
        param[0] = context;
        param[1] = message;

        usbClass.Call("CreateToast", param);
    }
}
