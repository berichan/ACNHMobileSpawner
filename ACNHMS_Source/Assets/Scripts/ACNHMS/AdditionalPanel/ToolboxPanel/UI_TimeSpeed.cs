using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NHSE.Core;
using System;
using System.Threading;

public class UI_TimeSpeed : IUI_Additional
{
    public readonly Dictionary<int, uint> WalkSteps = new Dictionary<int, uint>()
    {
        { 0, 0xBD578661 },
        { 1, 0x1E201001 },
        { 2, 0x1E211001 },
        { 3, 0x1E221001 },
    };

    public readonly Dictionary<int, uint> SpeedSteps = new Dictionary<int, uint>()
    {
        { 0, 0x3F800000 },
        { 1, 0x40000000 },
        { 2, 0x40A00000 },
        { 3, 0x42480000 },
    };

    public const uint FreezeTimeValue = 0xD503201F;
    public const uint UnFreezeTimeValue = 0xF9203260;

    public const uint CollisionOnValue = 0xB95BA014;
    public const uint CollisionOffValue = 0x12800014;

    public InputField Year, Month, Day, Hour, Minute;
    public GameObject Blocker;

    private TimeBlock CurrentTime;

    // Start is called before the first frame update
    void Start()
    {
        CurrentTime = new TimeBlock();
    }

    private void OnEnable()
    {
        StartCoroutine(checkNullNextFrame());
    }

    IEnumerator checkNullNextFrame()
    {
        yield return new WaitForEndOfFrame();
        if (CurrentConnection != null)
        {
            Blocker.SetActive(!CurrentConnection.Connected);
            FetchTime();
        }
    }

    public void SetCollision(bool on)
    {
        try
        {
            var nVal = on ? CollisionOnValue : CollisionOffValue;
            CurrentConnection.WriteBytes(BitConverter.GetBytes(nVal), OffsetHelper.CollisionStateOffset, NHSE.Injection.RWMethod.Main);
        }
        catch (Exception e) { PopupHelper.CreateError("Setting collision state failed: " + e.Message, 3f); }
    }

    public void SetWalkSpeed(int step)
    {
        try
        {
            var nVal = WalkSteps[step];
            CurrentConnection.WriteBytes(BitConverter.GetBytes(nVal), OffsetHelper.WalkSpeedOffset, NHSE.Injection.RWMethod.Main);
        }
        catch (Exception e) { PopupHelper.CreateError("Setting walk speed failed: " + e.Message, 3f); }
    }

    public void SetAnimSpeed(int step)
    {
        try
        {
            var nVal = SpeedSteps[step];
            CurrentConnection.WriteBytes(BitConverter.GetBytes(nVal), OffsetHelper.AnimationSpeedOffset, NHSE.Injection.RWMethod.Main);
        }
        catch (Exception e) { PopupHelper.CreateError("Setting animation speed failed: " + e.Message, 3f); }
    }

    public void SetTextSpeed(int speed)
    {
        byte spd = (byte)speed;
        try
        {
            if (spd != 0)
                CurrentConnection.FreezeBytes(new byte[1] { spd }, (uint)OffsetHelper.TextSpeedAddress);
            else
                CurrentConnection.UnFreezeBytes((uint)OffsetHelper.TextSpeedAddress);
        }
        catch (Exception e) { PopupHelper.CreateError("Setting text speed failed: " + e.Message, 3f); }
    }

    public void SetTimeFreeze(bool freeze)
    {
        if (freeze)
        {
            CurrentConnection.WriteBytes(BitConverter.GetBytes(FreezeTimeValue), OffsetHelper.TimeStateAddress, NHSE.Injection.RWMethod.Main);
        }
        else
        {
            CurrentConnection.WriteBytes(BitConverter.GetBytes(UnFreezeTimeValue), OffsetHelper.TimeStateAddress, NHSE.Injection.RWMethod.Main);
            FetchTime();
        }
    }

    public void SetTime()
    {
        byte f = 0;
        if (ushort.TryParse(Year.text, out var yr))
            CurrentTime.Year = yr;

        if (byte.TryParse(Month.text, out f))
            CurrentTime.Month = f;
        if (byte.TryParse(Day.text, out f))
            CurrentTime.Day = f;
        if (byte.TryParse(Hour.text, out f))
            CurrentTime.Hour = f;
        if (byte.TryParse(Minute.text, out f))
            CurrentTime.Minute = f;

        var b = CurrentTime.ToBytesClass();
        SetTimeFreeze(true);
        CurrentConnection.WriteBytes(b, OffsetHelper.TimeAddress);
        //Thread.Sleep(100);
        //SetTimeFreeze(false);
    }

    public void FetchTime()
    {
        var b = CurrentConnection.ReadBytes(OffsetHelper.TimeAddress, TimeBlock.SIZE);
        CurrentTime = b.ToClass<TimeBlock>();
        TimeToUI();
    }

    public void SkipDay() => SkipTime(1d);
    public void SkipHour() => SkipTime(1d/24d);

    private void SkipTime(double days)
    {
        DateTime now = new DateTime((int)CurrentTime.Year, (int)CurrentTime.Month, (int)CurrentTime.Day, (int) CurrentTime.Hour, (int) CurrentTime.Minute, 0);
        now = now.AddDays(days);
        CurrentTime.Year = (ushort)now.Year;
        CurrentTime.Month = (byte)now.Month;
        CurrentTime.Day = (byte)now.Day;
        CurrentTime.Hour = (byte)now.Hour;
        CurrentTime.Minute = (byte)now.Minute;
        TimeToUI();
        SetTime();
    }

    private void TimeToUI()
    {
        Year.text = CurrentTime.Year.ToString();
        Month.text = CurrentTime.Month.ToString();
        Day.text = CurrentTime.Day.ToString();
        Hour.text = CurrentTime.Hour.ToString();
        Minute.text = CurrentTime.Minute.ToString();
    }
}
