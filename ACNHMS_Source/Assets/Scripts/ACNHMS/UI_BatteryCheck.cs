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
        var batteryString = System.Text.Encoding.UTF8.GetString(Connection.GetBattery()).TrimEnd('\0').TrimEnd('\n');
        var battery = int.Parse(batteryString);

        SliceImageMask.fillAmount = battery / 100f;
        BatteryJuiceImage.color = battery < 21 ? Color.red : Color.green;
        PercentageText.text = $"{battery}%";
    }
}
