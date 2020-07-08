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
    public Toggle MovingOutToggle, ReloadVillagerToggle;
    public InputField VillagerRamOffset, VillagerHouseRamOffset;
    public Button DataButton;

    public RawImage[] TenVillagers;

    public GameObject BlockerRoot;

    public UI_VillagerSelect Selector;
    public UI_VillagerData DataSelector;

    private Villager loadedVillager;
    private List<VillagerHouse> loadedVillagerHouses;
    private List<Villager> loadedVillagerShellsList;
    private SpriteParser villagerSprites;
    private bool loadedVillagerShells = false;
    private int currentlyLoadedVillagerIndex = -1;

    public VillagerHouse GetCurrentLoadedVillagerHouse() => loadedVillagerHouses?.Find(x => x.NPC1 == (sbyte)currentlyLoadedVillagerIndex);

    public Villager GetCurrentlyLoadedVillager() => loadedVillager;

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

        DataButton.interactable = false;
    }

    private void loadAllVillagers()
    {
        try
        {
            // load all houses
            loadAllHouses();

            loadedVillagerShellsList = new List<Villager>();
            for (int i = 0; i < 10; ++i)
            {
                byte[] loaded = CurrentConnection.ReadBytes(CurrentVillagerAddress + (uint)(i * Villager.SIZE), 3);
                Villager villagerShell = new Villager(loaded);
                loadedVillagerShellsList.Add(villagerShell);
                if (villagerShell.Species == (byte)VillagerSpecies.non)
                {
                    TenVillagers[i].GetComponent<Button>().interactable = false;
                    continue;
                }
                else
                    TenVillagers[i].GetComponent<Button>().interactable = true;

                var ourHouse = loadedVillagerHouses.Find(x => x.NPC1 == (sbyte)i);
                if (ourHouse != null)
                    if (checkIfMovingIn(ourHouse))
                        TenVillagers[i].GetComponent<Button>().interactable = false; // but still show them

                Texture2D pic = SpriteBehaviour.PullTextureFromParser(villagerSprites, villagerShell.InternalName);
                if (pic != null)
                    TenVillagers[i].texture = pic;
            }

            loadedVillagerShells = true;
            BlockerRoot.gameObject.SetActive(false);
        }
        catch (Exception e)
        {
            PopupHelper.CreateError(e.Message, 2f);
        }
    }

    private void loadAllHouses()
    {
        loadedVillagerHouses = new List<VillagerHouse>();
        byte[] houses = CurrentConnection.ReadBytes(CurrentVillagerHouseAddress, VillagerHouse.SIZE * 10);
        for (int i = 0; i < 10; ++i)
        {
            loadedVillagerHouses.Add(new VillagerHouse(houses.Slice(i * VillagerHouse.SIZE, VillagerHouse.SIZE)));
        }
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

            // reload all houses
            loadAllHouses();

            currentlyLoadedVillagerIndex = index;
            loadedVillager = new Villager(loaded);

            VillagerToUI(loadedVillager);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            PopupHelper.CreateError(e.Message, 2f);
        }
    }

    private Villager loadVillagerExternal(int index, bool includeHouses)
    {
        try
        {
            byte[] loaded = CurrentConnection.ReadBytes(CurrentVillagerAddress + (uint)(index * Villager.SIZE), Villager.SIZE);

            if (villagerIsNull(loaded))
                return null;

            // reload all houses
            if (includeHouses)
                loadAllHouses();
            
            return new Villager(loaded);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            PopupHelper.CreateError(e.Message, 2f);
            return null;
        }
    }

    public void LoadVillager(int index)
    {
        if (!loadedVillagerShells)
            return;

        UI_Popup.CurrentInstance.CreatePopupMessage(0.001f, 
            string.Format("Fetching {0}...", GameInfo.Strings.GetVillager(loadedVillagerShellsList[index].InternalName)), 
            () => { loadVillager(index); }, 
            loadedVillagerShellsList[index].Gender == 0 ? new Color (0.15f, 0.46f, 1f) : new Color (1f, 0.51f, 0.75f), // blue or pink
            false,
            (Texture2D)TenVillagers[index].texture); 
    }

    public void VillagerToUI(Villager v)
    {
        VillagerName.text = GameInfo.Strings.GetVillager(v.InternalName);
        VillagerPhrase.text = v.CatchPhrase;
        MainVillagerTexture.texture = SpriteBehaviour.PullTextureFromParser(villagerSprites, v.InternalName);
        MovingOutToggle.isOn = v.MovingOut;

        SaveVillagerLabel.text = string.Format("Save villager ({0})", VillagerName.text);

        DataButton.interactable = true;
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
            {
                // send all houses
                List<byte> linearHouseArray = new List<byte>();
                foreach (VillagerHouse vh in loadedVillagerHouses)
                    linearHouseArray.AddRange(vh.Data);
                CurrentConnection.WriteBytes(linearHouseArray.ToArray(), CurrentVillagerHouseAddress);
                CurrentConnection.WriteBytes(linearHouseArray.ToArray(), CurrentVillagerHouseAddress + (uint)OffsetHelper.VillagerHouseBufferDiff); // there's a temporary day buffer
            }


            if (UI_ACItemGrid.LastInstanceOfItemGrid != null)
                UI_ACItemGrid.LastInstanceOfItemGrid.PlayHappyParticles();
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            PopupHelper.CreateError(e.Message, 2f);
        }
    }

    public void SetCurrentVillagerWithCheck()
    {
        if (currentlyLoadedVillagerIndex == -1)
        {
            PopupHelper.CreateError("No villager selected. Select a villager from the left-hand panel.", 2f);
            return;
        }
        checkReloadVillager();
        setCurrentVillager();
    }

    private void setCurrentVillager()
    {
        UI_Popup.CurrentInstance.CreatePopupMessage(0.001f,
            string.Format("Saving {0}...", GameInfo.Strings.GetVillager(loadedVillagerShellsList[currentlyLoadedVillagerIndex].InternalName)), 
            () => { setCurrentVillager(true); },
            loadedVillagerShellsList[currentlyLoadedVillagerIndex].Gender == 0 ? new Color(0.15f, 0.46f, 1f) : new Color(1f, 0.51f, 0.75f), // blue or pink
            false,
            (Texture2D)TenVillagers[currentlyLoadedVillagerIndex].texture);
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

    public void ShowDataSelector()
    {
        DataSelector.gameObject.SetActive(true);
        DataSelector.VillagerName.text = VillagerName.text;
        DataSelector.VillagerImg.texture = (Texture2D)MainVillagerTexture.texture;
    }

    private void loadVillagerFromResource()
    {
        if (currentlyLoadedVillagerIndex == -1)
        {
            PopupHelper.CreateError("No villager selected to replace.", 2f);
            return;
        }
        UI_Popup.CurrentInstance.CreatePopupMessage(0.001f,
            string.Format("Sending {0}...", GameInfo.Strings.GetVillager(loadedVillagerShellsList[currentlyLoadedVillagerIndex].InternalName)), 
            () => { loadVillagerDataFromSelector(); },
            null,
            false,
            (Texture2D)TenVillagers[currentlyLoadedVillagerIndex].texture);
    }

    private void loadVillagerDataFromSelector()
    {
        try
        {
            checkReloadVillager();
            string newVillager = Selector.LastSelectedVillager;
            byte[] villagerDump = ((TextAsset)Resources.Load("DefaultVillagers/" + newVillager + "V")).bytes;
            byte[] villagerHouse = ((TextAsset)Resources.Load("DefaultVillagers/" + newVillager + "H")).bytes;
            if (villagerDump == null || villagerHouse == null)
                throw new Exception("Villager not found: " + newVillager);

            loadVillagerData(new Villager(villagerDump), new VillagerHouse(villagerHouse));
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            PopupHelper.CreateError(e.Message, 2f);
        }
    }

    private void loadVillagerData(Villager v, VillagerHouse vh, bool raw = false)
    {
        try
        {
            Villager newV = v;
            VillagerHouse newVH = vh;
            if (!raw)
            {
                newV.SetMemories(loadedVillager.GetMemories());
                newV.SetEventFlagsSave(loadedVillager.GetEventFlagsSave());
                newV.CatchPhrase = GameInfo.Strings.GetVillagerDefaultPhrase(newV.InternalName);
            }

            VillagerHouse loadedVillagerHouse = GetCurrentLoadedVillagerHouse(); // non indexed so search for the correct one
            int index = loadedVillagerHouses.IndexOf(loadedVillagerHouse);
            if (index == -1)
                throw new Exception("The villager being replaced doesn't have a house on your island.");

            // check if they are moving in
            if (checkIfMovingIn(loadedVillagerHouse))
                newVH = combineHouseOrders(newVH, loadedVillagerHouse);
            newVH.NPC1 = loadedVillagerHouse.NPC1;

            loadedVillagerHouses[index] = newVH;
            loadedVillager = newV;
            loadedVillagerShellsList[currentlyLoadedVillagerIndex] = newV;

            TenVillagers[currentlyLoadedVillagerIndex].texture = SpriteBehaviour.PullTextureFromParser(villagerSprites, newV.InternalName);
            setCurrentVillager(); // where the magic happens
            VillagerToUI(loadedVillager);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            PopupHelper.CreateError(e.Message, 2f);
        }
    }

    private bool checkIfMovingIn(VillagerHouse vOld) => vOld.WallUniqueID == WallType.HouseWallNSoldOut;

    private VillagerHouse combineHouseOrders(VillagerHouse vNew, VillagerHouse vOld)
    {
        VillagerHouse vTmp = new VillagerHouse(vOld.Data);
        vTmp.OrderWallUniqueID = vNew.OrderWallUniqueID;
        vTmp.OrderRoofUniqueID = vNew.OrderRoofUniqueID;
        vTmp.OrderDoorUniqueID = vNew.OrderDoorUniqueID;
        return vTmp;
    }

    private void resetVillagerSelection()
    {

    }

    private void checkReloadVillager()
    {
        if (ReloadVillagerToggle.isOn)
        {
            Villager v = loadVillagerExternal(currentlyLoadedVillagerIndex, true);
            if (v != null)
            {
                loadedVillager.SetMemories(v.GetMemories());
                loadedVillager.SetEventFlagsSave(v.GetEventFlagsSave());
                loadedVillager.MovingOut = v.MovingOut;
                loadedVillager.CatchPhrase = v.CatchPhrase;
            }
        }

    }

    // villager data

    public void WriteVillagerDataHouse(VillagerHouse vh)
    {
        checkReloadVillager();
        loadVillagerData(loadedVillager, vh, true);
    }

    public void WriteVillagerDataVillager(Villager v)
    {
        checkReloadVillager();
        VillagerHouse loadedVillagerHouse = loadedVillagerHouses.Find(x => x.NPC1 == (sbyte)currentlyLoadedVillagerIndex); // non indexed so search for the correct one
        int index = loadedVillagerHouses.IndexOf(loadedVillagerHouse);
        if (index == -1)
            throw new Exception("The villager having their house replaced doesn't have a house on your island."); // not sure why but it can get unloaded during the check

        loadVillagerData(v, loadedVillagerHouse, true);
    }

    // tools

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
