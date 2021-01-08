using NHSE.Core;
using NHSE.Injection;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UI_ACItemGrid : MonoBehaviour
{
    public readonly int MAXITEMS = 40;

    public static UI_ACItemGrid LastInstanceOfItemGrid;
    // honestly this is bad but I'll clean it up later
#if PLATFORM_ANDROID
    public USBBotAndroid CurrentUSBBotAndroid { get { return usbac.Bot; } }
#endif
#if UNITY_STANDALONE || UNITY_EDITOR
    public USBBot CurrentUSBBot { get { return usbc.Bot; } }
#endif
    public SysBot CurrentSysBot { get { return sbc.Bot; } }

    public RectTransform SelectionOverlay;
    public UI_Sysbot UISB;
    public UI_ACItem PrefabItem;
    public UI_SearchWindow SearchWindow;
    public UI_SetControlFiller Filler;
    public ParticleSystem HappyParticles;

    [HideInInspector]
    public List<Item> Items = new List<Item>();

    private SysBotController sbc;
    private AutoInjector injector;

#if PLATFORM_ANDROID
    private USBBotAndroidController usbac;
    private AutoInjector usbaInjector;
#endif
#if UNITY_STANDALONE || UNITY_EDITOR
    private USBBotController usbc;
    private AutoInjector usbInjector;
#endif

    private List<UI_ACItem> uiitems;

    public int CurrentSelected { get; private set; }

    private Coroutine currentAnimationFuction;

    private void Start()
    {
        //IL_00b4: Unknown result type (might be due to invalid IL or missing references)
        //IL_00cb: Unknown result type (might be due to invalid IL or missing references)
        //IL_00f8: Unknown result type (might be due to invalid IL or missing references)
        //IL_0102: Expected O, but got Unknown
        sbc = new SysBotController(InjectionType.Pouch);
        UISB.AssignSysbot(sbc);
#if PLATFORM_ANDROID
        usbac = new USBBotAndroidController();
        UISB.AssignUSBBotAndroid(usbac);
#endif
#if UNITY_STANDALONE || UNITY_EDITOR
        usbc = new USBBotController();
        UISB.AssignUSBBot(usbc);
#endif
        for (int i = 0; i < MAXITEMS; i++)
        {
            Items.Add(new Item(Item.NONE));
        }

        uiitems = new List<UI_ACItem>(GetComponentsInChildren<UI_ACItem>());
        List<UI_ACItem> list = new List<UI_ACItem>();
        int num = 0;
        foreach (UI_ACItem uiitem in uiitems)
        {
            GameObject obj = Instantiate(PrefabItem.gameObject);
            obj.transform.SetParent(transform);
            obj.transform.position = uiitem.transform.position;
            obj.transform.localScale = uiitem.transform.localScale;
            UI_ACItem component = obj.GetComponent<UI_ACItem>();
            int currentIndex = num;
            component.ButtonComponent.onClick.AddListener(delegate
            {
                SetSelection(currentIndex);
            });
            list.Add(component);
            uiitem.gameObject.SetActive(false);
            num++;
        }
        uiitems = list;

        PrefabItem.gameObject.SetActive(false);
        set(Items.ToArray());

        PocketInjector inj = new PocketInjector(Items, sbc.Bot);
        injector = new AutoInjector(inj, AfterRead, AfterWrite);
#if PLATFORM_ANDROID
        PocketInjector usbaInj = new PocketInjector(Items, usbac.Bot);
        usbaInjector = new AutoInjector(usbaInj, AfterRead, AfterWrite);
#endif
#if UNITY_STANDALONE || UNITY_EDITOR
        PocketInjector usbInj = new PocketInjector(Items, usbc.Bot);
        usbInjector = new AutoInjector(usbInj, AfterRead, AfterWrite);
#endif

        SetSelection(0);
        LastInstanceOfItemGrid = this;
    }

    public void SetSelection(int itemIndex)
    {
        CurrentSelected = itemIndex;
        Item itemAssigned = uiitems[CurrentSelected].ItemAssigned;
        Filler.UpdateSelected(itemIndex, itemAssigned);
        if (!itemAssigned.IsNone)
        {
            SearchWindow.LoadItem(itemAssigned);
        }
        if (currentAnimationFuction != null)
        {
            StopCoroutine(currentAnimationFuction);
        }
        currentAnimationFuction = StartCoroutine(sendSelectorToSelected());
    }

    public void DeleteRow(int row)
    {
        int start = row * 10;
        for (int i = start; i < start + 10; ++i)
        {
            Items[i].Delete();
            uiitems[i].Assign(Items[i]);
        }
    }

    public void DeleteRow(Item indexOfItem) => DeleteRow(Items.IndexOf(indexOfItem) / 10);

    public void PlayHappyParticles()
    {
        HappyParticles.gameObject.SetActive(true);
        HappyParticles.Stop();
        HappyParticles.Play();
    }

    private IEnumerator sendSelectorToSelected(float time = 0.25f)
    {
        float i = 0f;
        float rate = 1f / time;
        Vector3 startPos = SelectionOverlay.transform.position;
        Vector3 endPos = uiitems[CurrentSelected].transform.position;
        while (i < 1f)
        {
            i += Time.deltaTime * rate;
            Vector3 position = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0f, 1f, i));
            SelectionOverlay.transform.position = (position);
            yield return null;
        }
    }

    private void AfterRead(InjectionResult r)
    {
        if (r == InjectionResult.Success)
        {
            set(Items.ToArray());
        }
        else
        {
            Debug.LogError(r.ToString());
            if (r != InjectionResult.Same)
                PopupHelper.CreateError(r.ToString(), 2f);
        }
    }

    private static void AfterWrite(InjectionResult r)
    {
        Debug.Log($"Write result: {r}");
        if (r != InjectionResult.Success)
            if (r != InjectionResult.Same)
                PopupHelper.CreateError($"Write result: {r}", 2f);
    }

    private void set(Item[] items)
    {
        for (int i = 0; i < items.Length; i++)
        {
            uiitems[i].Assign(items[i]);
        }
    }
    
    public void SetItemAt(Item it, int index, bool setFocus)
    {
        uiitems[index].Assign(it);
        if (setFocus)
        {
            SetSelection(index);
        }
    }

    public Item GetItemAt(int index)
    {
        return uiitems[index].ItemAssigned;
    }

    public void ResetAllItems()
    {
        for (int i = 0; i < uiitems.Count; ++i)
        {
            uiitems[i].Assign(Items[i]);
        }
    }

    public void ReadFromSource()
    {
        uint offset = sbc.GetDefaultOffset();
        injector.SetWriteOffset(offset);
        injector.ValidateEnabled = UI_Settings.GetValidateData();
        try
        {
            InjectionResult injectionResult = injector.Read(true);
            if (injectionResult != InjectionResult.Success)
            {
                Debug.Log(injectionResult.ToString());
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
            UISB.ConnectedText.text = (ex.Message);
            UISB.SetConnected(val: false);
        }
    }

    public void WriteToSource()
    {
        uint offset = sbc.GetDefaultOffset();
        injector.SetWriteOffset(offset);
        injector.ValidateEnabled = UI_Settings.GetValidateData();
        try
        {
            InjectionResult injectionResult = injector.Write(true);
            if (injectionResult == InjectionResult.Success)
            {
                PlayHappyParticles();
            }
            else
            {
                Debug.Log(injectionResult.ToString());
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
            UISB.ConnectedText.text = (ex.Message);
            UISB.SetConnected(val: false);
        }
    }

#if PLATFORM_ANDROID || UNITY_STANDALONE || UNITY_EDITOR
    public void ReadFromSourceUSBA()
    {
        uint offset = sbc.GetDefaultOffset();
        AutoInjector platformInjector;
#if UNITY_STANDALONE || UNITY_EDITOR
        platformInjector = usbInjector;
#else
        platformInjector = usbaInjector;
#endif
        platformInjector.SetWriteOffset(offset);
        platformInjector.ValidateEnabled = UI_Settings.GetValidateData();
        try
        {
            InjectionResult injectionResult = platformInjector.Read(true);
            if (injectionResult != InjectionResult.Success)
            {
                Debug.Log(injectionResult.ToString());
#if UNITY_STANDALONE || UNITY_EDITOR
                PopupHelper.CreateError(injectionResult.ToString(), 2f);
#endif
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
            UISB.ConnectedText.text = (ex.Message);
            UISB.SetConnected(false);
#if UNITY_STANDALONE || UNITY_EDITOR
            PopupHelper.CreateError(ex.Message, 2f);
#endif
        }
    }

    public void WriteToSourceUSBA()
    {
        uint offset = sbc.GetDefaultOffset();
        AutoInjector platformInjector;
#if UNITY_STANDALONE || UNITY_EDITOR
        platformInjector = usbInjector;
#else
        platformInjector = usbaInjector;
#endif
        platformInjector.SetWriteOffset(offset);
        platformInjector.ValidateEnabled = UI_Settings.GetValidateData();
        try
        {
            InjectionResult injectionResult = platformInjector.Write(true);
            if (injectionResult == InjectionResult.Success)
            {
                PlayHappyParticles();
            }
            else
            {
                Debug.Log(injectionResult.ToString());
#if UNITY_STANDALONE || UNITY_EDITOR
                PopupHelper.CreateError(injectionResult.ToString(), 2f);
#endif
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
            UISB.ConnectedText.text = (ex.Message);
            UISB.SetConnected(false);
#if UNITY_STANDALONE || UNITY_EDITOR
            PopupHelper.CreateError(ex.Message, 2f);
#endif
        }
    }
#endif

        /// <summary>
        /// Returns null if settings selected bot is active
        /// </summary>
        /// <returns>Active readwriter</returns>
        public IRAMReadWriter GetCurrentlyActiveReadWriter()
    {
        InjectionProtocol currentIP = UI_Settings.GetInjectionProtocol();
        IRAMReadWriter toRet = null;
        switch (currentIP)
        {
            case InjectionProtocol.Sysbot:
                if (CurrentSysBot.Connected)
                    toRet = CurrentSysBot;
                break;
            case InjectionProtocol.UsbBot:
#if UNITY_STANDALONE || UNITY_EDITOR
                if (CurrentUSBBot.Connected)
                    toRet = CurrentUSBBot;
#elif PLATFORM_ANDROID
                if (CurrentUSBBotAndroid.Connected)
                    toRet = CurrentUSBBotAndroid;
#endif
                break;
        }
        return toRet;
    }
}
