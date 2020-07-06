using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NHSE.Core;
using NHSE.Injection;
using System.IO;
using NH_CreationEngine;
using System;

public class UI_Villager : IUI_Additional
{
    // sprites
    public static string VillagerFilename = "villagerdump";
    public static string VillagerFilenameHeader = VillagerFilename + "header";
    public static string VillagerPath { get { return SpriteBehaviour.UsableImagePath + Path.DirectorySeparatorChar + VillagerFilename; } }
    public static string VillagerHeaderPath { get { return SpriteBehaviour.UsableImagePath + Path.DirectorySeparatorChar + VillagerFilenameHeader; } }

    public static string VillagerRootAddress = OffsetHelper.VillagerAddress.ToString("X"); // ABA86BC4
    public static string VillagerHouseAddress = OffsetHelper.VillagerHouseAddress.ToString("X");
    public static uint CurrentVillagerAddress { get { return StringUtil.GetHexValue(VillagerRootAddress); } }
    public static uint CurrentVillagerHouseAddress { get { return StringUtil.GetHexValue(VillagerHouseAddress); } }

    public Text VillagerName, SaveVillagerLabel;
    public RawImage MainVillagerTexture;
    public InputField VillagerPhrase;
    public Toggle MovingOutToggle;
    public InputField VillagerRamOffset, VillagerHouseRamOffset;

    public RawImage[] TenVillagers;

    public GameObject BlockerRoot;

    public UI_VillagerSelect Selector;

    private Villager loadedVillager;
    private VillagerHouse loadedVillagerHouse;
    private SpriteParser villagerSprites;
    private bool loadedVillagerShells = false;
    private int currentlyLoadedVillagerIndex = -1;

    void Start()
    {
        checkAndLoadSpriteDump();
        VillagerRamOffset.text = VillagerRootAddress;
        VillagerHouseRamOffset.text = VillagerHouseAddress;

        for (int i = 0; i < TenVillagers.Length; ++i)
        {
            int tmpVal = i; // non indexed so it doesn't screw up 
            TenVillagers[i].GetComponent<Button>().onClick.AddListener(delegate { LoadVillager(tmpVal); });
        }

        VillagerPhrase.onValueChanged.AddListener(delegate { loadedVillager.CatchPhrase = VillagerPhrase.text; });
        MovingOutToggle.onValueChanged.AddListener(delegate { loadedVillager.MovingOut = MovingOutToggle.isOn; });

        VillagerRamOffset.onValueChanged.AddListener(delegate { VillagerRootAddress = VillagerRamOffset.text; });
        VillagerHouseRamOffset.onValueChanged.AddListener(delegate { VillagerHouseAddress = VillagerHouseRamOffset.text; });
    }

    private void loadAllVillagers()
    {
        for (int i = 0; i < 10; ++i)
        {
            byte[] loaded = CurrentConnection.ReadBytes(CurrentVillagerAddress + (uint)(i * Villager.SIZE), 3);
            Villager villagerShell = new Villager(loaded);
            Texture2D pic = SpriteBehaviour.PullTextureFromParser(villagerSprites, villagerShell.InternalName);
            if (pic != null)
                TenVillagers[i].texture = pic;
        }

        loadedVillagerShells = true;
        BlockerRoot.gameObject.SetActive(false);
    }

    public void LoadAllVillagers() // gets first 3 bytes of each villager
    {
        UI_Popup.CurrentInstance.CreatePopupMessage(0.001f, "Loading villagers...", () => { loadAllVillagers(); });
    }

    public void loadVillager(int index)
    {
        try
        {
            byte[] loaded = CurrentConnection.ReadBytes(CurrentVillagerAddress + (uint)(index * Villager.SIZE), Villager.SIZE);

            if (villagerIsNull(loaded))
                return;

            currentlyLoadedVillagerIndex = index;
            loadedVillager = new Villager(loaded);

            // get their house
            byte[] loadedHouse = CurrentConnection.ReadBytes(CurrentVillagerHouseAddress + (uint)(currentlyLoadedVillagerIndex * VillagerHouse.SIZE), VillagerHouse.SIZE);
            loadedVillagerHouse = new VillagerHouse(loadedHouse);

            VillagerToUI(loadedVillager);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            PopupHelper.CreateError(e.Message, 2f);
        }
    }

    public void LoadVillager(int index)
    {
        if (!loadedVillagerShells)
            return;

        UI_Popup.CurrentInstance.CreatePopupMessage(0.001f, "Loading villager...", () => { loadVillager(index); });
    }

