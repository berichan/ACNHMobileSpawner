using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NHSE.Core;
using NHSE.Injection;
using UnityEngine.UI;

public class UI_Villager : IUI_Additional
{
    public static string VillagerRootAddress = OffsetHelper.VillagerAddress.ToString("X"); // ABA86BC4
    public static uint CurrentVillagerAddress { get { return StringUtil.GetHexValue(VillagerRootAddress); } }

    Villager loadedVillager;

    public InputField VillagerName, VillagerPhrase;
    public InputField VillagerRamOffset;

    void Start()
    {
        VillagerRamOffset.text = VillagerRootAddress;
    }

    public void LoadVillager()
    {
        SysBot current = (SysBot)CurrentConnection;
        byte[] loaded = current.ReadBytesLarge(CurrentVillagerAddress, Villager.SIZE);
        loadedVillager = new Villager(loaded);
        VillagerName.text = loadedVillager.InternalName;
        VillagerPhrase.text = loadedVillager.CatchPhrase;
    }
}
