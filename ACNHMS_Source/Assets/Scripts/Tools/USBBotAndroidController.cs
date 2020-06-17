using System.Collections;
using System.Collections.Generic;
using NHSE.Injection;
using System;

public class USBBotAndroidController
{
    public readonly USBBotAndroid Bot = new USBBotAndroid();

    public bool Connect()
    {
        try
        {
            return Bot.Connect();
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            AndroidUSBUtils.CurrentInstance.DebugToast(ex.Message);
            return false;
        }
    }

    public void WriteBytes(byte[] data, uint offset)
    {
        Bot.WriteBytes(data, offset);
        //SetOffset(offset);
    }

    public byte[] ReadBytes(uint offset, int length)
    {
        var result = Bot.ReadBytes(offset, length);
        //SetOffset(offset);
        return result;
    }
}