    public void VillagerToUI(Villager v)
    {
        VillagerName.text = GameInfo.Strings.GetVillager(v.InternalName);
        VillagerPhrase.text = v.CatchPhrase;
        MainVillagerTexture.texture = SpriteBehaviour.PullTextureFromParser(villagerSprites, v.InternalName);
        MovingOutToggle.isOn = v.MovingOut;

        SaveVillagerLabel.text = string.Format("Save villager ({0})", VillagerName.text);
    }

    private void setCurrentVillager(bool includeHouse)
    {
        if (currentlyLoadedVillagerIndex == -1)
            return;

        try
        {
            byte[] villager = loadedVillager.Data;
            CurrentConnection.WriteBytes(villager, CurrentVillagerAddress + (uint)(currentlyLoadedVillagerIndex * Villager.SIZE));

            if (includeHouse)
                CurrentConnection.WriteBytes(loadedVillagerHouse.Data, CurrentVillagerHouseAddress + (uint)(currentlyLoadedVillagerIndex * VillagerHouse.SIZE));


            if (UI_ACItemGrid.LastInstanceOfItemGrid != null)
                UI_ACItemGrid.LastInstanceOfItemGrid.PlayHappyParticles();
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            PopupHelper.CreateError(e.Message, 2f);
        }
    }

    public void SetCurrentVillager()
    {
        UI_Popup.CurrentInstance.CreatePopupMessage(0.001f, "Saving villager...", () => { setCurrentVillager(true); });
    }

    public void RevertCurrentPhraseToOriginal()
    {
        if (currentlyLoadedVillagerIndex == -1)
            return;

        VillagerPhrase.text = loadedVillager.CatchPhrase = GameInfo.Strings.GetVillagerDefaultPhrase(loadedVillager.InternalName);
    }

    public void ShowSelector()
    {
        Selector.Init(() => { loadVillagerFromResource(); }, () => { resetVillagerSelection(); }, villagerSprites);
    }

    private void loadVillagerFromResource()
    {
        UI_Popup.CurrentInstance.CreatePopupMessage(0.001f, "Sending villager...", () => { loadVillagerData(); });
    }

    private void loadVillagerData()
    {
        try
        {
            string newVillager = Selector.LastSelectedVillager;
            byte[] villagerDump = ((TextAsset)Resources.Load("DefaultVillagers/" + newVillager + "V")).bytes;
            byte[] villagerHouse = ((TextAsset)Resources.Load("DefaultVillagers/" + newVillager + "H")).bytes;
            if (villagerDump == null || villagerHouse == null)
                throw new Exception("Villager not found: " + newVillager);

            Villager newV = new Villager(villagerDump);
            newV.SetMemories(loadedVillager.GetMemories());
            newV.CatchPhrase = GameInfo.Strings.GetVillagerDefaultPhrase(newVillager);
            VillagerHouse newVH = new VillagerHouse(villagerHouse);
            newVH.NPC1 = loadedVillagerHouse.NPC1;


            loadedVillager = newV;
            loadedVillagerHouse = newVH;

            SetCurrentVillager();
            TenVillagers[currentlyLoadedVillagerIndex].texture = SpriteBehaviour.PullTextureFromParser(villagerSprites, newVillager);
            VillagerToUI(loadedVillager);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            PopupHelper.CreateError(e.Message, 2f);
        }
    }

    private void resetVillagerSelection()
    {

    }

    private void checkAndLoadSpriteDump()
    {
        string dir = Path.GetDirectoryName(VillagerPath);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        if(!File.Exists(VillagerPath))
        {
            byte[] byteDump = ((TextAsset)Resources.Load("SpriteLoading/" + VillagerFilename)).bytes;
            byte[] byteHeader = ((TextAsset)Resources.Load("SpriteLoading/" + VillagerFilenameHeader)).bytes;
            File.WriteAllBytes(VillagerPath, byteDump);
            File.WriteAllBytes(VillagerHeaderPath, byteHeader);
        }

        villagerSprites = new SpriteParser(VillagerPath, VillagerHeaderPath);
    }

    private bool villagerIsNull(byte[] villager) // first 32 bytes will be 0
    {
        int maxCheck = Mathf.Min(villager.Length, 32);
        for (int i = 0; i < maxCheck; ++i)
            if (villager[i] != 0)
                return false;
        return true;
    }
}
