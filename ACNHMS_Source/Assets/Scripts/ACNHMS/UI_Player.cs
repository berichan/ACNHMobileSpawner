using NHSE.Core;
using NHSE.Injection;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Player : MonoBehaviour
{
    public static UI_Player LastInstanceOfPlayer;

    public Text PlayerName;
    public RawImage PlayerText;

    // Start is called before the first frame update
    void Start()
    {
        LastInstanceOfPlayer = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // jpeg sizes are 100k-200k bytes so I gave up on this because the load time simply isn't worth it (around 15 seconds over usb)
    public void FetchPlayerJpeg(int playerNumber, IRAMReadWriter rw)
    {
        ulong address = OffsetHelper.getPlayerProfileMainAddress(SysBotController.CurrentOffsetFirstPlayerUInt) - 0xB8 + 0x10 + (OffsetHelper.PlayerSize * (ulong)playerNumber);
        int length = BitConverter.ToInt32(rw.ReadBytes((uint)address, 4), 0);
    }

    public static string[] FetchPlayerNames(IRAMReadWriter rw)
    {
        List<string> toRet = new List<string>();
        for (int i = 0; i < 8; ++i)
        {
            ulong address = OffsetHelper.getPlayerIdAddress(SysBotController.CurrentOffsetFirstPlayerUInt) - 0xB8 + 0x20 + (OffsetHelper.PlayerSize*(ulong)i);
            byte[] pName = rw.ReadBytes((uint)address, 20);
            string name = string.Empty;
            if (!isZeroArray(pName))
                name = StringUtil.GetString(pName, 0, 10);
            toRet.Add(name == string.Empty ? string.Format("No one ({0})", (char)((uint)'A' + i)) : name);
        }

        return toRet.ToArray();
    }

    public static string GetFirstPlayerTownName(IRAMReadWriter rw)
    {
        ulong address = OffsetHelper.getPlayerIdAddress(SysBotController.CurrentOffsetFirstPlayerUInt) - 0xB8 + 0x04;
        byte[] tName = rw.ReadBytes(address, 20);
        return StringUtil.GetString(tName, 0, 10);
    }

    private static bool isZeroArray(byte[] bytes) 
    {
        for (int i = 0; i < bytes.Length; ++i)
            if (bytes[i] != 0)
                return false;
        return true;
    }
}
