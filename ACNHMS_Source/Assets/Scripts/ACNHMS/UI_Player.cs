using NHSE.Core;
using NHSE.Injection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Player : MonoBehaviour
{
    

    public Text PlayerName;
    public RawImage PlayerText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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

    private static bool isZeroArray(byte[] bytes) 
    {
        for (int i = 0; i < bytes.Length; ++i)
            if (bytes[i] != 0)
                return false;
        return true;
    }
}
