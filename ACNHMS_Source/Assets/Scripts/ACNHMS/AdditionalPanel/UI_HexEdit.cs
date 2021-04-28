using NHSE.Core;
using UnityEngine.UI;
using UnityEngine;
using System;

public class UI_HexEdit : IUI_Additional
{
    public static string HEXADDRESSKEY = "HEXBKEY";
    public static string ValueAddress = SysBotController.CurrentOffset;
    public static uint CurrentAddress { get { return StringUtil.GetHexValue(ValueAddress); } }

    public HexEditorBeri HexEdit;
    public InputField RamOffset, ByteLength;
    public Toggle IsMain;

    // Start is called before the first frame update
    void Start()
    {
        RamOffset.text = ValueAddress = getAddress(ValueAddress);

        RamOffset.onValueChanged.AddListener(delegate { ValueAddress = RamOffset.text; });
    }

    public void BeginHexEdit()
    {
        UI_Popup.CurrentInstance.CreatePopupMessage(0.001f, "Fetching bytes...", () => { beginHexEdit(); });
    }

    private void beginHexEdit()
    {
        try
        {
            int byteLengthToGet = int.Parse(ByteLength.text);
            byte[] bytesFetched = CurrentConnection.ReadBytes(CurrentAddress, byteLengthToGet, IsMain.isOn ? NHSE.Injection.RWMethod.Main : NHSE.Injection.RWMethod.Heap);
            setAddress(ValueAddress);
            UI_Popup.CurrentInstance.CreatePopupMessage(0.001f, "Bytes received. Initilializing hex editor...", () => {
                HexEdit.InitialiseWithBytes(bytesFetched, "Send bytes", PushBytes);
            });
        }
        catch (Exception e)
        {
            PopupHelper.CreateError(e.Message, 2f);
        }
    }

    private void pushBytes(byte[] bytes)
    {
        try
        {
            CurrentConnection.WriteBytes(bytes, CurrentAddress, IsMain.isOn ? NHSE.Injection.RWMethod.Main : NHSE.Injection.RWMethod.Heap);

            if (UI_ACItemGrid.LastInstanceOfItemGrid != null)
                UI_ACItemGrid.LastInstanceOfItemGrid.PlayHappyParticles();
        }
        catch (Exception e)
        {
            PopupHelper.CreateError(e.Message, 2f);
        }
    }

    public void PushBytes(byte[] bytes)
    {
        UI_Popup.CurrentInstance.CreatePopupMessage(0.01f, "Sending bytes to console...", () => {
            pushBytes(bytes);
        });
    }

    public void TryChangeValueOffset(int addsub)
    {
        try
        {
            int nVal = Math.Max(0, (int)CurrentAddress + addsub);
            string nValStr = nVal.ToString("X");
            setAddress(nValStr);
            RamOffset.text = ValueAddress = nValStr;
        }
        catch (Exception e)
        {
            PopupHelper.CreateError(e.Message, 2f);
        }
    }

    public void TryChangeValueLength(int addsub)
    {
        try
        {
            int byteLengthToGet = int.Parse(ByteLength.text);
            byteLengthToGet = Mathf.Clamp(byteLengthToGet + addsub, 0, 9999);
            ByteLength.text = byteLengthToGet.ToString();
        }
        catch (Exception e)
        {
            PopupHelper.CreateError(e.Message, 2f);
        }
    }

    private void setAddress(string newAddress) => PlayerPrefs.SetString(HEXADDRESSKEY + Application.version, newAddress);
    private string getAddress(string defaultOffset) => PlayerPrefs.GetString(HEXADDRESSKEY + Application.version, defaultOffset);
    
}
