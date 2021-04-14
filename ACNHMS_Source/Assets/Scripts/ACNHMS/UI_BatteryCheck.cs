using UnityEngine;
using NHSE.Injection;
using System;
using UnityEngine.UI;

public class UI_BatteryCheck : MonoBehaviour
{
    public Image SliceImageMask;
    public Image BatteryJuiceImage;
    public Text PercentageText;

    private IRAMReadWriter Connection => UI_ACItemGrid.LastInstanceOfItemGrid.GetCurrentlyActiveReadWriter();
    private bool? CanCheckBattery = null;

    public void CheckVersion()
    {
        var ver = System.Text.Encoding.UTF8.GetString(Connection.GetVersion()).TrimEnd('\0').TrimEnd('\n');
        var verLower = ver.ToLower();
        var verDouble = double.TryParse(verLower, out var version);
        if (verDouble && version > 1.729)
        {
            CanCheckBattery = true;
        }
        else
        {
            PopupHelper.CreateError("Battery checks require sys-botbase version 1.73 or above.", 3f);
            CanCheckBattery = false;
        }
    }

    public void CheckBattery()
    {
        try
        {
            doBatteryCheckAndUpdateUI();
        }
        catch (Exception e)
        {
            PopupHelper.CreateError($"Unable to check battery percentage: {e.Message}", 3f);
        }
    }

    private void doBatteryCheckAndUpdateUI()
    {
        if (!CanCheckBattery.HasValue)
            CheckVersion();

        if (!CanCheckBattery.Value)
        {
            gameObject.SetActive(false);
            return;
        }

        var batteryString = System.Text.Encoding.UTF8.GetString(Connection.GetBattery()).TrimEnd('\0').TrimEnd('\n');
        var battery = int.Parse(batteryString);

        SliceImageMask.fillAmount = battery / 100f;
        BatteryJuiceImage.color = battery < 21 ? Color.red : Color.green;
        PercentageText.text = $"{battery}%";
    }
}
