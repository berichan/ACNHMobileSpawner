using System;
using NHSE.Injection;
using NHSE.Core;
using UnityEngine;
using System.Net.Sockets;

public class SysBotController
{
    public static string CurrentOffset = OffsetHelper.InventoryOffset.ToString("X");// "ABA526A8";
    public static uint CurrentOffsetFirstPlayerUInt { get => StringUtil.GetHexValue(CurrentOffset); }

    public SysBotController(InjectionType type) => Type = type;

    private readonly InjectionType Type;
    private uint OffsetValue;
    public readonly SysBot Bot = new SysBot();

    public string IP;
    public string Port;

    public bool Connect(string ip, string port, out string error)
    {
        if (!int.TryParse(port, out var p))
            p = 6000;

        try
        {
            Bot.Connect(ip, p);
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            Debug.LogError(ex.Message);
            error = ex.Message;
            if (ex is SocketException)
                UI_Popup.CurrentInstance.CreatePopupChoice($"Error: {((SocketException)ex).ErrorCode.ToString()}. \r\n This app could not connect to Sys-botbase running on your console. This is likely because sys-botbase is not installed correctly, a vpn or firewall is stopping the connection, or you aren't on the same network. Please run the python tester script on the Sys-botbase github page to ensure it is working.", "OK", () => { }, Color.red, "Check local IPs", () => { NetworkInfoUnity.TryCreatePingObject(ip); }); 
            return false;
        }

        error = "";
        return true;
    }

    public uint GetDefaultOffset()
    {
        var toRet = StringUtil.GetHexValue(CurrentOffset) + ((ulong)UI_Settings.GetPlayerIndex() * OffsetHelper.PlayerSize);
        return (uint)toRet;
    }

    public void SetOffset(uint value)
    {
        CurrentOffset = value.ToString("X");
    }
        

    public void WriteBytes(byte[] data, uint offset)
    {
        Bot.WriteBytes(data, offset);
        SetOffset(offset);
    }

    public byte[] ReadBytes(uint offset, int length)
    {
        var result = Bot.ReadBytes(offset, length);
        SetOffset(offset);
        return result;
    }
}
